// Copyright (c) Jordan Molenaar (jordanm.cloud). All rights reserved.

namespace jordanm.cloud.mosyle.Clients.Mosyle;

/// <summary>
/// Testable boundary for the Mosyle Business API operations used by the sync workflow.
/// </summary>
public interface IMosyleClient
{
    /// <summary>
    /// Authenticates with Mosyle and stores the returned bearer token for subsequent requests.
    /// </summary>
    Task<LoginResponse?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves one page of Mosyle devices.
    /// </summary>
    /// <param name="os">The device OS filter Mosyle expects, such as <c>ios</c>.</param>
    /// <param name="supervised">Whether to request only supervised devices.</param>
    /// <param name="page">The one-based Mosyle page number.</param>
    Task<DeviceListResponse?> GetDevicesAsync(
        string os = "ios",
        bool supervised = true,
        int page = 1,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a Mosyle device with one or more Snipe-IT asset tags.
    /// </summary>
    Task<DeviceUpdateResponse?> UpdateDeviceAsync(
        string serialNumber,
        IReadOnlyCollection<string>? assetTags = null,
        CancellationToken cancellationToken = default);
}
