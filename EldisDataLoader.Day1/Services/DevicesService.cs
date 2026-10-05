using System.Data;
using EldisDataLoader.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EldisDataLoader.Services;

/// <summary>
/// Загрузка и сохранение списка приборов учёта
/// </summary>
public class DevicesService : EldisServiceBase
{
    public override string ServiceName => "Приборы учёта";

    public DevicesService(
        ILogger<DevicesService> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
        : base(logger, httpClientFactory, configuration)
    {
    }

    public override async Task ExecuteAsync(string token, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient(token);
        Logger.LogInformation("[{Service}] Начало загрузки...", ServiceName);

        int currentPage = 1;
        int pageSize = 500; // Максимальное значение по документации API
        int totalSaved = 0;

        while (true)
        {
            // ИСПРАВЛЕНО: параметры передаются через URL (query string), а не через form data
            var endpoint = $"/v1/devices/list?limit={pageSize}&page={currentPage}";

            var apiResponse = await PostAndDeserializeAsync<EldisDeviceResponse>(
                client, endpoint, null, cancellationToken);

            if (apiResponse?.Response?.Devices?.List == null || apiResponse.Response.Devices.List.Count == 0)
            {
                break; // Записей больше нет
            }

            var devices = apiResponse.Response.Devices.List;
            Logger.LogInformation("[{Service}] Страница {Page}: получено {Count} приборов",
                ServiceName, currentPage, devices.Count);

            await SaveDevicesAsync(devices, cancellationToken);
            totalSaved += devices.Count;

            // Если записей меньше, чем pageSize, значит это была последняя страница
            if (devices.Count < pageSize)
            {
                break;
            }

            currentPage++;
        }

        Logger.LogInformation("[{Service}] Успешно сохранено {Count} приборов", ServiceName, totalSaved);
    }

    private async Task SaveDevicesAsync(List<DeviceDto> devices, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);

        foreach (var device in devices)
        {
            // Защита от null Id
            if (!device.Id.HasValue)
            {
                Logger.LogWarning("[{Service}] Пропущен прибор без Id: {Name}", ServiceName, device.DeviceName);
                continue;
            }

            const string sql = @"
                MERGE INTO [dbo].[EldisDevices] AS target
                USING (SELECT @Id AS Id) AS source
                ON target.Id = source.Id
                WHEN MATCHED THEN 
                    UPDATE SET 
                        ModemId = @ModemId, 
                        ObjectId = CASE WHEN EXISTS(SELECT 1 FROM [dbo].[EldisObjects] WHERE Id = @ObjectId) THEN @ObjectId ELSE NULL END,
                        DeviceStatus = @DeviceStatus, ModemStatus = @ModemStatus, ModemSubStatus = @ModemSubStatus,
                        LastConnection = @LastConnection, LastConnectionStatusColor = @LastConnectionStatusColor,
                        ModemIsBusy = @ModemIsBusy, DeviceModel = @DeviceModel, DeviceName = @DeviceName,
                        AddressObject = @AddressObject, ObjectName = @ObjectName, Consumer = @Consumer,
                        ModemName = @ModemName, TimeOnDevice = @TimeOnDevice,
                        ImpossibleGetExactTime = @ImpossibleGetExactTime, Description = @Description,
                        DashboardId = @DashboardId, DashboardName = @DashboardName,
                        CreatedOn = @CreatedOn, Latitude = @Latitude, Longitude = @Longitude,
                        ImportedAt = SYSUTCDATETIME()
                WHEN NOT MATCHED THEN 
                    INSERT (Id, ModemId, ObjectId, DeviceStatus, ModemStatus, ModemSubStatus,
                            LastConnection, LastConnectionStatusColor, ModemIsBusy, DeviceModel, DeviceName,
                            AddressObject, ObjectName, Consumer, ModemName, TimeOnDevice,
                            ImpossibleGetExactTime, Description, DashboardId, DashboardName,
                            CreatedOn, Latitude, Longitude, ImportedAt)
                    VALUES (@Id, @ModemId, 
                            CASE WHEN EXISTS(SELECT 1 FROM [dbo].[EldisObjects] WHERE Id = @ObjectId) THEN @ObjectId ELSE NULL END,
                            @DeviceStatus, @ModemStatus, @ModemSubStatus,
                            @LastConnection, @LastConnectionStatusColor, @ModemIsBusy, @DeviceModel, @DeviceName,
                            @AddressObject, @ObjectName, @Consumer, @ModemName, @TimeOnDevice,
                            @ImpossibleGetExactTime, @Description, @DashboardId, @DashboardName,
                            @CreatedOn, @Latitude, @Longitude, SYSUTCDATETIME());";

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", device.Id.Value);
            command.Parameters.AddWithValue("@ModemId", device.ModemId.HasValue ? (object)device.ModemId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ObjectId", device.ObjectId.HasValue ? (object)device.ObjectId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@DeviceStatus", device.DeviceStatus.HasValue ? (object)device.DeviceStatus.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ModemStatus", device.ModemStatus.HasValue ? (object)device.ModemStatus.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ModemSubStatus", device.ModemSubStatus.HasValue ? (object)device.ModemSubStatus.Value : DBNull.Value);
            command.Parameters.AddWithValue("@LastConnection", device.LastConnection.HasValue ? (object)device.LastConnection.Value : DBNull.Value);
            command.Parameters.AddWithValue("@LastConnectionStatusColor", device.LastConnectionStatusColor ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ModemIsBusy", device.ModemIsBusy.HasValue ? (object)device.ModemIsBusy.Value : DBNull.Value);
            command.Parameters.AddWithValue("@DeviceModel", device.DeviceModel ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DeviceName", device.DeviceName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@AddressObject", device.AddressObject ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ObjectName", device.ObjectName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Consumer", device.Consumer ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ModemName", device.ModemName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@TimeOnDevice", device.TimeOnDevice ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ImpossibleGetExactTime", device.ImpossibleGetExactTime.HasValue ? (object)device.ImpossibleGetExactTime.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Description", device.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DashboardId", device.DashboardId.HasValue ? (object)device.DashboardId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@DashboardName", device.DashboardName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CreatedOn", device.CreatedOn.HasValue ? (object)device.CreatedOn.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Latitude", device.Latitude.HasValue ? (object)device.Latitude.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Longitude", device.Longitude.HasValue ? (object)device.Longitude.Value : DBNull.Value);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}