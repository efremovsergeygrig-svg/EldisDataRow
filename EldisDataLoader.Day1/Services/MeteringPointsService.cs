using System.Data;
using EldisDataLoader.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EldisDataLoader.Services;

/// <summary>
/// Загрузка и сохранение списка точек учёта
/// </summary>
public class MeteringPointsService : EldisServiceBase
{
    public override string ServiceName => "Точки учёта";

    public MeteringPointsService(
        ILogger<MeteringPointsService> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
        : base(logger, httpClientFactory, configuration)
    {
    }

    public override async Task ExecuteAsync(string token, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient(token);

        Logger.LogInformation("[{Service}] Начало загрузки...", ServiceName);

        var apiResponse = await PostAndDeserializeAsync<EldisApiResponse>(
            client, "/v1/tv/listForDevelopment", null, cancellationToken);

        if (apiResponse?.Response?.Tv?.ListForDevelopment == null)
        {
            Logger.LogWarning("[{Service}] Не удалось получить список точек учёта", ServiceName);
            return;
        }

        var points = apiResponse.Response.Tv.ListForDevelopment;
        Logger.LogInformation("[{Service}] Получено {Count} точек учёта", ServiceName, points.Count);

        await SavePointsAsync(points, cancellationToken);

        Logger.LogInformation("[{Service}] Успешно сохранено {Count} точек учёта", ServiceName, points.Count);
    }

    private async Task SavePointsAsync(List<MeteringPointDto> points, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);

        foreach (var point in points)
        {
            // Проверяем существование ObjectId и DeviceId (защита от FK-конфликтов)
            const string sql = @"
                MERGE INTO [dbo].[EldisMeteringPoints] AS target
                USING (SELECT @Id AS Id) AS source
                ON target.Id = source.Id
                WHEN MATCHED THEN 
                    UPDATE SET 
                        DeviceId = CASE WHEN EXISTS(SELECT 1 FROM [dbo].[EldisDevices] WHERE Id = @DeviceId) THEN @DeviceId ELSE NULL END,
                        ObjectId = CASE WHEN EXISTS(SELECT 1 FROM [dbo].[EldisObjects] WHERE Id = @ObjectId) THEN @ObjectId ELSE NULL END,
                        Status = @Status, Identifier = @Identifier, Identifier2 = @Identifier2, Address = @Address,
                        DeviceName = @DeviceName, DeviceCode = @DeviceCode, ModelModificationId = @ModelModificationId,
                        ModelModificationName = @ModelModificationName, CustomModelName = @CustomModelName,
                        SerialNumber = @Sn, ResourceId = @ResourceId, ResourceCode = @ResourceCode,
                        ResourceName = @ResourceName, MeasurePointNumber = @MeasurePointNumber,
                        MeasurePointName = @MeasurePointName, IsHeat = @IsHeat, IsGvs = @IsGvs,
                        SystemHeat = @SystemHeat, SystemGvs = @SystemGvs, SchemeGvs = @SchemeGvs,
                        InputConfiguration = @InputConfiguration, CreatedOn = @CreatedOn,
                        Description = @Description, IsReadOnly = @IsReadOnly, IsArchivalRecord = @IsArchivalRecord,
                        MeteoStationId = @MeteoStationId, ModemSn = @ModemSn, ModemModelName = @ModemModelName,
                        ModemModelCode = @ModemModelCode, IsAccounting = @IsAccounting,
                        ImportedAt = SYSUTCDATETIME()
                WHEN NOT MATCHED THEN 
                    INSERT (Id, DeviceId, ObjectId, Status, Identifier, Identifier2, Address, DeviceName, DeviceCode, 
                            ModelModificationId, ModelModificationName, CustomModelName, SerialNumber, ResourceId, 
                            ResourceCode, ResourceName, MeasurePointNumber, MeasurePointName, IsHeat, IsGvs, 
                            SystemHeat, SystemGvs, SchemeGvs, InputConfiguration, CreatedOn, Description, 
                            IsReadOnly, IsArchivalRecord, MeteoStationId, ModemSn, ModemModelName, ModemModelCode, 
                            IsAccounting, ImportedAt)
                    VALUES (@Id, 
                            CASE WHEN EXISTS(SELECT 1 FROM [dbo].[EldisDevices] WHERE Id = @DeviceId) THEN @DeviceId ELSE NULL END,
                            CASE WHEN EXISTS(SELECT 1 FROM [dbo].[EldisObjects] WHERE Id = @ObjectId) THEN @ObjectId ELSE NULL END,
                            @Status, @Identifier, @Identifier2, @Address, @DeviceName, @DeviceCode, 
                            @ModelModificationId, @ModelModificationName, @CustomModelName, @Sn, @ResourceId, 
                            @ResourceCode, @ResourceName, @MeasurePointNumber, @MeasurePointName, @IsHeat, @IsGvs, 
                            @SystemHeat, @SystemGvs, @SchemeGvs, @InputConfiguration, @CreatedOn, @Description, 
                            @IsReadOnly, @IsArchivalRecord, @MeteoStationId, @ModemSn, @ModemModelName, @ModemModelCode, 
                            @IsAccounting, SYSUTCDATETIME());";

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", point.Id);
            command.Parameters.AddWithValue("@DeviceId", point.DeviceId.HasValue ? (object)point.DeviceId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ObjectId", point.ObjectId.HasValue ? (object)point.ObjectId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Status", point.Status.HasValue ? (object)point.Status.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Identifier", point.Identifier ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Identifier2", point.Identifier2 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Address", point.Address ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DeviceName", point.DeviceName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DeviceCode", point.DeviceCode.HasValue ? (object)point.DeviceCode.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ModelModificationId", point.ModelModificationId.HasValue ? (object)point.ModelModificationId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ModelModificationName", point.ModelModificationName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CustomModelName", point.CustomModelName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Sn", point.Sn ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ResourceId", point.ResourceId.HasValue ? (object)point.ResourceId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ResourceCode", point.ResourceCode.HasValue ? (object)point.ResourceCode.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ResourceName", point.ResourceName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@MeasurePointNumber", point.MeasurePointNumber ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@MeasurePointName", point.MeasurePointName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsHeat", point.IsHeat.HasValue ? (object)point.IsHeat.Value : DBNull.Value);
            command.Parameters.AddWithValue("@IsGvs", point.IsGvs.HasValue ? (object)point.IsGvs.Value : DBNull.Value);
            command.Parameters.AddWithValue("@SystemHeat", point.SystemHeat.HasValue ? (object)point.SystemHeat.Value : DBNull.Value);
            command.Parameters.AddWithValue("@SystemGvs", point.SystemGvs.HasValue ? (object)point.SystemGvs.Value : DBNull.Value);
            command.Parameters.AddWithValue("@SchemeGvs", point.SchemeGvs.HasValue ? (object)point.SchemeGvs.Value : DBNull.Value);
            command.Parameters.AddWithValue("@InputConfiguration", point.InputConfiguration.HasValue ? (object)point.InputConfiguration.Value : DBNull.Value);
            command.Parameters.AddWithValue("@CreatedOn", point.CreatedOn.HasValue ? (object)point.CreatedOn.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Description", point.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsReadOnly", point.IsReadOnly);
            command.Parameters.AddWithValue("@IsArchivalRecord", point.IsArchivalRecord);
            command.Parameters.AddWithValue("@MeteoStationId", point.MeteoStationId.HasValue ? (object)point.MeteoStationId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ModemSn", point.ModemSn ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ModemModelName", point.ModemModelName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ModemModelCode", point.ModemModelCode.HasValue ? (object)point.ModemModelCode.Value : DBNull.Value);
            command.Parameters.AddWithValue("@IsAccounting", point.IsAccounting.HasValue ? (object)point.IsAccounting.Value : DBNull.Value);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}