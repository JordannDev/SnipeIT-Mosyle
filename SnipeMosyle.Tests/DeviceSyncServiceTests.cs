// Copyright (c) Jordan Molenaar (jordanm.cloud). All rights reserved.

using jordanm.cloud.mosyle.Clients.Mosyle;
using jordanm.cloud.mosyle.Clients.SnipeIt;
using jordanm.cloud.mosyle.Options;
using jordanm.cloud.mosyle.Sync;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
using OptionsFactory = Microsoft.Extensions.Options.Options;
using SnipeModel = jordanm.cloud.mosyle.Clients.SnipeIt.Model;

namespace SnipeMosyle.Tests;

public sealed class DeviceSyncServiceTests
{
    [Fact]
    public async Task RunAsync_CreatesAsset_WhenSnipeLookupReturnsNull()
    {
        var mosyle = new FakeMosyleClient(Device());
        var snipe = new FakeSnipeItClient
        {
            Lookup = _ => null
        };
        snipe.Models.Add(new SnipeModel { Id = 50, Name = "iPad 10" });

        await CreateService(mosyle, snipe).RunAsync();

        Assert.Single(snipe.CreatedAssets);
        Assert.Single(mosyle.UpdatedDevices);
    }

    [Fact]
    public async Task RunAsync_CreatesAsset_WhenSnipeLookupReturnsNoRows()
    {
        var mosyle = new FakeMosyleClient(Device());
        var snipe = new FakeSnipeItClient
        {
            Lookup = _ => new AssetListResponse()
        };
        snipe.Models.Add(new SnipeModel { Id = 50, Name = "iPad 10" });

        await CreateService(mosyle, snipe).RunAsync();

        Assert.Single(snipe.CreatedAssets);
        Assert.Single(mosyle.UpdatedDevices);
    }

    [Fact]
    public async Task RunAsync_SkipsDuplicateActiveAssets()
    {
        var mosyle = new FakeMosyleClient(Device());
        var snipe = new FakeSnipeItClient
        {
            Lookup = _ => new AssetListResponse
            {
                Rows =
                [
                    Asset(statusMeta: "Ready to Deploy"),
                    Asset(id: 11, statusMeta: "Deployed")
                ]
            }
        };

        await CreateService(mosyle, snipe).RunAsync();

        Assert.Empty(snipe.CreatedAssets);
        Assert.Empty(snipe.UpdatedAssets);
        Assert.Empty(mosyle.UpdatedDevices);
    }

    [Fact]
    public async Task RunAsync_SkipsAllArchivedAssets()
    {
        var mosyle = new FakeMosyleClient(Device());
        var snipe = new FakeSnipeItClient
        {
            Lookup = _ => new AssetListResponse
            {
                Rows = [Asset(statusMeta: "Archived")]
            }
        };

        await CreateService(mosyle, snipe).RunAsync();

        Assert.Empty(snipe.CreatedAssets);
        Assert.Empty(snipe.UpdatedAssets);
        Assert.Empty(mosyle.UpdatedDevices);
    }

    [Fact]
    public async Task RunAsync_UpdatesChangedFieldsAndModel()
    {
        var mosyle = new FakeMosyleClient(Device());
        var snipe = new FakeSnipeItClient
        {
            Lookup = _ => new AssetListResponse
            {
                Rows =
                [
                    Asset(
                        modelName: "iPad 9",
                        imei: "old-imei",
                        eid: "old-eid",
                        phoneNumber: "old-phone")
                ]
            }
        };
        snipe.Models.Add(new SnipeModel { Id = 200, Name = "iPad 10" });

        await CreateService(mosyle, snipe).RunAsync();

        var update = Assert.Single(snipe.UpdatedAssets);
        Assert.Equal(10, update.AssetId);
        Assert.Equal(200, update.Request.ModelId);
        Assert.Equal("imei-new", update.Request.Imei);
        Assert.Equal("eid-new", update.Request.Eid);
        Assert.Equal("555-0100", update.Request.SimCardPhoneNumber);
    }

    [Fact]
    public async Task RunAsync_CreatesMissingModelBeforeCreatingAsset()
    {
        var mosyle = new FakeMosyleClient(Device());
        var snipe = new FakeSnipeItClient
        {
            Lookup = _ => new AssetListResponse()
        };

        await CreateService(mosyle, snipe).RunAsync();

        Assert.Single(snipe.CreatedModels);
        var asset = Assert.Single(snipe.CreatedAssets);
        Assert.Equal(1000, asset.ModelId);
    }

    private static DeviceSyncService CreateService(FakeMosyleClient mosyle, FakeSnipeItClient snipe)
    {
        return new DeviceSyncService(
            mosyle,
            snipe,
            OptionsFactory.Create(new MosyleOptions
            {
                AccessToken = "access-token",
                Email = "user@example.com",
                Password = "password"
            }),
            OptionsFactory.Create(new SnipeItOptions()),
            NullLogger<DeviceSyncService>.Instance);
    }

    private static Device Device(string serial = "ABC123", string modelName = "iPad 10")
    {
        return new Device
        {
            DeviceName = "Mosyle Device",
            SerialNumber = serial,
            DeviceModelName = modelName,
            DeviceModel = "A1234",
            IMEI = "imei-new",
            EID = "eid-new",
            PhoneNumber = "555-0100",
            DateLastBeat = "2026-06-15"
        };
    }

    private static Asset Asset(
        int id = 10,
        string statusMeta = "Ready to Deploy",
        string modelName = "iPad 10",
        string imei = "imei-new",
        string eid = "eid-new",
        string phoneNumber = "555-0100")
    {
        return new Asset
        {
            Id = id,
            Name = "Snipe Asset",
            AssetTag = $"HB-{id}",
            Serial = "ABC123",
            Model = new SnipeModel { Id = 99, Name = modelName },
            StatusLabel = new StatusLabel { StatusMeta = statusMeta },
            CustomFields = new Dictionary<string, CustomField>
            {
                ["IMEI"] = new() { Value = imei },
                ["EID"] = new() { Value = eid },
                ["SIM Card Phone Number"] = new() { Value = phoneNumber }
            }
        };
    }

    private sealed class FakeMosyleClient : IMosyleClient
    {
        private readonly List<Device> _devices;

        public FakeMosyleClient(params Device[] devices)
        {
            _devices = devices.ToList();
        }

        public List<(string SerialNumber, IReadOnlyCollection<string>? AssetTags)> UpdatedDevices { get; } = [];

        public Task<LoginResponse?> LoginAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<LoginResponse?>(new LoginResponse { Email = email });
        }

        public Task<DeviceListResponse?> GetDevicesAsync(
            string os = "ios",
            bool supervised = true,
            int page = 1,
            CancellationToken cancellationToken = default)
        {
            var devices = page == 1 ? _devices : [];
            return Task.FromResult<DeviceListResponse?>(new DeviceListResponse
            {
                Response = [new DeviceResponse { Devices = devices }]
            });
        }

        public Task<DeviceUpdateResponse?> UpdateDeviceAsync(
            string serialNumber,
            IReadOnlyCollection<string>? assetTags = null,
            CancellationToken cancellationToken = default)
        {
            UpdatedDevices.Add((serialNumber, assetTags));
            return Task.FromResult<DeviceUpdateResponse?>(new DeviceUpdateResponse());
        }
    }

    private sealed class FakeSnipeItClient : ISnipeItClient
    {
        private int _nextModelId = 1000;

        public Func<string, AssetListResponse?> Lookup { get; set; } = _ => new AssetListResponse();
        public List<SnipeModel> Models { get; } = [];
        public List<CreateAssetRequest> CreatedAssets { get; } = [];
        public List<string> CreatedModels { get; } = [];
        public List<(int AssetId, UpdateAssetRequest Request)> UpdatedAssets { get; } = [];

        public Task<AssetListResponse?> GetAssetBySerialAsync(
            string serial,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Lookup(serial));
        }

        public Task<AssetListResponse?> GetAssetsAsync(int page = 1, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<AssetListResponse?>(new AssetListResponse());
        }

        public Task<UserResponse?> GetUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<UserResponse?>(new UserResponse());
        }

        public Task<ModelListResponse?> GetAllModelsAsync(
            int limit = 200,
            int offset = 0,
            string sort = "created_at",
            string order = "asc",
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<ModelListResponse?>(new ModelListResponse { Rows = Models.ToList() });
        }

        public Task<CreateModelResponse?> CreateModelAsync(
            string name,
            int categoryId,
            int? manufacturerId = null,
            string? modelNumber = null,
            int? eol = null,
            int? fieldsetId = null,
            CancellationToken cancellationToken = default)
        {
            CreatedModels.Add(name);
            var model = new SnipeModel { Id = _nextModelId++, Name = name, ModelNumber = modelNumber };
            Models.Add(model);
            return Task.FromResult<CreateModelResponse?>(new CreateModelResponse { Rows = model });
        }

        public Task<UpdateAssetResponse?> UpdateAssetAsync(
            int assetId,
            UpdateAssetRequest updateRequest,
            CancellationToken cancellationToken = default)
        {
            UpdatedAssets.Add((assetId, updateRequest));
            return Task.FromResult<UpdateAssetResponse?>(new UpdateAssetResponse());
        }

        public Task<CreateAssetResponse?> CreateAssetAsync(
            CreateAssetRequest requestModel,
            CancellationToken cancellationToken = default)
        {
            CreatedAssets.Add(requestModel);
            return Task.FromResult<CreateAssetResponse?>(new CreateAssetResponse
            {
                Payload = new CreatedAsset { AssetTag = "HB-100" }
            });
        }
    }
}
