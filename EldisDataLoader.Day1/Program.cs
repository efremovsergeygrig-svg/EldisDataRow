using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

using Microsoft.Extensions.Hosting;
using EldisDataLoader;
using EldisDataLoader.Services;

var builder = Host.CreateApplicationBuilder(args);
// Явно добавляем наш переименованный файл настроек
builder.Configuration.AddJsonFile("appsettings.service.json", optional: true, reloadOnChange: true);


// Настраиваем логирование: убираем стандартных провайдеров и добавляем наш файловый
builder.Logging.ClearProviders();
builder.Logging.AddProvider(new FileLoggerProvider());

// HTTP-клиент (нужен для работы всех сервисов через IHttpClientFactory)
builder.Services.AddHttpClient();

// Сервис авторизации
builder.Services.AddSingleton<AuthService>();

// Сервисы загрузки данных
builder.Services.AddSingleton<ObjectsService>();
builder.Services.AddSingleton<DevicesService>();
builder.Services.AddSingleton<MeteringPointsService>();
builder.Services.AddSingleton<RawDataService>();

// Фоновый работник (менеджер)
builder.Services.AddHostedService<Worker>();

// Электронная почта (используем Singleton, так как Worker тоже Singleton)
builder.Services.AddSingleton<NotificationService>();

var host = builder.Build();

// ==========================================
// ИНФОРМАЦИЯ О ВЕРСИИ И ЗАПУСКЕ
// ==========================================
var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
var buildDate = System.IO.File.GetLastWriteTime(exePath).ToString("dd.MM.yyyy HH:mm");
var startTime = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");

FileLogger.Write("===========================================");
FileLogger.Write($"[ЗАПУСК] Дата сборки файла: {buildDate}");
FileLogger.Write($"[ЗАПУСК] Время запуска:     {startTime}");
FileLogger.Write("===========================================");
// ==========================================

host.Run();