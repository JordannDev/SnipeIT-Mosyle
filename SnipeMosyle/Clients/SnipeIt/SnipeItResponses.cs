// Copyright (c) Jordan Molenaar (jordanm.cloud). All rights reserved.

using Newtonsoft.Json;

namespace jordanm.cloud.mosyle.Clients.SnipeIt;

/// <summary>
/// Response returned by Snipe-IT after creating an asset.
/// </summary>
public sealed class CreateAssetResponse
{
    [JsonProperty("status")] public string Status { get; set; } = string.Empty;
    [JsonProperty("payload")] public CreatedAsset Payload { get; set; } = new();
}

public sealed class CreatedAsset
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("asset_tag")] public string AssetTag { get; set; } = string.Empty;
    [JsonProperty("serial")] public string Serial { get; set; } = string.Empty;
}

public sealed class UpdateAssetResponse
{
    [JsonProperty("status")] public string Status { get; set; } = string.Empty;
    [JsonProperty("rows")] public Asset? Rows { get; set; }
}

public sealed class CreateModelResponse
{
    [JsonProperty("status")] public string Status { get; set; } = string.Empty;
    [JsonProperty("rows")] public Model? Rows { get; set; }
}

public sealed class ModelListResponse
{
    [JsonProperty("status")] public string Status { get; set; } = string.Empty;
    [JsonProperty("total")] public int Total { get; set; }
    [JsonProperty("offset")] public int Offset { get; set; }
    [JsonProperty("limit")] public int Limit { get; set; }
    [JsonProperty("rows")] public List<Model> Rows { get; set; } = [];
}

/// <summary>
/// Standard Snipe-IT list envelope used by asset search endpoints.
/// </summary>
public sealed class AssetListResponse
{
    [JsonProperty("status")] public string Status { get; set; } = string.Empty;
    [JsonProperty("total")] public int Total { get; set; }
    [JsonProperty("offset")] public int Offset { get; set; }
    [JsonProperty("limit")] public int Limit { get; set; }
    [JsonProperty("rows")] public List<Asset> Rows { get; set; } = [];
}

public sealed class AssetResponse
{
    [JsonProperty("status")] public string Status { get; set; } = string.Empty;
    [JsonProperty("payload")] public Asset? Payload { get; set; }
}

public sealed class UserResponse
{
    [JsonProperty("status")] public string Status { get; set; } = string.Empty;
    [JsonProperty("payload")] public User? Payload { get; set; }
}
