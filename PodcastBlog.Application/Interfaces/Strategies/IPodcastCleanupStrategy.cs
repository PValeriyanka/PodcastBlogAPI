using PodcastBlog.Domain.Models;

namespace PodcastBlog.Application.Interfaces.Strategies
{
    public interface IPodcastCleanupStrategy
    {
        Task CleanupAsync(Podcast podcast, CancellationToken cancellationToken);
    }
}