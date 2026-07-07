// Copyright (c) Jordan Molenaar (jordanm.cloud). All rights reserved.

namespace jordanm.cloud.mosyle.Options;

/// <summary>
/// Runtime settings and default Snipe-IT IDs used when creating assets and models.
/// </summary>
/// <remarks>
/// This type is bound from the <c>SnipeIt</c> section in <c>appsettings.json</c>.
/// The numeric defaults preserve the legacy behavior from the original sync.
/// </remarks>
public sealed class SnipeItOptions
{
    /// <summary>The configuration section name used for Snipe-IT settings.</summary>
    public const string SectionName = "SnipeIt";

    /// <summary>The Snipe-IT site base URL; the client appends <c>/api/v1</c>.</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>The Snipe-IT API bearer token.</summary>
    public string ApiToken { get; set; } = string.Empty;

    /// <summary>The status assigned to newly-created assets. Current value maps to "In Stock".</summary>
    public int DefaultStatusId { get; set; } = 2;

    /// <summary>The location assigned to newly-created assets. Current value maps to HQ.</summary>
    public int DefaultLocationId { get; set; } = 13;

    /// <summary>The manufacturer assigned when creating missing models.</summary>
    public int DefaultManufacturerId { get; set; } = 1;

    /// <summary>The category assigned when creating missing models.</summary>
    public int DefaultCategoryId { get; set; } = 6;

    /// <summary>The fieldset assigned when creating missing models.</summary>
    public int DefaultFieldsetId { get; set; } = 5;

    /// <summary>The end-of-life value sent when creating missing models.</summary>
    public int DefaultModelEol { get; set; }

    /// <summary>Delay between successful paged Snipe-IT requests.</summary>
    public int PageDelayMilliseconds { get; set; } = Clients.ApiRetryPolicy.DefaultPageDelayMilliseconds;

    /// <summary>Maximum number of retries after Snipe-IT returns HTTP 429.</summary>
    public int RateLimitMaxRetries { get; set; } = Clients.ApiRetryPolicy.DefaultMaxRateLimitRetries;
}
