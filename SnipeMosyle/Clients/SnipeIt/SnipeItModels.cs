// Copyright (c) Jordan Molenaar (jordanm.cloud). All rights reserved.

using Newtonsoft.Json;

namespace jordanm.cloud.mosyle.Clients.SnipeIt;

/// <summary>
/// Snipe-IT model metadata used to map Mosyle device model names to model IDs.
/// </summary>
public sealed class Model
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("manufacturer")] public Manufacturer? Manufacturer { get; set; }
    [JsonProperty("image")] public string? Image { get; set; }
    [JsonProperty("model_number")] public string? ModelNumber { get; set; }
    [JsonProperty("min_amt")] public string? MinAmount { get; set; }
    [JsonProperty("remaining")] public int Remaining { get; set; }
    [JsonProperty("depreciation")] public Depreciation? Depreciation { get; set; }
    [JsonProperty("assets_count")] public int AssetsCount { get; set; }
    [JsonProperty("category")] public Category? Category { get; set; }
    [JsonProperty("fieldset")] public Fieldset? Fieldset { get; set; }
    [JsonProperty("default_fieldset_values")] public List<DefaultFieldsetValue> DefaultFieldsetValues { get; set; } = [];
    [JsonProperty("eol")] public string? EOL { get; set; }
    [JsonProperty("requestable")] public bool Requestable { get; set; }
    [JsonProperty("notes")] public string? Notes { get; set; }
    [JsonProperty("created_by")] public CreatedBy? CreatedBy { get; set; }
    [JsonProperty("created_at")] public DateInfo? CreatedAt { get; set; }
    [JsonProperty("updated_at")] public DateInfo? UpdatedAt { get; set; }
    [JsonProperty("deleted_at")] public string? DeletedAt { get; set; }
    [JsonProperty("available_actions")] public AvailableActions? AvailableActions { get; set; }
}

public sealed class Manufacturer
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
}

public sealed class Depreciation
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
}

public sealed class Category
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
}

public sealed class Fieldset
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
}

public sealed class DefaultFieldsetValue
{
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("db_column_name")] public string DbColumnName { get; set; } = string.Empty;
    [JsonProperty("default_value")] public string DefaultValue { get; set; } = string.Empty;
    [JsonProperty("format")] public string Format { get; set; } = string.Empty;
    [JsonProperty("required")] public bool Required { get; set; }
}

public sealed class CreatedBy
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
}

public sealed class DateInfo
{
    [JsonProperty("datetime")] public string DateTime { get; set; } = string.Empty;
    [JsonProperty("formatted")] public string Formatted { get; set; } = string.Empty;
}

public sealed class AvailableActions
{
    [JsonProperty("update")] public bool Update { get; set; }
    [JsonProperty("delete")] public bool Delete { get; set; }
    [JsonProperty("clone")] public bool Clone { get; set; }
    [JsonProperty("restore")] public bool Restore { get; set; }
}

/// <summary>
/// Snipe-IT hardware asset record used by the sync comparison logic.
/// </summary>
public sealed class Asset
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("asset_tag")] public string AssetTag { get; set; } = string.Empty;
    [JsonProperty("serial")] public string Serial { get; set; } = string.Empty;
    [JsonProperty("model")] public Model Model { get; set; } = new();
    [JsonProperty("model_number")] public string? ModelNumber { get; set; }
    [JsonProperty("status_label")] public StatusLabel StatusLabel { get; set; } = new();
    [JsonProperty("category")] public Category? Category { get; set; }
    [JsonProperty("manufacturer")] public Manufacturer? Manufacturer { get; set; }
    [JsonProperty("supplier")] public Supplier? Supplier { get; set; }
    [JsonProperty("notes")] public string? Notes { get; set; }
    [JsonProperty("order_number")] public string? OrderNumber { get; set; }
    [JsonProperty("company")] public Company? Company { get; set; }
    [JsonProperty("location")] public Location? Location { get; set; }
    [JsonProperty("rtd_location")] public Location? RTDLocation { get; set; }
    [JsonProperty("image")] public string? Image { get; set; }
    [JsonProperty("assigned_to")] public AssignedTo? AssignedTo { get; set; }
    [JsonProperty("warranty")] public string? Warranty { get; set; }
    [JsonProperty("warranty_expires")] public WarrantyExpires? WarrantyExpires { get; set; }
    [JsonProperty("created_at")] public DateInfo? CreatedAt { get; set; }
    [JsonProperty("updated_at")] public DateInfo? UpdatedAt { get; set; }
    [JsonProperty("purchase_date")] public DateInfo? PurchaseDate { get; set; }
    [JsonProperty("last_checkout")] public DateInfo? LastCheckout { get; set; }
    [JsonProperty("expected_checkin")] public DateInfo? ExpectedCheckin { get; set; }
    [JsonProperty("purchase_cost")] public string? PurchaseCost { get; set; }
    [JsonProperty("user_can_checkout")] public bool UserCanCheckout { get; set; }
    [JsonProperty("custom_fields")] public Dictionary<string, CustomField> CustomFields { get; set; } = [];
    [JsonProperty("available_actions")] public AvailableActions? AvailableActions { get; set; }
}

public sealed class WarrantyExpires
{
    [JsonProperty("date")] public string Date { get; set; } = string.Empty;
    [JsonProperty("formatted")] public string Formatted { get; set; } = string.Empty;
}

public sealed class CustomField
{
    [JsonProperty("field")] public string Field { get; set; } = string.Empty;
    [JsonProperty("value")] public string Value { get; set; } = string.Empty;
    [JsonProperty("field_format")] public string FieldFormat { get; set; } = string.Empty;
    [JsonProperty("element")] public string Element { get; set; } = string.Empty;
}

public sealed class StatusLabel
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("status_meta")] public string StatusMeta { get; set; } = string.Empty;
}

public sealed class Supplier
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
}

public sealed class Location
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
}

public sealed class AssignedTo
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("username")] public string Username { get; set; } = string.Empty;
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("first_name")] public string FirstName { get; set; } = string.Empty;
    [JsonProperty("last_name")] public string LastName { get; set; } = string.Empty;
    [JsonProperty("employee_number")] public string EmployeeNumber { get; set; } = string.Empty;
    [JsonProperty("type")] public string Type { get; set; } = string.Empty;
}

public sealed class Company
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
}

public sealed class User
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("email")] public string Email { get; set; } = string.Empty;
}
