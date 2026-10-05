using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EldisDataLoader.Web.Services;

public class DebugTableInfo
{
    public string TableName { get; set; } = "";
    public DateTime? Date { get; set; }
    public int RowCount { get; set; }
}

public class DebugService
{
    private readonly IConfiguration _configuration;

    public DebugService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<List<DebugTableInfo>> GetTableInfoAsync()
    {
        var result = new List<DebugTableInfo>();
        var connectionString = _configuration["Database:ConnectionString"];

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT 'EldisObjects' AS TableName, MAX(ImportedAt) AS [Date], COUNT(*) AS [RowCount] FROM [dbo].[EldisObjects]
            UNION ALL
            SELECT 'EldisDevices', MAX(ImportedAt), COUNT(*) FROM [dbo].[EldisDevices]
            UNION ALL
            SELECT 'EldisMeteringPoints', MAX(ImportedAt), COUNT(*) FROM [dbo].[EldisMeteringPoints]
            UNION ALL
            SELECT 'EldisRawData', MAX(DtMeasure), COUNT(*) FROM [dbo].[EldisRawData]
            UNION ALL
            SELECT 'EldisUsers', MAX(LastLoginAt), COUNT(*) FROM [dbo].[EldisUsers]
            ORDER BY TableName";

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new DebugTableInfo
            {
                TableName = reader.GetString(0),
                Date = reader.IsDBNull(1) ? null : reader.GetDateTime(1),
                RowCount = reader.GetInt32(2)
            });
        }

        return result;
    }
}