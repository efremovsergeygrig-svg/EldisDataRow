using System.Data;
using System.Globalization;
using EldisDataLoader.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EldisDataLoader.Services;

/// <summary>
/// Загрузка и сохранение сырых показаний по всем точкам учёта
/// </summary>
public class RawDataService : EldisServiceBase
{
    public override string ServiceName => "Сырые данные";

    public RawDataService(
        ILogger<RawDataService> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
        : base(logger, httpClientFactory, configuration)
    {
    }

    public override async Task ExecuteAsync(string token, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient(token);

        var meteringPoints = await GetMeteringPointsFromDbAsync(cancellationToken);
        var totalPoints = meteringPoints.Count;

        if (totalPoints == 0)
        {
            Logger.LogWarning("[{Service}] Нет точек учёта в БД для загрузки", ServiceName);
            return;
        }

        var yesterday = DateTime.Today.AddDays(-1);
        var startDate = yesterday.ToString("dd.MM.yyyy") + " 00:00:00";
        var endDate = yesterday.ToString("dd.MM.yyyy") + " 23:59:59";

        Logger.LogInformation("[{Service}] === НАЧАЛО СБОРА СЫРЫХ ДАННЫХ ===", ServiceName);
        Logger.LogInformation("[{Service}] Всего точек: {Count}", ServiceName, totalPoints);
        Logger.LogInformation("[{Service}] Дата запроса: {Date} (вчера)", ServiceName, yesterday.ToString("dd.MM.yyyy"));
        Logger.LogInformation("[{Service}] Ожидаемое время: ~{Minutes} минут", ServiceName, totalPoints * 2 / 60);

        const int typeDataCode = 30004;
        const string displayAll = "false";

        int totalSaved = 0;
        int totalEmpty = 0;
        int totalErrors = 0;
        int pointIndex = 0;

        await using var dbConnection = new SqlConnection(ConnectionString);
        await dbConnection.OpenAsync(cancellationToken);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        foreach (var (pointId, sn) in meteringPoints)
        {
            pointIndex++;

            if (cancellationToken.IsCancellationRequested)
            {
                Logger.LogWarning("[{Service}] Операция отменена пользователем", ServiceName);
                break;
            }

            try
            {
                var queryString = $"?id={Uri.EscapeDataString(pointId.ToString())}" +
                                  $"&startDate={Uri.EscapeDataString(startDate)}" +
                                  $"&endDate={Uri.EscapeDataString(endDate)}" +
                                  $"&typeDataCode={typeDataCode}" +
                                  $"&displayAll={displayAll}";

                var apiResponse = await GetAndDeserializeAsync<EldisRawDataResponse>(
                    client, $"/v3/data/rawData{queryString}", cancellationToken);

                if (apiResponse?.Response?.Data?.RawData == null || apiResponse.Response.Data.RawData.Count == 0)
                {
                    totalEmpty++;
                    await Task.Delay(200, cancellationToken);
                    continue;
                }

                var rawDataList = apiResponse.Response.Data.RawData;

                foreach (var data in rawDataList)
                {
                    await SaveRawDataAsync(dbConnection, pointId, data, typeDataCode, startDate, endDate, cancellationToken);
                    totalSaved++;
                }

                if (pointIndex % 100 == 0 || pointIndex == totalPoints)
                {
                    var elapsed = stopwatch.Elapsed;
                    var speed = pointIndex / elapsed.TotalMinutes;
                    var remaining = TimeSpan.FromMinutes((totalPoints - pointIndex) / speed);

                    Logger.LogInformation(
                        "[{Service}] [{Current}/{Total}] Сохранено: {Saved} | Пусто: {Empty} | Ошибки: {Errors} | Скорость: {Speed:F1} т/мин | Осталось: {Remaining}",
                        ServiceName, pointIndex, totalPoints, totalSaved, totalEmpty, totalErrors, speed, remaining);
                }

                await Task.Delay(200, cancellationToken);
            }
            catch (Exception ex)
            {
                totalErrors++;
                if (totalErrors <= 10)
                {
                    Logger.LogError(ex, "[{Service}] Ошибка для точки {Sn}", ServiceName, sn);
                }
            }
        }

        stopwatch.Stop();

        Logger.LogInformation("[{Service}] === СБОР ЗАВЕРШЁН ===", ServiceName);
        Logger.LogInformation("[{Service}] Обработано точек: {Count}", ServiceName, pointIndex);
        Logger.LogInformation("[{Service}] Сохранено записей: {Count}", ServiceName, totalSaved);
        Logger.LogInformation("[{Service}] Точек без данных: {Count}", ServiceName, totalEmpty);
        Logger.LogInformation("[{Service}] Ошибок: {Count}", ServiceName, totalErrors);
        Logger.LogInformation("[{Service}] Общее время: {Time}", ServiceName, stopwatch.Elapsed);
    }

    private async Task<List<(Guid Id, string Sn)>> GetMeteringPointsFromDbAsync(CancellationToken cancellationToken)
    {
        var points = new List<(Guid Id, string Sn)>();
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = "SELECT Id, ISNULL(SerialNumber, 'N/A') AS SerialNumber FROM [dbo].[EldisMeteringPoints]";
        await using var cmd = new SqlCommand(sql, connection);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            points.Add((reader.GetGuid(0), reader.GetString(1)));
        }

        return points;
    }

    private async Task SaveRawDataAsync(
     SqlConnection connection,
     Guid pointId,
     EldisRawDataDto data,
     int typeDataCode,
     string startDate,
     string endDate,
     CancellationToken cancellationToken)
    {
        const string sql = @"
            MERGE INTO [dbo].[EldisRawData] AS target
            USING (SELECT @MeteringPointId AS MeteringPointId, @DtMeasure AS DtMeasure, @TypeDataCode AS TypeDataCode) AS source
            ON target.MeteringPointId = source.MeteringPointId 
               AND target.DtMeasure = source.DtMeasure 
               AND target.TypeDataCode = source.TypeDataCode
            WHEN MATCHED THEN 
                UPDATE SET 
                    M_Itog1_TR = @M_Itog1_TR, M_Itog2_TR = @M_Itog2_TR, Mg_Itog_TV = @Mg_Itog_TV,
                    Qg_Itog_TV = @Qg_Itog_TV, V_Itog1_TR = @V_Itog1_TR, V_Itog2_TR = @V_Itog2_TR,
                    t1_TR = @t1_TR, t2_TR = @t2_TR, V1_TR = @V1_TR, V2_TR = @V2_TR,
                    M1_TR = @M1_TR, M2_TR = @M2_TR, P1_TR = @P1_TR, P2_TR = @P2_TR,
                    Mg_TV = @Mg_TV, Qo_TV = @Qo_TV, Qg_TV = @Qg_TV, Qo_Itog_TV = @Qo_Itog_TV,
                    dt_TV = @dt_TV, tsw_TV = @tsw_TV, ta_TV = @ta_TV, QntHIP_TV = @QntHIP_TV,
                    QntP_TV = @QntP_TV, QntHIP_Itog_TV = @QntHIP_Itog_TV, NS_TV = @NS_TV, DI_TV = @DI_TV,
                    StartDate = @StartDate, EndDate = @EndDate, ImportedAt = GETDATE()
            WHEN NOT MATCHED THEN 
                INSERT (MeteringPointId, DtMeasure, M_Itog1_TR, M_Itog2_TR, Mg_Itog_TV, Qg_Itog_TV,
                        V_Itog1_TR, V_Itog2_TR, t1_TR, t2_TR, V1_TR, V2_TR, M1_TR, M2_TR, P1_TR, P2_TR,
                        Mg_TV, Qo_TV, Qg_TV, Qo_Itog_TV, dt_TV, tsw_TV, ta_TV, QntHIP_TV, QntP_TV,
                        QntHIP_Itog_TV, NS_TV, DI_TV, TypeDataCode, StartDate, EndDate, ImportedAt)
                VALUES (@MeteringPointId, @DtMeasure, @M_Itog1_TR, @M_Itog2_TR, @Mg_Itog_TV, @Qg_Itog_TV,
                        @V_Itog1_TR, @V_Itog2_TR, @t1_TR, @t2_TR, @V1_TR, @V2_TR, @M1_TR, @M2_TR, @P1_TR, @P2_TR,
                        @Mg_TV, @Qo_TV, @Qg_TV, @Qo_Itog_TV, @dt_TV, @tsw_TV, @ta_TV, @QntHIP_TV, @QntP_TV,
                        @QntHIP_Itog_TV, @NS_TV, @DI_TV, @TypeDataCode, @StartDate, @EndDate, GETDATE());";

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@MeteringPointId", pointId);
        command.Parameters.AddWithValue("@DtMeasure", data.DtMeasure);
        command.Parameters.AddWithValue("@TypeDataCode", typeDataCode);
        command.Parameters.AddWithValue("@StartDate", DateTime.Parse(startDate));
        command.Parameters.AddWithValue("@EndDate", DateTime.Parse(endDate));

        // 🛡️ ИСПРАВЛЕНИЕ: Передаем числа напрямую. ADO.NET и SQL Server сами корректно сохранят double.
        AddDoubleParam(command, "@M_Itog1_TR", data.M_Itog1_TR);
        AddDoubleParam(command, "@M_Itog2_TR", data.M_Itog2_TR);
        AddDoubleParam(command, "@Mg_Itog_TV", data.Mg_Itog_TV);
        AddDoubleParam(command, "@Qg_Itog_TV", data.Qg_Itog_TV);
        AddDoubleParam(command, "@V_Itog1_TR", data.V_Itog1_TR);
        AddDoubleParam(command, "@V_Itog2_TR", data.V_Itog2_TR);
        AddDoubleParam(command, "@t1_TR", data.t1_TR);
        AddDoubleParam(command, "@t2_TR", data.t2_TR);
        AddDoubleParam(command, "@V1_TR", data.V1_TR);
        AddDoubleParam(command, "@V2_TR", data.V2_TR);
        AddDoubleParam(command, "@M1_TR", data.M1_TR);
        AddDoubleParam(command, "@M2_TR", data.M2_TR);
        AddDoubleParam(command, "@P1_TR", data.P1_TR);
        AddDoubleParam(command, "@P2_TR", data.P2_TR);
        AddDoubleParam(command, "@Mg_TV", data.Mg_TV);
        AddDoubleParam(command, "@Qo_TV", data.Qo_TV);
        AddDoubleParam(command, "@Qg_TV", data.Qg_TV);
        AddDoubleParam(command, "@Qo_Itog_TV", data.Qo_Itog_TV);
        AddDoubleParam(command, "@dt_TV", data.dt_TV);
        AddDoubleParam(command, "@tsw_TV", data.tsw_TV);
        AddDoubleParam(command, "@ta_TV", data.ta_TV);
        AddDoubleParam(command, "@QntHIP_TV", data.QntHIP_TV);
        AddDoubleParam(command, "@QntP_TV", data.QntP_TV);
        AddDoubleParam(command, "@QntHIP_Itog_TV", data.QntHIP_Itog_TV);
        AddDoubleParam(command, "@DI_TV", data.DI_TV);

        command.Parameters.AddWithValue("@NS_TV", data.NS_TV.HasValue ? (object)data.NS_TV.Value : DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// 🛡️ ИСПРАВЛЕННЫЙ МЕТОД: Передает значение double напрямую, а не как строку VarChar.
    /// Это самый надежный способ работы с числами в ADO.NET.
    /// </summary>
    private void AddDoubleParam(SqlCommand command, string paramName, double? value)
    {
        if (value.HasValue)
        {
            command.Parameters.AddWithValue(paramName, (object)value.Value);
        }
        else
        {
            command.Parameters.AddWithValue(paramName, DBNull.Value);
        }
    }
}