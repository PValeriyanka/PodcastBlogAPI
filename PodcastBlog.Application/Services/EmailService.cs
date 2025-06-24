using Microsoft.Extensions.Configuration;
using PodcastBlog.Application.Interfaces.Services;
using System.Net;
using System.Net.Mail;

namespace PodcastBlog.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpClient _smtpClient;
        private readonly string _fromAddress;

        public EmailService(IConfiguration config)
        {
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
            var mail = new MailMessage
            {
                From = new MailAddress(_fromAddress),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            mail.To.Add(toEmail);

            await _smtpClient.SendMailAsync(mail);
        }
    }

}
