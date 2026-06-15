// Copyright (c) Jordan Molenaar (jordanm.cloud). All rights reserved.

using Newtonsoft.Json;

namespace jordanm.cloud.mosyle.Clients.SnipeIt;

/// <summary>
/// Payload for creating a Snipe-IT hardware asset.
/// </summary>
public sealed class CreateAssetRequest
{
    [JsonProperty("status_id")] public int StatusId { get; set; }
    [JsonProperty("model_id")] public int ModelId { get; set; }
    [JsonProperty("name")] public string? Name { get; set; }
    [JsonProperty("serial")] public string? Serial { get; set; }
    [JsonProperty("notes")] public string? Notes { get; set; }
    [JsonProperty("location_id")] public int? LocationId { get; set; }
    [JsonProperty("custom_fields")] public object? CustomFields { get; set; }
    [JsonProperty("_snipeit_imei_13")] public string? Imei { get; set; }
    [JsonProperty("_snipeit_eid_14")] public string? Eid { get; set; }
}

/// <summary>
/// Payload for patching Mosyle-managed fields on an existing Snipe-IT asset.
/// </summary>
public sealed class UpdateAssetRequest
{
    [JsonProperty("model_id")] public int? ModelId { get; set; }
    [JsonProperty("serial")] public string? Serial { get; set; }
    [JsonProperty("_snipeit_imei_13")] public string? Imei { get; set; }
    [JsonProperty("_snipeit_eid_14")] public string? Eid { get; set; }
    [JsonProperty("_snipeit_sim_card_phone_number_15")] public string? SimCardPhoneNumber { get; set; }
}
