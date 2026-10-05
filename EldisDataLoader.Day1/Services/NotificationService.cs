using System.Net;
using System.Net.Mail;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EldisDataLoader.Services;

public class NotificationService
{
    private readonly IConfiguration _configuration;
    private readonly List<NotificationRecipient> _recipients;

    public NotificationService(IConfiguration configuration)
    {
        _configuration = configuration;
        _recipients = GetRecipients();

        // Игнорируем ошибки имени в SSL-сертификате для внутреннего Exchange
        ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
    }

    private List<NotificationRecipient> GetRecipients()
    {
        var connectionString = _configuration["Database:ConnectionString"];
        var recipients = new List<NotificationRecipient>();

        try
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            const string sql = @"
                SELECT [UserEmail], [SendEmailOnSuccess], [SendEmailOnError]
                FROM [dbo].[EldisUsers]
                WHERE [IsActive] = 1 
                  AND [UserEmail] IS NOT NULL 
                  AND ([SendEmailOnSuccess] = 1 OR [SendEmailOnError] = 1)";

            using var command = new SqlCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var email = reader.GetString(0);
                var sendOnSuccess = reader.GetBoolean(1);
                var sendOnError = reader.GetBoolean(2);

                if (!string.IsNullOrWhiteSpace(email) && email.Contains("@"))
                {
                    recipients.Add(new NotificationRecipient
                    {
                        Email = email,
                        SendOnSuccess = sendOnSuccess,
                        SendOnError = sendOnError
                    });
                }
            }
        }
        catch (Exception ex)
        {
            FileLogger.WriteError($"Ошибка при получении списка получателей: {ex.Message}");
        }

        return recipients;
    }

    public async Task SendStartNotificationAsync()
    {
        var recipients = _recipients.Where(r => r.SendOnSuccess).ToList();
        if (!recipients.Any()) return;

        var subject = $"🚀 Eldis: Загрузка запущена ({DateTime.Now:dd.MM.yyyy})";
        var body = $@"<h2>Загрузка показаний запущена</h2><p><b>Дата и время старта:</b> {DateTime.Now:dd.MM.yyyy HH:mm:ss}</p><p>Ожидайте отчет о завершении.</p>";
        await SendEmailsAsync(recipients, subject, body);
    }

    public async Task SendSuccessNotificationAsync(TimeSpan duration, string summary = "")
    {
        var recipients = _recipients.Where(r => r.SendOnSuccess).ToList();
        if (!recipients.Any()) return;

        var subject = $"✅ Eldis: Загрузка успешно завершена ({DateTime.Now:dd.MM.yyyy})";
        var body = $@"<h2>Загрузка показаний завершена успешно</h2><p><b>Дата и время окончания:</b> {DateTime.Now:dd.MM.yyyy HH:mm:ss}</p><p><b>Время выполнения:</b> {duration.Hours}ч {duration.Minutes}м {duration.Seconds}с</p>{(string.IsNullOrEmpty(summary) ? "" : $"<p><b>Детали:</b> {summary}</p>")}";
        await SendEmailsAsync(recipients, subject, body);
    }

    public async Task SendErrorNotificationAsync(string errorMessage)
    {
        var recipients = _recipients.Where(r => r.SendOnError).ToList();
        if (!recipients.Any()) return;

        var subject = $"❌ Eldis: Ошибка при загрузке ({DateTime.Now:dd.MM.yyyy})";
        var body = $@"<h2>Произошла ошибка при загрузке показаний</h2><p><b>Дата:</b> {DateTime.Now:dd.MM.yyyy HH:mm}</p><p><b>Ошибка:</b><br/><code>{errorMessage}</code></p>";
        await SendEmailsAsync(recipients, subject, body);
    }

    private async Task SendEmailsAsync(List<NotificationRecipient> recipients, string subject, string body)
    {
        var smtpServer = _configuration["Email:SmtpServer"] ?? "smtp.yourcompany.local";
        var port = int.TryParse(_configuration["Email:SmtpPort"], out var p) ? p : 25;
        var fromEmail = _configuration["Email:FromEmail"] ?? "eldis-service@yourcompany.local";

        var smtpUser = _configuration["Email:SmtpUser"];
        var smtpPassword = _configuration["Email:SmtpPassword"];
        var useSsl = bool.TryParse(_configuration["Email:UseSsl"], out var ssl) && ssl;

        // 🔍 ОТЛАДКА: Запишем в лог, видит ли программа логин
        var userDisplay = string.IsNullOrEmpty(smtpUser) ? "(ПУСТО)" : smtpUser;
        FileLogger.Write($"[DEBUG] SMTP Настройки: Сервер={smtpServer}, Порт={port}, SSL={useSsl}, Пользователь={userDisplay}");

        using var client = new SmtpClient(smtpServer, port);
        client.EnableSsl = useSsl;

        // Если логин и пароль указаны, используем их
        if (!string.IsNullOrEmpty(smtpUser) && !string.IsNullOrEmpty(smtpPassword))
        {
            client.Credentials = new NetworkCredential(smtpUser, smtpPassword);
            FileLogger.Write("[DEBUG] Используются явные учетные данные (Credentials).");
        }
        else
        {
            // Если логина нет, пробуем использовать учетные данные Windows (актуально, если задача запускается от доменного пользователя)
            client.UseDefaultCredentials = true;
            FileLogger.Write("[DEBUG] Явные учетные данные не найдены. Используется UseDefaultCredentials = true.");
        }

        foreach (var recipient in recipients)
        {
            try
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(recipient.Email);

                await client.SendMailAsync(mailMessage);
                FileLogger.Write($"✅ Письмо успешно отправлено: {recipient.Email}");
            }
            catch (Exception ex)
            {
                FileLogger.WriteError($"❌ Ошибка отправки письма {recipient.Email}. Полная информация: {ex.ToString()}");
            }
        }
    }

    private class NotificationRecipient
    {
        public string Email { get; set; } = string.Empty;
        public bool SendOnSuccess { get; set; }
        public bool SendOnError { get; set; }
    }
}