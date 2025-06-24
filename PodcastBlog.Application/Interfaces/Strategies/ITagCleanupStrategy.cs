using PodcastBlog.Domain.Models;

namespace PodcastBlog.Application.Interfaces.Strategies
{
    public interface ITagCleanupStrategy
    {
        Task CleanupAsync(Tag tag, CancellationToken cancellationToken);
    }
}