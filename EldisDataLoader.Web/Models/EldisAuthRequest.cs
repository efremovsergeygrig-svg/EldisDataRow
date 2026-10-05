namespace EldisDataLoader.Models;

public class EldisAuthRequest
{
    public string KeyApi { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class EldisAuthResponse
{
    public string Token { get; set; } = string.Empty;
}