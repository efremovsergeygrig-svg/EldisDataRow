using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EldisDataLoader.Models;

public class EldisRawDataDto
{
    [JsonPropertyName("dtMeasure")] public DateTime DtMeasure { get; set; }

    [JsonPropertyName("M_Itog1_TR")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? M_Itog1_TR { get; set; }

    [JsonPropertyName("M_Itog2_TR")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? M_Itog2_TR { get; set; }

    [JsonPropertyName("Mg_Itog_TV")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? Mg_Itog_TV { get; set; }

    [JsonPropertyName("Qg_Itog_TV")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? Qg_Itog_TV { get; set; }

    [JsonPropertyName("V_Itog1_TR")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? V_Itog1_TR { get; set; }

    [JsonPropertyName("V_Itog2_TR")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? V_Itog2_TR { get; set; }

    [JsonPropertyName("t1_TR")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? t1_TR { get; set; }

    [JsonPropertyName("t2_TR")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? t2_TR { get; set; }

    [JsonPropertyName("V1_TR")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? V1_TR { get; set; }

    [JsonPropertyName("V2_TR")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? V2_TR { get; set; }

    [JsonPropertyName("M1_TR")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? M1_TR { get; set; }

    [JsonPropertyName("M2_TR")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? M2_TR { get; set; }

    [JsonPropertyName("P1_TR")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? P1_TR { get; set; }

    [JsonPropertyName("P2_TR")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? P2_TR { get; set; }

    [JsonPropertyName("Mg_TV")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? Mg_TV { get; set; }

    [JsonPropertyName("Qo_TV")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? Qo_TV { get; set; }

    [JsonPropertyName("Qg_TV")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? Qg_TV { get; set; }

    [JsonPropertyName("Qo_Itog_TV")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? Qo_Itog_TV { get; set; }

    [JsonPropertyName("dt_TV")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? dt_TV { get; set; }

    [JsonPropertyName("tsw_TV")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? tsw_TV { get; set; }

    [JsonPropertyName("ta_TV")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? ta_TV { get; set; }

    [JsonPropertyName("QntHIP_TV")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? QntHIP_TV { get; set; }

    [JsonPropertyName("QntP_TV")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? QntP_TV { get; set; }

    [JsonPropertyName("QntHIP_Itog_TV")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? QntHIP_Itog_TV { get; set; }

    [JsonPropertyName("NS_TV")]
    [JsonConverter(typeof(NullableBoolConverter))]
    public bool? NS_TV { get; set; }

    [JsonPropertyName("DI_TV")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? DI_TV { get; set; }
}

public class EldisRawDataResponse
{
    [JsonPropertyName("response")]
    public RawDataResponseData Response { get; set; }
}

public class RawDataResponseData
{
    [JsonPropertyName("data")]
    public RawDataData Data { get; set; }
}

public class RawDataData
{
    [JsonPropertyName("rawData")]
    public List<EldisRawDataDto> RawData { get; set; }
}