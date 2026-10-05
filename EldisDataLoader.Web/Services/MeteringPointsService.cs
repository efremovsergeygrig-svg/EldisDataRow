using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EldisDataLoader.Web.Services;

public class MeteringPointsService
{
    private readonly IConfiguration _configuration;

    public MeteringPointsService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<List<MeteringPointViewDto>> GetAllMeteringPointsAsync()
    {
        var result = new List<MeteringPointViewDto>();
        var connectionString = _configuration["Database:ConnectionString"];

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT 
                [Id],
                [LINK],
                [SerialNumber],
                [DeviceName],
                [Address],
                [ResourceName],
                [MeasurePointName],
                [CreatedOn]
            FROM [dbo].[EldisMeteringPoints]
            ORDER BY [DeviceName], [SerialNumber]";

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new MeteringPointViewDto
            {
                Id = reader.GetGuid(0),
                Link = reader.GetInt32(1),
                SerialNumber = reader.IsDBNull(2) ? null : reader.GetString(2),
                DeviceName = reader.IsDBNull(3) ? null : reader.GetString(3),
                Address = reader.IsDBNull(4) ? null : reader.GetString(4),
                ResourceName = reader.IsDBNull(5) ? null : reader.GetString(5),
                MeasurePointName = reader.IsDBNull(6) ? null : reader.GetString(6),
                CreatedOn = reader.IsDBNull(7) ? null : reader.GetDateTime(7)
            });
        }

        return result;
    }
}