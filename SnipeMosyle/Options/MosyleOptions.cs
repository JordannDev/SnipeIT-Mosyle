// Copyright (c) Jordan Molenaar (jordanm.cloud). All rights reserved.

namespace jordanm.cloud.mosyle.Options;

/// <summary>
/// Runtime settings required to authenticate with the Mosyle Business API.
/// </summary>
/// <remarks>
/// This type is bound from the <c>Mosyle</c> section in <c>appsettings.json</c>.
/// Environment variables or a secret provider can override the same keys later
/// without changing the sync service.
/// </remarks>
public sealed class MosyleOptions
{
    /// <summary>The configuration section name used for Mosyle settings.</summary>
    public const string SectionName = "Mosyle";

    /// <summary>The Mosyle Business API base URL.</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>The Mosyle access token sent as the <c>accessToken</c> header.</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>The Mosyle account email used for the login call.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>The Mosyle account password used for the login call.</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Delay between successful paged Mosyle requests.</summary>
    public int PageDelayMilliseconds { get; set; } = Clients.ApiRetryPolicy.DefaultPageDelayMilliseconds;

    /// <summary>Maximum number of retries after Mosyle returns HTTP 429.</summary>
    public int RateLimitMaxRetries { get; set; } = Clients.ApiRetryPolicy.DefaultMaxRateLimitRetries;
}
