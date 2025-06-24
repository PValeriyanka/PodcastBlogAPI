using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Domain.Models;

namespace PodcastBlog.Application.Strategies
{
    public class NotificationCleanupStrategy : INotificationCleanupStrategy
    {
        public Task CleanupAsync(Notification notification, CancellationToken cancellationToken)
        {
            notification.User.Notifications.Remove(notification);

            return Task.CompletedTask;
        }
    }
}
