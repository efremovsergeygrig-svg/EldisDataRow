using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EldisDataLoader.Models;

public class EldisObjectResponse
{
    [JsonPropertyName("response")]
    public ObjectResponseData Response { get; set; }
}

public class ObjectResponseData
{
    [JsonPropertyName("objects")]
    public ObjectsData Objects { get; set; }
}

public class ObjectsData
{
    [JsonPropertyName("list")]
    public List<ObjectDto> List { get; set; }

    [JsonPropertyName("pagination")]
    public PaginationData Pagination { get; set; }
}

public class PaginationData
{
    [JsonPropertyName("currentPage")]
    public int CurrentPage { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }
}

public class ObjectDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("isWinterMode")]
    public bool IsWinterMode { get; set; }

    [JsonPropertyName("isModeChanged")]
    public bool IsModeChanged { get; set; }

    [JsonPropertyName("address")]
    public string Address { get; set; }

    [JsonPropertyName("typeObject")]
    public TypeObjectDto TypeObject { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("consumer")]
    public string Consumer { get; set; }

    [JsonPropertyName("dashboardID")]
    public Guid? DashboardId { get; set; }

    [JsonPropertyName("dashboardName")]
    public string DashboardName { get; set; }

    [JsonPropertyName("createdOn")]
    [JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime? CreatedOn { get; set; }

    [JsonPropertyName("objectIdentifier")]
    public string ObjectIdentifier { get; set; }

    [JsonPropertyName("hasEvents")]
    public bool? HasEvents { get; set; }

    [JsonPropertyName("activeEvents")]
    public int? ActiveEvents { get; set; }

    [JsonPropertyName("managementOrganizationID")]
    public Guid? ManagementOrganizationId { get; set; }

    [JsonPropertyName("managementOrganizationName")]
    public string ManagementOrganizationName { get; set; }

    // ВАЖНО: Тип изменен на JsonElement?, чтобы избежать ошибок парсинга смешанных типов в массиве
    [JsonPropertyName("tags")]
    public JsonElement? Tags { get; set; }
}

public class TypeObjectDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}