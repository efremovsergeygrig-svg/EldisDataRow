using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using EldisDataLoader.Models;

namespace EldisDataLoader.Web.Services;

/// <summary>
/// Сервис для получения списка приборов учёта из БД
/// </summary>
public class DevicesService
{
    private readonly IConfiguration _configuration;

    public DevicesService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Возвращает все приборы из базы данных
    /// </summary>
    public async Task<List<DeviceDto>> GetAllDevicesAsync()
    {
        var devices = new List<DeviceDto>();
        var connectionString = _configuration["Database:ConnectionString"];

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT Id, DeviceName, DeviceModel, AddressObject, ObjectName, 
                   ModemName, DeviceStatus, CreatedOn
            FROM [dbo].[EldisDevices]
            ORDER BY DeviceName";

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            devices.Add(new DeviceDto
            {
                Id = reader.GetGuid(0),
                DeviceName = reader.IsDBNull(1) ? null : reader.GetString(1),
                DeviceModel = reader.IsDBNull(2) ? null : reader.GetString(2),
                AddressObject = reader.IsDBNull(3) ? null : reader.GetString(3),
                ObjectName = reader.IsDBNull(4) ? null : reader.GetString(4),
                ModemName = reader.IsDBNull(5) ? null : reader.GetString(5),
                DeviceStatus = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                CreatedOn = reader.IsDBNull(7) ? null : reader.GetDateTime(7)
            });
        }

        return devices;
    }
}