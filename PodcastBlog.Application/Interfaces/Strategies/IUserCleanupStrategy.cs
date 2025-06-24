using PodcastBlog.Domain.Models;

namespace PodcastBlog.Application.Interfaces.Strategies
{
    public interface IUserCleanupStrategy
    {
        Task CleanupAsync(User user, CancellationToken cancellationToken);
    }
}