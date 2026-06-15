// Copyright (c) Jordan Molenaar (jordanm.cloud). All rights reserved.

using jordanm.cloud.mosyle.Clients.Mosyle;
using jordanm.cloud.mosyle.Clients.SnipeIt;
using jordanm.cloud.mosyle.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SnipeModel = jordanm.cloud.mosyle.Clients.SnipeIt.Model;

namespace jordanm.cloud.mosyle.Sync;

/// <summary>
/// Synchronizes Mosyle devices into Snipe-IT assets.
/// </summary>
/// <remarks>
/// This service owns orchestration and comparison decisions only. API transport
/// details stay behind <see cref="IMosyleClient"/> and <see cref="ISnipeItClient"/>,
/// which keeps this class unit-testable without real credentials.
/// </remarks>
public sealed class DeviceSyncService : IDeviceSyncService
{
    private readonly IMosyleClient _mosyleClient;
    private readonly ISnipeItClient _snipeClient;
    private readonly MosyleOptions _mosyleOptions;
    private readonly SnipeItOptions _snipeOptions;
    private readonly ILogger<DeviceSyncService> _logger;
    private List<SnipeModel> _models = [];

    public DeviceSyncService(
        IMosyleClient mosyleClient,
        ISnipeItClient snipeClient,
        IOptions<MosyleOptions> mosyleOptions,
        IOptions<SnipeItOptions> snipeOptions,
        ILogger<DeviceSyncService> logger)
    {
        _mosyleClient = mosyleClient;
        _snipeClient = snipeClient;
        _mosyleOptions = mosyleOptions.Value;
        _snipeOptions = snipeOptions.Value;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Mosyle to Snipe-IT sync.");

        await _mosyleClient.LoginAsync(_mosyleOptions.Email, _mosyleOptions.Password, cancellationToken);

        var devices = await PullMosyleDevicesAsync(cancellationToken);
        await RefreshSnipeModelsAsync(cancellationToken);

        var summary = new SyncSummary();
        foreach (var device in devices)
            summary.Record(await SyncDeviceAsync(device, cancellationToken));

        _logger.LogInformation(
            "Completed sync. Processed={processed}, Created={created}, Updated={updated}, Unchanged={unchanged}, Skipped={skipped}, Errors={errors}.",
            summary.Processed,
            summary.Created,
            summary.Updated,
            summary.Unchanged,
            summary.Skipped,
            summary.Errors);
    }

    private async Task<List<Device>> PullMosyleDevicesAsync(CancellationToken cancellationToken)
    {
        // Preserve the legacy two-page fetch for this cleanup pass. General pagination
        // belongs in a later behavior-focused improvement.
        var devices = await GetDevicesPageAsync(1, cancellationToken);
        devices.AddRange(await GetDevicesPageAsync(2, cancellationToken));

        _logger.LogInformation("Pulled {count} devices from Mosyle", devices.Count);
        return devices;
    }

    private async Task<List<Device>> GetDevicesPageAsync(int page, CancellationToken cancellationToken)
    {
        var response = await _mosyleClient.GetDevicesAsync(page: page, cancellationToken: cancellationToken);
        return response?.Response.FirstOrDefault()?.Devices ?? [];
    }

    private async Task RefreshSnipeModelsAsync(CancellationToken cancellationToken)
    {
        var snipeModels = await _snipeClient.GetAllModelsAsync(cancellationToken: cancellationToken);
        _models = snipeModels?.Rows ?? [];
    }

    private async Task<DeviceSyncOutcome> SyncDeviceAsync(Device device, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(device.SerialNumber))
        {
            _logger.LogWarning("Skipping Mosyle device {device} because it has no serial number", device.DeviceName);
            return DeviceSyncOutcome.Skipped;
        }

        var snipeAsset = await _snipeClient.GetAssetBySerialAsync(device.SerialNumber, cancellationToken: cancellationToken);
        var rows = snipeAsset?.Rows ?? [];

        // Missing serial matches are the only case where this pass creates a new asset.
        if (rows.Count == 0)
        {
            _logger.LogInformation("Asset with serial {serial} not found in Snipe; creating.", device.SerialNumber);
            return await CreateAssetAsync(device, cancellationToken)
                ? DeviceSyncOutcome.Created
                : DeviceSyncOutcome.Error;
        }

        var activeRows = rows
            .Where(asset => !string.Equals(asset.StatusLabel.StatusMeta, "Archived", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Archived-only and duplicate-active matches are ambiguous inventory states, so
        // the sync logs and skips instead of guessing which record should be changed.
        if (activeRows.Count == 0)
        {
            _logger.LogWarning("Asset with serial {serial} only has archived Snipe rows; skipping.", device.SerialNumber);
            return DeviceSyncOutcome.Skipped;
        }

        if (activeRows.Count > 1)
        {
            _logger.LogWarning("Asset with serial {serial} found more than one active row.", device.SerialNumber);
            return DeviceSyncOutcome.Skipped;
        }

        return await UpdateAssetIfNeededAsync(activeRows[0], device, cancellationToken)
            ? DeviceSyncOutcome.Updated
            : DeviceSyncOutcome.Unchanged;
    }

    private async Task<bool> UpdateAssetIfNeededAsync(Asset asset, Device device, CancellationToken cancellationToken)
    {
        var updateRequest = new UpdateAssetRequest
        {
            Serial = device.SerialNumber
        };

        var needsUpdate = ApplyCustomFieldUpdates(asset, device, updateRequest);
        var modelId = await ResolveModelUpdateAsync(asset, device, cancellationToken);

        if (modelId.HasValue)
        {
            updateRequest.ModelId = modelId.Value;
            needsUpdate = true;
        }

        if (!needsUpdate)
        {
            _logger.LogInformation("Asset {asset} is already up to date.", asset.AssetTag);
            return false;
        }

        // Preserve the original update shape: once any tracked field changes, send the
        // full Mosyle values for the managed custom fields plus serial/model changes.
        updateRequest.Imei = device.IMEI;
        updateRequest.Eid = device.EID;
        updateRequest.SimCardPhoneNumber = device.PhoneNumber;

        _logger.LogInformation("Updating asset {asset}", asset.Name);
        await _snipeClient.UpdateAssetAsync(asset.Id, updateRequest, cancellationToken);
        _logger.LogInformation("Updated asset {asset}", asset.AssetTag);
        return true;
    }

    private bool ApplyCustomFieldUpdates(Asset asset, Device device, UpdateAssetRequest updateRequest)
    {
        var needsUpdate = false;

        if (TryGetCustomFieldValue(asset, "IMEI", out var imeiField) && imeiField != device.IMEI)
        {
            _logger.LogInformation("Updating IMEI for asset {asset} from {old} to {new}", asset.Name, imeiField, device.IMEI);
            updateRequest.Imei = device.IMEI;
            needsUpdate = true;
        }

        if (TryGetCustomFieldValue(asset, "EID", out var eidField) && eidField != device.EID)
        {
            _logger.LogInformation("Updating EID for asset {asset} from {old} to {new}", asset.Name, eidField, device.EID);
            updateRequest.Eid = device.EID;
            needsUpdate = true;
        }

        if (TryGetCustomFieldValue(asset, "SIM Card Phone Number", out var phoneNumberField) &&
            phoneNumberField != device.PhoneNumber)
        {
            _logger.LogInformation(
                "Updating Phone Number for asset {asset} from {old} to {new}",
                asset.Name,
                phoneNumberField,
                device.PhoneNumber);
            updateRequest.SimCardPhoneNumber = device.PhoneNumber;
            needsUpdate = true;
        }

        return needsUpdate;
    }

    private static bool TryGetCustomFieldValue(Asset asset, string fieldName, out string? value)
    {
        if (asset.CustomFields.TryGetValue(fieldName, out var customField))
        {
            value = customField.Value;
            return value != null;
        }

        value = null;
        return false;
    }

    private async Task<int?> ResolveModelUpdateAsync(Asset asset, Device device, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(device.DeviceModelName) ||
            string.Equals(asset.Model.Name, device.DeviceModelName, StringComparison.Ordinal))
            return null;

        var snipeModel = await EnsureModelAsync(device, cancellationToken);
        if (snipeModel == null)
            return null;

        _logger.LogInformation(
            "Updating Model for asset {asset} from {old} to {new}",
            asset.Name,
            asset.Model.Name,
            device.DeviceModelName);

        return snipeModel.Id;
    }

    private async Task<bool> CreateAssetAsync(Device device, CancellationToken cancellationToken)
    {
        var snipeModel = await EnsureModelAsync(device, cancellationToken);
        if (snipeModel == null)
        {
            _logger.LogError("Could not find or create model for device {device}, skipping asset creation", device.DeviceName);
            return false;
        }

        var newAsset = await _snipeClient.CreateAssetAsync(new CreateAssetRequest
        {
            Name = device.DeviceName,
            ModelId = snipeModel.Id,
            Serial = device.SerialNumber,
            StatusId = _snipeOptions.DefaultStatusId,
            LocationId = _snipeOptions.DefaultLocationId,
            Imei = device.IMEI,
            Eid = device.EID,
            Notes = $"Created from Mosyle sync. Last seen online at {device.DateLastBeat}"
        }, cancellationToken);

        var assetTag = newAsset?.Payload.AssetTag;
        if (string.IsNullOrWhiteSpace(assetTag))
        {
            _logger.LogError("Failed to create asset for device {device}", device.DeviceName);
            return false;
        }

        _logger.LogInformation("Created new asset {asset} in Snipe", assetTag);
        await _mosyleClient.UpdateDeviceAsync(device.SerialNumber, [assetTag], cancellationToken);
        _logger.LogInformation("Updated Mosyle device {device} with asset tag {asset}", device.DeviceName, assetTag);
        return true;
    }

    private async Task<SnipeModel?> EnsureModelAsync(Device device, CancellationToken cancellationToken)
    {
        var modelName = device.DeviceModelName;
        if (string.IsNullOrWhiteSpace(modelName))
            return null;

        var snipeModel = _models.FirstOrDefault(model => string.Equals(model.Name, modelName, StringComparison.Ordinal));
        if (snipeModel != null)
            return snipeModel;

        // Snipe-IT requires assets to reference an existing model. Create the model
        // with the legacy default IDs, then refresh the local cache to obtain its ID.
        _logger.LogInformation("Model {model} not found in Snipe for device {device}; creating.", modelName, device.DeviceName);
        await _snipeClient.CreateModelAsync(
            modelName,
            _snipeOptions.DefaultCategoryId,
            _snipeOptions.DefaultManufacturerId,
            device.DeviceModel,
            _snipeOptions.DefaultModelEol,
            _snipeOptions.DefaultFieldsetId,
            cancellationToken);

        await RefreshSnipeModelsAsync(cancellationToken);
        return _models.FirstOrDefault(model => string.Equals(model.Name, modelName, StringComparison.Ordinal));
    }

    private enum DeviceSyncOutcome
    {
        Created,
        Updated,
        Unchanged,
        Skipped,
        Error
    }

    private sealed class SyncSummary
    {
        public int Processed { get; private set; }
        public int Created { get; private set; }
        public int Updated { get; private set; }
        public int Unchanged { get; private set; }
        public int Skipped { get; private set; }
        public int Errors { get; private set; }

        public void Record(DeviceSyncOutcome outcome)
        {
            Processed++;

            switch (outcome)
            {
                case DeviceSyncOutcome.Created:
                    Created++;
                    break;
                case DeviceSyncOutcome.Updated:
                    Updated++;
                    break;
                case DeviceSyncOutcome.Unchanged:
                    Unchanged++;
                    break;
                case DeviceSyncOutcome.Skipped:
                    Skipped++;
                    break;
                case DeviceSyncOutcome.Error:
                    Errors++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null);
            }
        }
    }
}
