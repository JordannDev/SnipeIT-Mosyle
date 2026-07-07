// Copyright (c) Jordan Molenaar (jordanm.cloud). All rights reserved.

namespace jordanm.cloud.mosyle.Clients.SnipeIt;

/// <summary>
/// Testable boundary for the Snipe-IT API operations used by the sync workflow.
/// </summary>
public interface ISnipeItClient
{
    /// <summary>
    /// Finds Snipe-IT hardware records by serial number.
    /// </summary>
    Task<AssetListResponse?> GetAssetBySerialAsync(
        string serial,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves a page of Snipe-IT hardware assets.</summary>
    Task<AssetListResponse?> GetAssetsAsync(int page = 1, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all Snipe-IT hardware assets by walking paged list responses.</summary>
    Task<AssetListResponse?> GetAllAssetsAsync(
        int limit = 200,
        int offset = 0,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves a Snipe-IT user by ID.</summary>
    Task<UserResponse?> GetUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Snipe-IT models used to match Mosyle device model names.
    /// </summary>
    Task<ModelListResponse?> GetAllModelsAsync(
        int limit = 200,
        int offset = 0,
        string sort = "created_at",
        string order = "asc",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a missing Snipe-IT model so assets can be created or moved to the right model.
    /// </summary>
    Task<CreateModelResponse?> CreateModelAsync(
        string name,
        int categoryId,
        int? manufacturerId = null,
        string? modelNumber = null,
        int? eol = null,
        int? fieldsetId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Patches an existing Snipe-IT asset with Mosyle-sourced values.
    /// </summary>
    Task<UpdateAssetResponse?> UpdateAssetAsync(
        int assetId,
        UpdateAssetRequest updateRequest,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Snipe-IT asset for a Mosyle device that is not present in inventory.
    /// </summary>
    Task<CreateAssetResponse?> CreateAssetAsync(
        CreateAssetRequest requestModel,
        CancellationToken cancellationToken = default);
}
