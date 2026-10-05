using Microsoft.Extensions.Logging;
using System;

namespace EldisDataLoader.Services;

/// <summary>
/// Кастомный провайдер логирования, который перенаправляет все сообщения в глобальный FileLogger
/// </summary>
public class FileLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new FileLoggerAdapter(categoryName);
    }

    public void Dispose() { }

    // Переименовали класс в FileLoggerAdapter, чтобы не конфликтовать с глобальным FileLogger
    private class FileLoggerAdapter : ILogger
    {
        private readonly string _categoryName;

        public FileLoggerAdapter(string categoryName)
        {
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var message = formatter(state, exception);

            // Форматируем сообщение с уровнем логирования
            var formattedMessage = $"[{logLevel.ToString().ToUpper()}] {message}";

            if (exception != null)
            {
                formattedMessage += $"\nИсключение: {exception.Message}";
            }

            // Обращаемся к глобальному статическому классу FileLogger
            FileLogger.Write(formattedMessage);
        }
    }
}