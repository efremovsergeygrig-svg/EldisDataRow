namespace EldisDataLoader.Services;

/// <summary>
/// Общий интерфейс для всех сервисов загрузки данных
/// </summary>
public interface IEldisService
{
    /// <summary>
    /// Название сервиса (для логирования)
    /// </summary>
    string ServiceName { get; }

    /// <summary>
    /// Выполнить загрузку данных
    /// </summary>
    Task ExecuteAsync(string token, CancellationToken cancellationToken);
}