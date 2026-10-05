using Microsoft.Extensions.Logging;
using System.Diagnostics;
using EldisDataLoader.Services;
using Microsoft.Extensions.Hosting; // <-- Добавлено для IHostApplicationLifetime

namespace EldisDataLoader;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly AuthService _authService;
    private readonly ObjectsService _objectsService;
    private readonly DevicesService _devicesService;
    private readonly MeteringPointsService _meteringPointsService;
    private readonly RawDataService _rawDataService;
    private readonly NotificationService _notificationService;

    // Добавляем интерфейс для управления временем жизни приложения
    private readonly IHostApplicationLifetime _appLifetime;

    public Worker(
        ILogger<Worker> logger,
        AuthService authService,
        ObjectsService objectsService,
        DevicesService devicesService,
        MeteringPointsService meteringPointsService,
        RawDataService rawDataService,
        NotificationService notificationService,
        IHostApplicationLifetime appLifetime) // <-- Добавляем в конструктор
    {
        _logger = logger;
        _authService = authService;
        _objectsService = objectsService;
        _devicesService = devicesService;
        _meteringPointsService = meteringPointsService;
        _rawDataService = rawDataService;
        _notificationService = notificationService;
        _appLifetime = appLifetime; // <-- Сохраняем
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Поскольку нам нужно выполнить работу один раз и выйти, 
        // мы можем даже не использовать while, но оставим его для совместимости с BackgroundService
        if (stoppingToken.IsCancellationRequested) return;

        var stopwatch = Stopwatch.StartNew();

        try
        {
            FileLogger.Write("=== НАЧАЛО РАБОТЫ ===");

            // --- ПОЧТА ВРЕМЕННО ОТКЛЮЧЕНА ---
            // await _notificationService.SendStartNotificationAsync();
            // --------------------------------

            var token = await _authService.GetTokenAsync(stoppingToken);
            FileLogger.Write("Токен успешно получен.");

            await _objectsService.ExecuteAsync(token, stoppingToken);
            await _devicesService.ExecuteAsync(token, stoppingToken);
            await _meteringPointsService.ExecuteAsync(token, stoppingToken);
            await _rawDataService.ExecuteAsync(token, stoppingToken);

            stopwatch.Stop();
            FileLogger.Write("=== ВСЕ СЕРВИСЫ ЗАВЕРШЕНЫ ===");

            // --- ПОЧТА ВРЕМЕННО ОТКЛЮЧЕНА ---
            // await _notificationService.SendSuccessNotificationAsync(stopwatch.Elapsed, "Все этапы выполнены");
            // --------------------------------
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            FileLogger.WriteError($"Критическая ошибка: {ex.Message}");
            _logger.LogError(ex, "Ошибка при работе с API");

            // --- ПОЧТА ВРЕМЕННО ОТКЛЮЧЕНА ---
            // await _notificationService.SendErrorNotificationAsync(ex.Message);
            // --------------------------------
        }
        finally
        {
            // САМОЕ ГЛАВНОЕ: Говорим приложению, что работа закончена и можно закрываться
            FileLogger.Write("=== ИНИЦИАЛИЗАЦИЯ ЗАВЕРШЕНИЯ РАБОТЫ ПРИЛОЖЕНИЯ ===");
            _appLifetime.StopApplication();
        }
    }
}