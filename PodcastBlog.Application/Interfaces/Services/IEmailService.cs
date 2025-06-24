namespace PodcastBlog.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendNotificationEmailAsync(string toEmail, string subject, string body);
    }
}
