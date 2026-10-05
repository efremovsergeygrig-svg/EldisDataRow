using System.Net.Http;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EldisDataLoader.Services;

/// <summary>
/// Базовый класс для всех сервисов Eldis.
/// Содержит общую логику работы с HTTP и БД.
/// </summary>
public abstract class EldisServiceBase : IEldisService
{
    protected readonly ILogger Logger;
    protected readonly IHttpClientFactory HttpClientFactory;
    protected readonly IConfiguration Configuration;
    protected readonly string BaseUrl;
    protected readonly string KeyApi;
    protected readonly string ConnectionString;

    public abstract string ServiceName { get; }

    protected EldisServiceBase(
        ILogger logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        Logger = logger;
        HttpClientFactory = httpClientFactory;
        Configuration = configuration;

        BaseUrl = configuration["EldisApi:BaseUrl"];
        KeyApi = configuration["EldisApi:KeyApi"];
        ConnectionString = configuration["Database:ConnectionString"];
    }

    /// <summary>
    /// Создаёт настроенный HTTP-клиент с заголовками авторизации
    /// </summary>
    protected HttpClient CreateAuthorizedClient(string token)
    {
        var client = HttpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("key", KeyApi);
        client.DefaultRequestHeaders.Add("Cookie", $"access_token={token}");
        return client;
    }

    /// <summary>
    /// Выполняет POST-запрос и возвращает десериализованный ответ
    /// </summary>
    protected async Task<TResponse?> PostAndDeserializeAsync<TResponse>(
        HttpClient client,
        string endpoint,
        Dictionary<string, string>? formData = null,
        CancellationToken cancellationToken = default)
    {
        var content = formData != null
            ? new FormUrlEncodedContent(formData)
            : new FormUrlEncodedContent(new List<KeyValuePair<string, string>>());

        var response = await client.PostAsync($"{BaseUrl}{endpoint}", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            return JsonSerializer.Deserialize<TResponse>(json);
        }
        catch (JsonException ex)
        {
            // ЛОВУШКА: Логируем сырой JSON при ошибке десериализации
            var preview = json.Length > 2000 ? json.Substring(0, 2000) + "... [ОБРЕЗАНО]" : json;
            Logger.LogError(ex, "[ОШИБКА JSON] Не удалось десериализовать ответ для {Endpoint}. Сырой ответ (первые 2000 символов):\n{JsonPreview}", endpoint, preview);
            throw; // Пробрасываем ошибку дальше, чтобы процесс остановился
        }
    }

    /// <summary>
    /// Выполняет GET-запрос и возвращает десериализованный ответ
    /// </summary>
    protected async Task<TResponse?> GetAndDeserializeAsync<TResponse>(
        HttpClient client,
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync($"{BaseUrl}{endpoint}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<TResponse>(json);
    }

    /// <summary>
    /// Абстрактный метод — каждая реализация описывает свою логику загрузки
    /// </summary>
    public abstract Task ExecuteAsync(string token, CancellationToken cancellationToken);
}