using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EldisDataLoader.Models;

public class EldisDeviceResponse
{
    [JsonPropertyName("response")]
    public DeviceResponseData Response { get; set; } = new();
}

public class DeviceResponseData
{
    [JsonPropertyName("devices")]
    public DevicesData Devices { get; set; } = new();
}

public class DevicesData
{
    [JsonPropertyName("list")]
    public List<DeviceDto>? List { get; set; }

    [JsonPropertyName("pagination")]
    public PaginationData Pagination { get; set; } = new();
}

public class DeviceDto
{
    [JsonPropertyName("id")]
    public Guid? Id { get; set; } // ИЗМЕНЕНО: Guid? вместо Guid (защита от null в API)

    [JsonPropertyName("modemID")]
    public Guid? ModemId { get; set; }

    [JsonPropertyName("objectID")]
    public Guid? ObjectId { get; set; }

    [JsonPropertyName("deviceStatus")]
    public int? DeviceStatus { get; set; }

    [JsonPropertyName("modemStatus")]
    public int? ModemStatus { get; set; }

    [JsonPropertyName("modemSubStatus")]
    public int? ModemSubStatus { get; set; }

    [JsonPropertyName("lastConnection")]
    [JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime? LastConnection { get; set; }

    [JsonPropertyName("lastConnectionStatusColor")]
    public string? LastConnectionStatusColor { get; set; }

    [JsonPropertyName("modemIsBusy")]
    public bool? ModemIsBusy { get; set; }

    [JsonPropertyName("deviceModel")]
    public string? DeviceModel { get; set; }

    [JsonPropertyName("deviceName")]
    public string? DeviceName { get; set; }

    [JsonPropertyName("addressObject")]
    public string? AddressObject { get; set; }

    [JsonPropertyName("objectName")]
    public string? ObjectName { get; set; }

    [JsonPropertyName("consumer")]
    public string? Consumer { get; set; }

    [JsonPropertyName("modemName")]
    public string? ModemName { get; set; }

    [JsonPropertyName("timeOnDevice")]
    public string? TimeOnDevice { get; set; }

    [JsonPropertyName("impossibleGetExactTime")]
    public bool? ImpossibleGetExactTime { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("dashboardID")]
    public Guid? DashboardId { get; set; }

    [JsonPropertyName("dashboardName")]
    public string? DashboardName { get; set; }

    [JsonPropertyName("createdOn")]
    [JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime? CreatedOn { get; set; }

    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }
}