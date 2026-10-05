using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EldisDataLoader.Web.Services;

/// <summary>
/// Сервис для получения сырых данных из БД с JOIN к точкам учёта
/// </summary>
public class RawDataQueryService
{
    private readonly IConfiguration _configuration;

    public RawDataQueryService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Возвращает сырые данные за указанную дату (без ограничения TOP 1000)
    /// </summary>
    public async Task<List<RawDataViewDto>> GetRawDataByDateAsync(DateTime date)
    {
        var result = new List<RawDataViewDto>();
        var connectionString = _configuration["Database:ConnectionString"];

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT 
                rd.[LINK],
                mp.[SerialNumber],
                mp.[DeviceName],
                mp.[Address],
                rd.[DtMeasure],
                rd.[TypeDataCode],
                rd.[Qg_Itog_TV],
                rd.[Mg_Itog_TV],
                rd.[V1_TR],
                rd.[V2_TR],
                rd.[M1_TR],
                rd.[M2_TR],
                rd.[t1_TR],
                rd.[t2_TR],
                rd.[tsw_TV],
                rd.[ta_TV],
                rd.[P1_TR],
                rd.[P2_TR],
                st.[ApiStatusMessage]
            FROM [dbo].[EldisRawData] rd
            LEFT JOIN [dbo].[EldisMeteringPoints] mp ON rd.[MeteringPointId] = mp.[Id]
            LEFT JOIN [dbo].[EldisRawDataApiStatus] st  
                ON rd.[MeteringPointId] = st.[MeteringPointId] 
                AND CAST(rd.[DtMeasure] AS DATE) = CAST(st.[DtMeasure] AS DATE)
                AND rd.[TypeDataCode] = st.[TypeDataCode]
            WHERE rd.[DtMeasure] >= @StartDate AND rd.[DtMeasure] < @EndDate
            ORDER BY rd.[DtMeasure] DESC, rd.[LINK] DESC";

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@StartDate", date.Date);
        command.Parameters.AddWithValue("@EndDate", date.Date.AddDays(1));

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new RawDataViewDto
            {
                Link = reader.GetInt32(0),
                SerialNumber = reader.IsDBNull(1) ? null : reader.GetString(1),
                DeviceName = reader.IsDBNull(2) ? null : reader.GetString(2),
                Address = reader.IsDBNull(3) ? null : reader.GetString(3),
                DtMeasure = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                TypeDataCode = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                QgItogTv = ParseDouble(reader, 6),
                MgItogTv = ParseDouble(reader, 7),
                V1Tr = ParseDouble(reader, 8),
                V2Tr = ParseDouble(reader, 9),
                M1Tr = ParseDouble(reader, 10),
                M2Tr = ParseDouble(reader, 11),
                T1 = ParseDouble(reader, 12),
                T2 = ParseDouble(reader, 13),
                Tsw = ParseDouble(reader, 14),
                Ta = ParseDouble(reader, 15),
                P1 = ParseDouble(reader, 16),
                P2 = ParseDouble(reader, 17),
                ApiStatusMessage = reader.IsDBNull(18) ? null : reader.GetString(18)
            });
        }

        for (int i = 0; i < result.Count; i++)
        {
            result[i].RowNumber = i + 1;
        }

        Console.WriteLine($"[DEBUG] Из базы пришло записей: {result.Count} для даты {date:dd.MM.yyyy}");

        return result;
    }

    /// <summary>
    /// Вспомогательный метод для безопасного парсинга чисел из текстовых полей БД.
    /// </summary>
    private static double? ParseDouble(SqlDataReader reader, int ordinal)
    {
        if (reader.IsDBNull(ordinal))
            return null;

        var value = reader.GetValue(ordinal);

        if (value is double d) return d;
        if (value is decimal dec) return (double)dec;
        if (value is int i) return i;
        if (value is long l) return l;
        if (value is float f) return f;

        if (value is string str && !string.IsNullOrWhiteSpace(str))
        {
            if (double.TryParse(str, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }

            if (double.TryParse(str, System.Globalization.NumberStyles.Any,
                new System.Globalization.CultureInfo("ru-RU"), out result))
            {
                return result;
            }
        }

        return null;
    }

    /// <summary>
    /// Возвращает статистику загрузки сырых данных по дням
    /// </summary>
    public async Task<List<RawDataStatDto>> GetRawDataStatsAsync()
    {
        var result = new List<RawDataStatDto>();
        var connectionString = _configuration["Database:ConnectionString"];

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT 
                CAST([DtMeasure] AS DATE) AS MeasureDate,
                MAX([ImportedAt]) AS ImportedAt,
                COUNT(*) AS RecordCount
            FROM [dbo].[EldisRawData]
            GROUP BY CAST([DtMeasure] AS DATE)
            ORDER BY MeasureDate DESC";

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new RawDataStatDto
            {
                MeasureDate = reader.GetDateTime(0),
                ImportedAt = reader.GetDateTime(1),
                RecordCount = reader.GetInt32(2)
            });
        }

        return result;
    }

    /// <summary>
    /// Возвращает прогресс загрузки исторических данных
    /// </summary>
    public async Task<List<HistoryProgressDto>> GetHistoryProgressAsync()
    {
        var result = new List<HistoryProgressDto>();
        var connectionString = _configuration["Database:ConnectionString"];

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT TOP 30
                CAST([DtMeasure] AS DATE) AS MeasureDate,
                COUNT(*) AS RecordCount,
                MAX([ImportedAt]) AS LastUpdate
            FROM [dbo].[EldisRawData]
            GROUP BY CAST([DtMeasure] AS DATE)
            ORDER BY MeasureDate DESC";

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new HistoryProgressDto
            {
                MeasureDate = reader.GetDateTime(0),
                RecordCount = reader.GetInt32(1),
                LastUpdate = reader.IsDBNull(2) ? null : reader.GetDateTime(2)
            });
        }

        return result;
    }

    /// <summary>
    /// Возвращает сводную статистику для карточек на странице прогресса
    /// </summary>
    public async Task<HistorySummaryDto> GetHistorySummaryAsync()
    {
        var connectionString = _configuration["Database:ConnectionString"];

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT 
                COUNT(DISTINCT CAST([DtMeasure] AS DATE)) AS TotalDays,
                COUNT(*) AS TotalRecords,
                MIN(CAST([DtMeasure] AS DATE)) AS OldestDate,
                MAX(CAST([DtMeasure] AS DATE)) AS NewestDate
            FROM [dbo].[EldisRawData]";

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new HistorySummaryDto
            {
                TotalDays = reader.GetInt32(0),
                TotalRecords = reader.GetInt64(1),
                OldestDate = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                NewestDate = reader.IsDBNull(3) ? null : reader.GetDateTime(3)
            };
        }

        return new HistorySummaryDto();
    }

    /// <summary>
    /// Возвращает время последней записи в базе (для определения "пульса" сервиса)
    /// </summary>
    public async Task<DateTime?> GetLastImportTimeAsync()
    {
        var connectionString = _configuration["Database:ConnectionString"];

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        const string sql = "SELECT MAX([ImportedAt]) FROM [dbo].[EldisRawData]";

        await using var command = new SqlCommand(sql, connection);
        var result = await command.ExecuteScalarAsync();

        if (result == DBNull.Value || result == null)
            return null;

        return Convert.ToDateTime(result);
    }

    /// <summary>
    /// Возвращает топ-5 дней с наименьшим количеством записей (аномалии)
    /// </summary>
    public async Task<List<AnomalyDayDto>> GetAnomalyDaysAsync()
    {
        var result = new List<AnomalyDayDto>();
        var connectionString = _configuration["Database:ConnectionString"];

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT TOP 5
                CAST([DtMeasure] AS DATE) AS MeasureDate,
                COUNT(*) AS RecordCount
            FROM [dbo].[EldisRawData]
            GROUP BY CAST([DtMeasure] AS DATE)
            HAVING COUNT(*) < 500
            ORDER BY RecordCount ASC";

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new AnomalyDayDto
            {
                MeasureDate = reader.GetDateTime(0),
                RecordCount = reader.GetInt32(1)
            });
        }

        return result;
    }

    /// <summary>
    /// Возвращает 10 самых старых дат в базе (хвост загрузки)
    /// </summary>
    public async Task<List<HistoryProgressDto>> GetHistoryTailAsync()
    {
        var result = new List<HistoryProgressDto>();
        var connectionString = _configuration["Database:ConnectionString"];

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT TOP 10
                CAST([DtMeasure] AS DATE) AS MeasureDate,
                COUNT(*) AS RecordCount,
                MAX([ImportedAt]) AS LastUpdate
            FROM [dbo].[EldisRawData]
            GROUP BY CAST([DtMeasure] AS DATE)
            ORDER BY MeasureDate ASC";

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new HistoryProgressDto
            {
                MeasureDate = reader.GetDateTime(0),
                RecordCount = reader.GetInt32(1),
                LastUpdate = reader.IsDBNull(2) ? null : reader.GetDateTime(2)
            });
        }

        return result;
    }
}

/// <summary>
/// DTO для статистики сырых данных по дням
/// </summary>
public class RawDataStatDto
{
    public DateTime MeasureDate { get; set; }
    public DateTime ImportedAt { get; set; }
    public int RecordCount { get; set; }
}

/// <summary>
/// DTO для сводных карточек на странице прогресса
/// </summary>
public class HistorySummaryDto
{
    public int TotalDays { get; set; }
    public long TotalRecords { get; set; }
    public DateTime? OldestDate { get; set; }
    public DateTime? NewestDate { get; set; }
}

/// <summary>
/// DTO для отображения прогресса загрузки истории по дням
/// </summary>
public class HistoryProgressDto
{
    public DateTime MeasureDate { get; set; }
    public int RecordCount { get; set; }
    public DateTime? LastUpdate { get; set; }
    public string StatusText => LastUpdate.HasValue
        ? $"Обновлено: {LastUpdate.Value:dd.MM.yyyy HH:mm:ss}"
        : "Нет данных";
}

/// <summary>
/// DTO для аномальных дней (мало данных)
/// </summary>
public class AnomalyDayDto
{
    public DateTime MeasureDate { get; set; }
    public int RecordCount { get; set; }
}