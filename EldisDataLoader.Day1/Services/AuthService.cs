using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EldisDataLoader.Services;

/// <summary>
/// Сервис получения токена авторизации
/// </summary>
public class AuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AuthService(
        ILogger<AuthService> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        var baseUrl = _configuration["EldisApi:BaseUrl"];
        var keyApi = _configuration["EldisApi:KeyApi"];
        var login = _configuration["EldisApi:Login"];
        var password = _configuration["EldisApi:Password"];

        client.DefaultRequestHeaders.Add("key", keyApi);

        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("login", login),
            new KeyValuePair<string, string>("password", password)
        });

        var response = await client.PostAsync($"{baseUrl}/v1/users/login", formData, cancellationToken);
        response.EnsureSuccessStatusCode();

        string token = null;
        if (response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
        {
            foreach (var cookie in cookieValues)
            {
                if (cookie.Contains("access_token="))
                {
                    token = cookie.Split("access_token=").Last().Split(';').First().Trim();
                    break;
                }
            }
        }

        if (string.IsNullOrEmpty(token))
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Токен не найден. Ответ сервера: {body}");
        }

        _logger.LogInformation("Токен успешно получен");
        return token;
    }
}