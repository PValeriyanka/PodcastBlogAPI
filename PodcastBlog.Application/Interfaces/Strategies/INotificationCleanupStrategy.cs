using PodcastBlog.Domain.Models;

namespace PodcastBlog.Application.Interfaces.Strategies
{
    public interface INotificationCleanupStrategy
    {
        Task CleanupAsync(Notification notification, CancellationToken cancellationToken);
    }
}
