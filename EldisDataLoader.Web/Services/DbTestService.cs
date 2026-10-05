using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EldisDataLoader.Web.Services;

/// <summary>
/// Сервис для проверки подключения к базе данных
/// </summary>
public class DbTestService
{
    private readonly IConfiguration _configuration;

    public DbTestService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Проверяет подключение к базе данных
    /// </summary>
    public async Task<bool> TestConnectionAsync()
    {
        var connectionString = _configuration["Database:ConnectionString"];

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Возвращает сообщение о статусе подключения (имя метода исправлено)
    /// </summary>
    public async Task<string> GetDatabaseStatusAsync()
    {
        var isConnected = await TestConnectionAsync();
        return isConnected ? "✅ Подключение к БД успешно" : "❌ Ошибка подключения к БД";
    }
}