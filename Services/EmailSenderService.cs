// Services/EmailSenderService.cs
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration; // Dodano dla IConfiguration
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Dodano dla ILogger
using System;

namespace CarWorkshopManagementSystem.Services
{
    public class EmailSenderService : IEmailSenderService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSenderService> _logger;

        public EmailSenderService(IConfiguration configuration, ILogger<EmailSenderService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message, byte[]? attachmentBytes = null, string? attachmentFileName = null)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var host = emailSettings["SmtpHost"];
                var port = int.Parse(emailSettings["SmtpPort"] ?? "587"); // Użyj wartości domyślnej 587
                var username = emailSettings["SmtpUsername"];
                var password = emailSettings["SmtpPassword"];
                var fromAddress = emailSettings["FromAddress"];
                var fromName = emailSettings["FromName"];

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fromAddress))
                {
                    _logger.LogError("Brak pełnej konfiguracji ustawień e-mail w appsettings.json.");
                    throw new InvalidOperationException("Brak pełnej konfiguracji ustawień e-mail.");
                }

                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(fromName, fromAddress));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = message };

                if (attachmentBytes != null && !string.IsNullOrEmpty(attachmentFileName))
                {
                    bodyBuilder.Attachments.Add(attachmentFileName, attachmentBytes, ContentType.Parse("application/pdf"));
                }

                email.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(username, password);
                    await client.SendAsync(email);
                    await client.DisconnectAsync(true);
                }
                _logger.LogInformation("E-mail wysłany pomyślnie do {ToEmail} z tematem: {Subject}.", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas wysyłania e-maila do {ToEmail} z tematem: {Subject}.", toEmail, subject);
                throw; // Przekaż wyjątek dalej
            }
        }
    }
}