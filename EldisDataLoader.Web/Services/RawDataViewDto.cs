namespace EldisDataLoader.Web.Services;

/// <summary>
/// Упрощённая модель сырых данных для отображения на странице
/// </summary>
public class RawDataViewDto
{
    public int RowNumber { get; set; }

    public int Link { get; set; }
    public string? SerialNumber { get; set; }
    public string? DeviceName { get; set; }
    public string? Address { get; set; }
    public DateTime? DtMeasure { get; set; }
    public int? TypeDataCode { get; set; }

    // Тепловая энергия и масса (накопительные)
    public double? QgItogTv { get; set; }
    public double? MgItogTv { get; set; }

    // Температуры
    public double? T1 { get; set; }
    public double? T2 { get; set; }
    public double? Tsw { get; set; }
    public double? Ta { get; set; }

    // Давления
    public double? P1 { get; set; }
    public double? P2 { get; set; }

    // 🟢 НОВЫЕ ПОЛЯ: Расход за период (как в зеленых кружочках)
    public double? V1Tr { get; set; }
    public double? V2Tr { get; set; }
    public double? M1Tr { get; set; }
    public double? M2Tr { get; set; }

    // Статус API
    public string? ApiStatusMessage { get; set; }
}