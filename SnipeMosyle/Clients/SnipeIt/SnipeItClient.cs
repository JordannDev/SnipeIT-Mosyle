// Copyright (c) Jordan Molenaar (jordanm.cloud). All rights reserved.

using Newtonsoft.Json;
using RestSharp;

namespace jordanm.cloud.mosyle.Clients.SnipeIt;

/// <summary>
/// RestSharp-backed implementation of the Snipe-IT API calls used by the sync.
/// </summary>
public sealed class SnipeItClient : ISnipeItClient
{
    private readonly RestClient _client;

    public SnipeItClient(string baseUrl, string apiToken)
    {
        var options = new RestClientOptions(baseUrl.TrimEnd('/') + "/api/v1/")
        {
            ThrowOnAnyError = false,
            Timeout = TimeSpan.FromSeconds(30)
        };

        _client = new RestClient(options);
        _client.AddDefaultHeader("Accept", "application/json");
        _client.AddDefaultHeader("Authorization", $"Bearer {apiToken}");
    }

    public Task<AssetListResponse?> GetAssetBySerialAsync(
        string serial,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var endpoint = $"hardware/byserial/{serial}?deleted={includeDeleted.ToString().ToLowerInvariant()}";
        return SendRequestAsync<AssetListResponse>(endpoint, Method.Get, cancellationToken: cancellationToken);
    }

    public Task<AssetListResponse?> GetAssetsAsync(int page = 1, CancellationToken cancellationToken = default)
    {
        return SendRequestAsync<AssetListResponse>($"hardware?page={page}", Method.Get, cancellationToken: cancellationToken);
    }

    public Task<UserResponse?> GetUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return SendRequestAsync<UserResponse>($"users/{userId}", Method.Get, cancellationToken: cancellationToken);
    }

    public Task<ModelListResponse?> GetAllModelsAsync(
        int limit = 200,
        int offset = 0,
        string sort = "created_at",
        string order = "asc",
        CancellationToken cancellationToken = default)
    {
        var endpoint = $"models?limit={limit}&offset={offset}&sort={sort}&order={order}";
        return SendRequestAsync<ModelListResponse>(endpoint, Method.Get, cancellationToken: cancellationToken);
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
        var body = new
        {
            name,
            model_number = modelNumber,
            category_id = categoryId,
            manufacturer_id = manufacturerId,
            eol,
            fieldset_id = fieldsetId
        };

        return SendRequestAsync<CreateModelResponse>("models", Method.Post, body, cancellationToken);
    }

    public Task<UpdateAssetResponse?> UpdateAssetAsync(
        int assetId,
        UpdateAssetRequest updateRequest,
        CancellationToken cancellationToken = default)
    {
        return SendRequestAsync<UpdateAssetResponse>($"hardware/{assetId}", Method.Patch, updateRequest, cancellationToken);
    }

    public Task<CreateAssetResponse?> CreateAssetAsync(
        CreateAssetRequest requestModel,
        CancellationToken cancellationToken = default)
    {
        return SendRequestAsync<CreateAssetResponse>("hardware", Method.Post, requestModel, cancellationToken);
    }

    private async Task<T?> SendRequestAsync<T>(
        string endpoint,
        Method method,
        object? body = null,
        CancellationToken cancellationToken = default)
    {
        var request = new RestRequest(endpoint, method);

        if (body != null)
            request.AddJsonBody(body);

        var response = await _client.ExecuteAsync(request, cancellationToken);

        if (!response.IsSuccessful)
            throw new InvalidOperationException($"API call failed: {response.StatusCode} - {response.Content}");

        // Snipe-IT responses are deserialized manually here to keep one consistent
        // error-handling path for GET, POST, and PATCH calls.
        return JsonConvert.DeserializeObject<T>(response.Content ?? "");
    }
}
