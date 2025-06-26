using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PodcastBlog.Application.Interfaces.Services;
using System.Net;
using System.Net.Mail;

namespace PodcastBlog.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpClient _smtpClient;
        private readonly string _fromAddress;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _logger = logger;
            _smtpClient = new SmtpClient
            {
                Host = config["Email:Host"],
                Port = int.Parse(config["Email:Port"] ?? "587"),
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    config["Email:Username"],
                    config["Email:Password"])
            };

            _fromAddress = config["Email:From"]!;
        }

        public async Task SendNotificationEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var mail = new MailMessage
                {
                    From = new MailAddress(_fromAddress),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };

                mail.To.Add(toEmail);

                await _smtpClient.SendMailAsync(mail);

                _logger.LogInformation("Письмо успешно отправлено на email");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке письма на email");
                throw;
            }
        }
    }

}
