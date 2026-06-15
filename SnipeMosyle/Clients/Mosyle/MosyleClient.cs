// Copyright (c) Jordan Molenaar (jordanm.cloud). All rights reserved.

using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace jordanm.cloud.mosyle.Clients.Mosyle;

/// <summary>
/// RestSharp-backed implementation of the small Mosyle API surface needed by the sync.
/// </summary>
public sealed class MosyleClient : IMosyleClient
{
    private readonly RestClient _client;
    private string? _sessionToken;

    public MosyleClient(string accessToken, string baseUrl)
    {
        var options = new RestClientOptions(baseUrl)
        {
            ThrowOnAnyError = false,
            Timeout = TimeSpan.FromSeconds(10)
        };

        _client = new RestClient(options, configureSerialization: serializer => serializer.UseNewtonsoftJson());
        _client.AddDefaultHeader("accessToken", accessToken);
    }

    public async Task<LoginResponse?> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var request = new RestRequest("login", Method.Post);
        request.AddJsonBody(new { email, password });

        var response = await _client.ExecuteAsync<LoginResponse>(request, cancellationToken);

        if (!response.IsSuccessful)
            throw new InvalidOperationException($"Login failed: {response.StatusCode} - {response.Content}");

        var login = response.Data;
        if (login == null && !string.IsNullOrEmpty(response.Content))
            login = JsonConvert.DeserializeObject<LoginResponse>(response.Content);

        // Mosyle returns the session bearer token in the Authorization response header,
        // not in the JSON body.
        var authHeader = response.Headers?
            .FirstOrDefault(header => string.Equals(header.Name, "Authorization", StringComparison.OrdinalIgnoreCase))
            ?.Value?.ToString();

        if (string.IsNullOrWhiteSpace(authHeader))
            return login;

        login ??= new LoginResponse();
        login.Authorization = authHeader;

        const string bearerPrefix = "Bearer ";
        _sessionToken = authHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase)
            ? authHeader[bearerPrefix.Length..].Trim()
            : authHeader;

        _client.AddDefaultHeader("Authorization", $"Bearer {_sessionToken}");
        return login;
    }

    public async Task<DeviceListResponse?> GetDevicesAsync(
        string os = "ios",
        bool supervised = true,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        EnsureLoggedIn();

        var request = new RestRequest("devices", Method.Post);
        request.AddJsonBody(new
        {
            operation = "list",
            options = new
            {
                os,
                supervised = supervised ? "true" : "false",
                page,
                // Preserve Mosyle's expected payload spelling from the original implementation.
                pazeSize = 100
            }
        });

        var response = await _client.ExecuteAsync<DeviceListResponse>(request, cancellationToken);

        if (!response.IsSuccessful)
            throw new InvalidOperationException($"Device list failed: {response.StatusCode} - {response.Content}");

        return response.Data ?? JsonConvert.DeserializeObject<DeviceListResponse>(response.Content ?? "{}");
    }

    public async Task<DeviceUpdateResponse?> UpdateDeviceAsync(
        string serialNumber,
        IReadOnlyCollection<string>? assetTags = null,
        CancellationToken cancellationToken = default)
    {
        EnsureLoggedIn();

        var request = new RestRequest("devices", Method.Post);
        request.AddJsonBody(new
        {
            operation = "update_device",
            serialnumber = serialNumber,
            asset_tag = assetTags
        });

        var response = await _client.ExecuteAsync<DeviceUpdateResponse>(request, cancellationToken);

        if (!response.IsSuccessful)
            throw new InvalidOperationException($"Device update failed: {response.StatusCode} - {response.Content}");

        return response.Data ?? JsonConvert.DeserializeObject<DeviceUpdateResponse>(response.Content ?? "{}");
    }

    private void EnsureLoggedIn()
    {
        if (string.IsNullOrWhiteSpace(_sessionToken))
            throw new InvalidOperationException("You must login first.");
    }
}
