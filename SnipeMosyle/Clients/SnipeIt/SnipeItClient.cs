// Copyright (c) Jordan Molenaar (jordanm.cloud). All rights reserved.

using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using jordanm.cloud.mosyle.Clients;

namespace jordanm.cloud.mosyle.Clients.SnipeIt;

/// <summary>
/// RestSharp-backed implementation of the Snipe-IT API calls used by the sync.
/// </summary>
public sealed class SnipeItClient : ISnipeItClient
{
    private readonly RestClient _client;
    private readonly int _pageDelayMilliseconds;
    private readonly int _maxRateLimitRetries;

    public SnipeItClient(
        string baseUrl,
        string apiToken,
        int pageDelayMilliseconds = ApiRetryPolicy.DefaultPageDelayMilliseconds,
        int maxRateLimitRetries = ApiRetryPolicy.DefaultMaxRateLimitRetries)
    {
        var options = new RestClientOptions(baseUrl.TrimEnd('/') + "/api/v1/")
        {
            ThrowOnAnyError = false,
            Timeout = TimeSpan.FromSeconds(30)
        };

        _client = new RestClient(options, configureSerialization: serializer => serializer.UseNewtonsoftJson());
        _client.AddDefaultHeader("Accept", "application/json");
        _client.AddDefaultHeader("Authorization", $"Bearer {apiToken}");
        _pageDelayMilliseconds = pageDelayMilliseconds;
        _maxRateLimitRetries = Math.Max(0, maxRateLimitRetries);
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

    public async Task<AssetListResponse?> GetAllAssetsAsync(
        int limit = 200,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        var pageLimit = Math.Max(1, limit);
        var currentOffset = Math.Max(0, offset);
        var allAssets = new AssetListResponse
        {
            Offset = currentOffset,
            Limit = pageLimit
        };

        while (true)
        {
            var page = await SendRequestAsync<AssetListResponse>(
                $"hardware?limit={pageLimit}&offset={currentOffset}",
                Method.Get,
                cancellationToken: cancellationToken);

            var rows = page?.Rows ?? [];
            allAssets.Status = page?.Status ?? allAssets.Status;
            allAssets.Total = page?.Total ?? allAssets.Total;
            allAssets.Rows.AddRange(rows);

            if (!ShouldFetchNextPage(allAssets.Total, allAssets.Rows.Count, rows.Count, pageLimit))
                return allAssets;

            currentOffset += pageLimit;
            await ApiRetryPolicy.DelayBetweenPagesAsync(_pageDelayMilliseconds, cancellationToken);
        }
    }

    public Task<UserResponse?> GetUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return SendRequestAsync<UserResponse>($"users/{userId}", Method.Get, cancellationToken: cancellationToken);
    }

    public async Task<ModelListResponse?> GetAllModelsAsync(
        int limit = 200,
        int offset = 0,
        string sort = "created_at",
        string order = "asc",
        CancellationToken cancellationToken = default)
    {
        var pageLimit = Math.Max(1, limit);
        var currentOffset = Math.Max(0, offset);
        var allModels = new ModelListResponse
        {
            Offset = currentOffset,
            Limit = pageLimit
        };

        while (true)
        {
            var endpoint = $"models?limit={pageLimit}&offset={currentOffset}&sort={sort}&order={order}";
            var page = await SendRequestAsync<ModelListResponse>(endpoint, Method.Get, cancellationToken: cancellationToken);

            var rows = page?.Rows ?? [];
            allModels.Status = page?.Status ?? allModels.Status;
            allModels.Total = page?.Total ?? allModels.Total;
            allModels.Rows.AddRange(rows);

            if (!ShouldFetchNextPage(allModels.Total, allModels.Rows.Count, rows.Count, pageLimit))
                return allModels;

            currentOffset += pageLimit;
            await ApiRetryPolicy.DelayBetweenPagesAsync(_pageDelayMilliseconds, cancellationToken);
        }
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

        var response = await ExecuteWithRateLimitRetryAsync(request, cancellationToken);

        if (!response.IsSuccessful)
            throw new InvalidOperationException($"API call failed: {response.StatusCode} - {response.Content}");

        // Snipe-IT responses are deserialized manually here to keep one consistent
        // error-handling path for GET, POST, and PATCH calls.
        return JsonConvert.DeserializeObject<T>(response.Content ?? "");
    }

    private static bool ShouldFetchNextPage(int total, int collectedRows, int pageRows, int pageLimit)
    {
        if (pageRows == 0)
            return false;

        if (total > 0)
            return collectedRows < total;

        return pageRows >= pageLimit;
    }

    private async Task<RestResponse> ExecuteWithRateLimitRetryAsync(
        RestRequest request,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            var response = await _client.ExecuteAsync(request, cancellationToken);
            if ((int)response.StatusCode != 429 || attempt >= _maxRateLimitRetries)
                return response;

            var delay = ApiRetryPolicy.GetRetryDelay(response.Headers, _pageDelayMilliseconds, attempt + 1);
            await Task.Delay(delay, cancellationToken);
        }
    }
}
