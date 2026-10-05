namespace EldisDataLoader.Web.Services;

/// <summary>
/// Упрощённая модель точки учёта для отображения на странице
/// </summary>
public class MeteringPointViewDto
{
    public Guid Id { get; set; } // ⚠️ СТРОГО Guid, а не int!
    public int Link { get; set; }
    public string? SerialNumber { get; set; }
    public string? DeviceName { get; set; }
    public string? Address { get; set; }
    public string? ResourceName { get; set; }
    public string? MeasurePointName { get; set; }
    public DateTime? CreatedOn { get; set; }
}