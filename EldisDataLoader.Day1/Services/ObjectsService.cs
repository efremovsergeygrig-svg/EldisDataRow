using EldisDataLoader.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text.Json;

namespace EldisDataLoader.Services;

/// <summary>
/// Загрузка и сохранение списка объектов
/// </summary>
public class ObjectsService : EldisServiceBase
{
    public override string ServiceName => "Объекты";

    public ObjectsService(
        ILogger<ObjectsService> logger,
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
            // Параметры передаются через URL (query string)
            var endpoint = $"/v1/objects/list?limit={pageSize}&page={currentPage}";

            var apiResponse = await PostAndDeserializeAsync<EldisObjectResponse>(
                client, endpoint, null, cancellationToken);

            if (apiResponse?.Response?.Objects?.List == null || apiResponse.Response.Objects.List.Count == 0)
                break;

            var objects = apiResponse.Response.Objects.List;

            Logger.LogInformation("[{Service}] Страница {Page}: получено {Count} объектов",
                ServiceName, currentPage, objects.Count);

            await SaveObjectsAsync(objects, cancellationToken);
            totalSaved += objects.Count;

            // Если записей меньше, чем pageSize, значит это была последняя страница
            if (objects.Count < pageSize)
            {
                break;
            }

            currentPage++;
        }

        Logger.LogInformation("[{Service}] Успешно сохранено {Count} объектов", ServiceName, totalSaved);
    }

    private async Task SaveObjectsAsync(List<ObjectDto> objects, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);

        foreach (var obj in objects)
        {
            string tagsJson = null;
            if (obj.Tags.HasValue &&
                obj.Tags.Value.ValueKind != JsonValueKind.Null &&
                obj.Tags.Value.ValueKind != JsonValueKind.Undefined)
            {
                tagsJson = obj.Tags.Value.GetRawText();
            }

            const string sql = @"
                MERGE INTO [dbo].[EldisObjects] AS target
                USING (SELECT @Id AS Id) AS source
                ON target.Id = source.Id
                WHEN MATCHED THEN 
                    UPDATE SET 
                        IsWinterMode = @IsWinterMode, IsModeChanged = @IsModeChanged, Address = @Address,
                        TypeObjectId = @TypeObjectId, TypeObjectName = @TypeObjectName, Name = @Name,
                        Latitude = @Latitude, Longitude = @Longitude, Description = @Description,
                        Consumer = @Consumer, DashboardId = @DashboardId, DashboardName = @DashboardName,
                        CreatedOn = @CreatedOn, ObjectIdentifier = @ObjectIdentifier, HasEvents = @HasEvents,
                        ActiveEvents = @ActiveEvents, ManagementOrganizationId = @ManagementOrganizationId,
                        ManagementOrganizationName = @ManagementOrganizationName, Tags = @Tags,
                        ImportedAt = SYSUTCDATETIME()
                WHEN NOT MATCHED THEN 
                    INSERT (Id, IsWinterMode, IsModeChanged, Address, TypeObjectId, TypeObjectName, Name,
                            Latitude, Longitude, Description, Consumer, DashboardId, DashboardName,
                            CreatedOn, ObjectIdentifier, HasEvents, ActiveEvents, ManagementOrganizationId,
                            ManagementOrganizationName, Tags, ImportedAt)
                    VALUES (@Id, @IsWinterMode, @IsModeChanged, @Address, @TypeObjectId, @TypeObjectName, @Name,
                            @Latitude, @Longitude, @Description, @Consumer, @DashboardId, @DashboardName,
                            @CreatedOn, @ObjectIdentifier, @HasEvents, @ActiveEvents, @ManagementOrganizationId,
                            @ManagementOrganizationName, @Tags, SYSUTCDATETIME());";

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", obj.Id);
            command.Parameters.AddWithValue("@IsWinterMode", obj.IsWinterMode);
            command.Parameters.AddWithValue("@IsModeChanged", obj.IsModeChanged);
            command.Parameters.AddWithValue("@Address", obj.Address ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@TypeObjectId", obj.TypeObject?.Id ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@TypeObjectName", obj.TypeObject?.Name ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Name", obj.Name ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Latitude", obj.Latitude.HasValue ? (object)obj.Latitude.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Longitude", obj.Longitude.HasValue ? (object)obj.Longitude.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Description", obj.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Consumer", obj.Consumer ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DashboardId", obj.DashboardId.HasValue ? (object)obj.DashboardId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@DashboardName", obj.DashboardName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CreatedOn", obj.CreatedOn.HasValue ? (object)obj.CreatedOn.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ObjectIdentifier", obj.ObjectIdentifier ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@HasEvents", obj.HasEvents.HasValue ? (object)obj.HasEvents.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ActiveEvents", obj.ActiveEvents.HasValue ? (object)obj.ActiveEvents.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ManagementOrganizationId", obj.ManagementOrganizationId.HasValue ? (object)obj.ManagementOrganizationId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ManagementOrganizationName", obj.ManagementOrganizationName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Tags", tagsJson ?? (object)DBNull.Value);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}