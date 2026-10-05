namespace EldisDataLoader.Models;

/// <summary>
/// Модель данных для сохранения показаний прибора учёта в БД
/// </summary>
public class MeterReadingEntity
{
    /// <summary>
    /// Номер/идентификатор прибора учёта
    /// </summary>
    public string DeviceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Дата и время снятия показания
    /// </summary>
    public DateTime ReadingTimestamp { get; set; }

    /// <summary>
    /// Значение показания
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Единица измерения
    /// </summary>
    public string Unit { get; set; } = string.Empty;
}