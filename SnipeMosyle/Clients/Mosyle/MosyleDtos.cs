// Copyright (c) Jordan Molenaar (jordanm.cloud). All rights reserved.

using Newtonsoft.Json;

namespace jordanm.cloud.mosyle.Clients.Mosyle;

/// <summary>
/// Response returned after updating Mosyle device metadata.
/// </summary>
public sealed class DeviceUpdateResponse
{
    [JsonProperty("status")] public string Status { get; set; } = string.Empty;
    [JsonProperty("response")] public List<DeviceUpdateResult> Response { get; set; } = [];
}

public sealed class DeviceUpdateResult
{
    [JsonProperty("status")] public string Status { get; set; } = string.Empty;
    [JsonProperty("info")] public string Info { get; set; } = string.Empty;
}

public sealed class LoginResponse
{
    [JsonProperty("UserId")] public string UserId { get; set; } = string.Empty;
    [JsonProperty("Email")] public string Email { get; set; } = string.Empty;
    public string Authorization { get; set; } = string.Empty;
}

public sealed class DeviceListResponse
{
    [JsonProperty("status")] public string Status { get; set; } = string.Empty;
    [JsonProperty("response")] public List<DeviceResponse> Response { get; set; } = [];
}

public sealed class DeviceResponse
{
    [JsonProperty("devices")] public List<Device> Devices { get; set; } = [];
    [JsonProperty("rows")] public int Rows { get; set; }
    [JsonProperty("page_size")] public int PageSize { get; set; }
    [JsonProperty("page")] public int Page { get; set; }
}

/// <summary>
/// Mosyle device payload. Most fields are passed through from the API as strings.
/// </summary>
public sealed class Device
{
    [JsonProperty("idusermosyle")] public string IdUserMosyle { get; set; } = string.Empty;
    [JsonProperty("idsharedgroup")] public string IdSharedGroup { get; set; } = string.Empty;
    [JsonProperty("deviceudid")] public string DeviceUdid { get; set; } = string.Empty;
    [JsonProperty("enrollment_type")] public string EnrollmentType { get; set; } = string.Empty;
    [JsonProperty("status_login")] public string StatusLogin { get; set; } = string.Empty;
    [JsonProperty("date_lastlogin")] public string DateLastLogin { get; set; } = string.Empty;
    [JsonProperty("idaccount")] public string IdAccount { get; set; } = string.Empty;
    [JsonProperty("os")] public string OS { get; set; } = string.Empty;
    [JsonProperty("total_disk")] public string TotalDisk { get; set; } = string.Empty;
    [JsonProperty("serial_number")] public string SerialNumber { get; set; } = string.Empty;
    [JsonProperty("percent_disk")] public string PercentDisk { get; set; } = string.Empty;
    [JsonProperty("AutoSetupAdminAccounts")] public string AutoSetupAdminAccounts { get; set; } = string.Empty;
    [JsonProperty("date_info")] public string DateInfo { get; set; } = string.Empty;
    [JsonProperty("date_kinfo")] public string DateKInfo { get; set; } = string.Empty;
    [JsonProperty("date_checkin")] public string DateCheckin { get; set; } = string.Empty;
    [JsonProperty("date_enroll")] public string DateEnroll { get; set; } = string.Empty;
    [JsonProperty("date_checkout")] public string DateCheckout { get; set; } = string.Empty;
    [JsonProperty("device_name")] public string DeviceName { get; set; } = string.Empty;
    [JsonProperty("device_model")] public string DeviceModel { get; set; } = string.Empty;
    [JsonProperty("battery")] public string Battery { get; set; } = string.Empty;
    [JsonProperty("osversion")] public string OSVersion { get; set; } = string.Empty;
    [JsonProperty("carrier")] public string Carrier { get; set; } = string.Empty;
    [JsonProperty("roaming_enabled")] public string RoamingEnabled { get; set; } = string.Empty;
    [JsonProperty("isroaming")] public string IsRoaming { get; set; } = string.Empty;
    [JsonProperty("imeiOne")] public string IMEI { get; set; } = string.Empty;
    [JsonProperty("PhoneNumberOne")] public string PhoneNumber { get; set; } = string.Empty;
    [JsonProperty("meidOne")] public string MEID { get; set; } = string.Empty;
    [JsonProperty("eidOne")] public string EID { get; set; } = string.Empty;
    [JsonProperty("available_disk")] public string AvailableDisk { get; set; } = string.Empty;
    [JsonProperty("wifi_mac_address")] public string WifiMacAddress { get; set; } = string.Empty;
    [JsonProperty("bluetooth_mac_address")] public string BluetoothMacAddress { get; set; } = string.Empty;
    [JsonProperty("is_supervised")] public string IsSupervised { get; set; } = string.Empty;
    [JsonProperty("date_app_info")] public string DateAppInfo { get; set; } = string.Empty;
    [JsonProperty("date_last_beat")] public string DateLastBeat { get; set; } = string.Empty;
    [JsonProperty("date_last_push")] public string DateLastPush { get; set; } = string.Empty;
    [JsonProperty("status")] public string Status { get; set; } = string.Empty;
    [JsonProperty("isActivationLockEnabled")] public string IsActivationLockEnabled { get; set; } = string.Empty;
    [JsonProperty("isDeviceLocatorServiceEnabled")] public string IsDeviceLocatorServiceEnabled { get; set; } = string.Empty;
    [JsonProperty("isDoNotDisturbInEffect")] public string IsDoNotDisturbInEffect { get; set; } = string.Empty;
    [JsonProperty("isCloudBackupEnabled")] public string IsCloudBackupEnabled { get; set; } = string.Empty;
    [JsonProperty("IsNetworkTethered")] public string IsNetworkTethered { get; set; } = string.Empty;
    [JsonProperty("needosupdate")] public string NeedOsUpdate { get; set; } = string.Empty;
    [JsonProperty("productkeyupdate")] public string ProductKeyUpdate { get; set; } = string.Empty;
    [JsonProperty("device_type")] public string DeviceType { get; set; } = string.Empty;
    [JsonProperty("lostmode_status")] public string LostModeStatus { get; set; } = string.Empty;
    [JsonProperty("is_muted")] public string IsMuted { get; set; } = string.Empty;
    [JsonProperty("date_muted")] public string DateMuted { get; set; } = string.Empty;
    [JsonProperty("activation_bypass")] public string ActivationBypass { get; set; } = string.Empty;
    [JsonProperty("date_media_info")] public string DateMediaInfo { get; set; } = string.Empty;
    [JsonProperty("tags")] public string Tags { get; set; } = string.Empty;
    [JsonProperty("is_deleted")] public string IsDeleted { get; set; } = string.Empty;
    [JsonProperty("iTunesStoreAccountHash")] public string ITunesStoreAccountHash { get; set; } = string.Empty;
    [JsonProperty("iTunesStoreAccountIsActive")] public string ITunesStoreAccountIsActive { get; set; } = string.Empty;
    [JsonProperty("date_profiles_info")] public string DateProfilesInfo { get; set; } = string.Empty;
    [JsonProperty("ethernet_mac_address")] public string EthernetMacAddress { get; set; } = string.Empty;
    [JsonProperty("model_name")] public string ModelName { get; set; } = string.Empty;
    [JsonProperty("LastCloudBackupDate")] public string LastCloudBackupDate { get; set; } = string.Empty;
    [JsonProperty("SystemIntegrityProtectionEnabled")] public string SystemIntegrityProtectionEnabled { get; set; } = string.Empty;
    [JsonProperty("BuildVersion")] public string BuildVersion { get; set; } = string.Empty;
    [JsonProperty("LocalHostName")] public string LocalHostName { get; set; } = string.Empty;
    [JsonProperty("HostName")] public string HostName { get; set; } = string.Empty;
    [JsonProperty("OSUpdateSettings")] public string OSUpdateSettings { get; set; } = string.Empty;
    [JsonProperty("ActiveManagedUsers")] public string ActiveManagedUsers { get; set; } = string.Empty;
    [JsonProperty("CurrentConsoleManagedUser")] public string CurrentConsoleManagedUser { get; set; } = string.Empty;
    [JsonProperty("date_printers")] public string DatePrinters { get; set; } = string.Empty;
    [JsonProperty("appleTVid")] public string AppleTVId { get; set; } = string.Empty;
    [JsonProperty("asset_tag")] public string AssetTag { get; set; } = string.Empty;
    [JsonProperty("ManagementStatus")] public string ManagementStatus { get; set; } = string.Empty;
    [JsonProperty("OSUpdateStatus")] public string OSUpdateStatus { get; set; } = string.Empty;
    [JsonProperty("AvailableOSUpdates")] public string AvailableOSUpdates { get; set; } = string.Empty;
    [JsonProperty("has_password")] public string HasPassword { get; set; } = string.Empty;
    [JsonProperty("TimeZone")] public string TimeZone { get; set; } = string.Empty;
    [JsonProperty("activation_bypass_mdm")] public string ActivationBypassMdm { get; set; } = string.Empty;
    [JsonProperty("open_direct_device_link")] public string OpenDirectDeviceLink { get; set; } = string.Empty;
    [JsonProperty("device_model_name")] public string DeviceModelName { get; set; } = string.Empty;
}
