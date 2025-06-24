using PodcastBlog.Domain.Models;

namespace PodcastBlog.Application.Interfaces.Strategies
{
    public interface IPostCleanupStrategy
    {
        Task CleanupAsync(Post post, CancellationToken cancellationToken);
    }
}