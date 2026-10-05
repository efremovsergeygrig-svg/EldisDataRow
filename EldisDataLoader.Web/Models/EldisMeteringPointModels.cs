using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EldisDataLoader.Models;

public class EldisApiResponse
{
    [JsonPropertyName("response")]
    public ResponseData Response { get; set; }
}

public class ResponseData
{
    [JsonPropertyName("tv")]
    public TvData Tv { get; set; }

    [JsonPropertyName("messages")]
    public List<object> Messages { get; set; }
}

public class TvData
{
    [JsonPropertyName("listForDevelopment")]
    public List<MeteringPointDto> ListForDevelopment { get; set; }
}

public class MeteringPointDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("deviceID")] public Guid? DeviceId { get; set; }
    [JsonPropertyName("objectID")] public Guid? ObjectId { get; set; }
    [JsonPropertyName("status")] public int? Status { get; set; }
    [JsonPropertyName("identifier")] public string Identifier { get; set; }
    [JsonPropertyName("identifier2")] public string Identifier2 { get; set; }
    [JsonPropertyName("address")] public string Address { get; set; }
    [JsonPropertyName("deviceName")] public string DeviceName { get; set; }
    [JsonPropertyName("deviceCode")] public int? DeviceCode { get; set; }
    [JsonPropertyName("modelModificationID")] public Guid? ModelModificationId { get; set; }
    [JsonPropertyName("modelModificationName")] public string ModelModificationName { get; set; }
    [JsonPropertyName("customModelName")] public string CustomModelName { get; set; }
    [JsonPropertyName("sn")] public string Sn { get; set; }
    [JsonPropertyName("resourceID")] public Guid? ResourceId { get; set; }
    [JsonPropertyName("resourceCode")] public int? ResourceCode { get; set; }
    [JsonPropertyName("resourceName")] public string ResourceName { get; set; }
    [JsonPropertyName("measurePointNumber")] public string MeasurePointNumber { get; set; }
    [JsonPropertyName("measurePointName")] public string MeasurePointName { get; set; }
    [JsonPropertyName("isHeat")] public bool? IsHeat { get; set; }
    [JsonPropertyName("isGVS")] public bool? IsGvs { get; set; }
    [JsonPropertyName("systemHeat")] public int? SystemHeat { get; set; }
    [JsonPropertyName("systemGVS")] public int? SystemGvs { get; set; }
    [JsonPropertyName("schemeGVS")] public int? SchemeGvs { get; set; }
    [JsonPropertyName("inputConfiguration")] public int? InputConfiguration { get; set; }

    [JsonPropertyName("createdOn")]
    [JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime? CreatedOn { get; set; }

    [JsonPropertyName("description")] public string Description { get; set; }
    [JsonPropertyName("readOnly")] public bool IsReadOnly { get; set; }
    [JsonPropertyName("isArchivalRecord")] public bool IsArchivalRecord { get; set; }
    [JsonPropertyName("meteoStationID")] public Guid? MeteoStationId { get; set; }
    [JsonPropertyName("modemSN")] public string ModemSn { get; set; }
    [JsonPropertyName("modemModelName")] public string ModemModelName { get; set; }
    [JsonPropertyName("modemModelCode")] public int? ModemModelCode { get; set; }
    [JsonPropertyName("isAccounting")] public bool? IsAccounting { get; set; }
}