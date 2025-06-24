using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;

namespace PodcastBlog.Domain.Interfaces.Repositories
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<PagedList<Notification>> GetNotificationsByUserPagedAsync(int userId, Parameters.Parameters parameters, CancellationToken cancellationToken);
    }
}
