namespace EldisDataLoader;

/// <summary>
/// Простой логгер, который пишет сообщения и в консоль, и в файл.
/// Имя файла формируется с датой: Log_YYYYMMDD.txt
/// </summary>
public static class FileLogger
{
    private static readonly string LogDirectory;
    private static readonly object LockObject = new object();

    static FileLogger()
    {
        // Папка для логов — рядом с exe-файлом
        LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        // Создаём папку Logs, если её нет
        if (!Directory.Exists(LogDirectory))
        {
            Directory.CreateDirectory(LogDirectory);
        }
    }

    /// <summary>
    /// Записывает сообщение в консоль и в файл
    /// </summary>
    public static void Write(string message)
    {
        // Вывод в консоль
        Console.WriteLine(message);

        // Запись в файл
        WriteToFile(message);
    }

    /// <summary>
    /// Записывает сообщение об ошибке (красным в консоль и в файл)
    /// </summary>
    public static void WriteError(string message)
    {
        var oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ForegroundColor = oldColor;

        WriteToFile($"[ОШИБКА] {message}");
    }

    /// <summary>
    /// Записывает сообщение в файл с датой
    /// </summary>
    private static void WriteToFile(string message)
    {
        lock (LockObject)
        {
            try
            {
                // Формат имени файла: Log_20260909.txt
                var fileName = $"Log_{DateTime.Now:yyyyMMdd}.txt";
                var filePath = Path.Combine(LogDirectory, fileName);

                // Формат строки лога: [09.09.2026 14:30:15] Сообщение
                var logEntry = $"[{DateTime.Now:dd.MM.yyyy HH:mm:ss}] {message}";

                File.AppendAllText(filePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Если не удалось записать в файл, хотя бы выведем в консоль
                Console.WriteLine($"[FileLogger] Ошибка записи в файл: {ex.Message}");
            }
        }
    }
}