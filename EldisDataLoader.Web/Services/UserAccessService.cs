using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EldisDataLoader.Web.Services;

/// <summary>
/// Сервис для проверки доступа пользователей к системе
/// </summary>
public class UserAccessService
{
    private readonly IConfiguration _configuration;

    public UserAccessService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Проверяет, есть ли пользователь в списке разрешённых
    /// </summary>
    public async Task<(bool IsAllowed, string? FullName, string? Role)> CheckAccessAsync(string domainLogin)
    {
        var connectionString = _configuration["Database:ConnectionString"];

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Ищем пользователя по логину (учитываем оба варианта: DOMAIN\user и просто user)
        const string sql = @"
            UPDATE [dbo].[EldisUsers]
            SET [LastLoginAt] = SYSUTCDATETIME()
            OUTPUT INSERTED.[FullName], INSERTED.[Role], INSERTED.[IsActive]
            WHERE [DomainLogin] = @Login AND [IsActive] = 1";

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Login", domainLogin);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            var fullName = reader.IsDBNull(0) ? null : reader.GetString(0);
            var role = reader.IsDBNull(1) ? "User" : reader.GetString(1);
            var isActive = reader.GetBoolean(2);

            return (isActive, fullName, role);
        }

        return (false, null, null);
    }
}