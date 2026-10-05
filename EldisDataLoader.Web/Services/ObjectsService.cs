using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using EldisDataLoader.Models;

namespace EldisDataLoader.Web.Services;

/// <summary>
/// Сервис для получения списка объектов из БД
/// </summary>
public class ObjectsService
{
    private readonly IConfiguration _configuration;

    public ObjectsService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Возвращает все объекты из базы данных
    /// </summary>
    public async Task<List<ObjectDto>> GetAllObjectsAsync()
    {
        var objects = new List<ObjectDto>();
        var connectionString = _configuration["Database:ConnectionString"];

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT Id, Name, Address, Description, CreatedOn
            FROM [dbo].[EldisObjects]
            ORDER BY Name";

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            objects.Add(new ObjectDto
            {
                Id = reader.GetGuid(0),
                // ДОБАВЛЕНА ПРОВЕРКА НА NULL:
                Name = reader.IsDBNull(1) ? "Не указано" : reader.GetString(1),
                Address = reader.IsDBNull(2) ? null : reader.GetString(2),
                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                CreatedOn = reader.IsDBNull(4) ? null : reader.GetDateTime(4)
            });
        }

        return objects;
    }
}