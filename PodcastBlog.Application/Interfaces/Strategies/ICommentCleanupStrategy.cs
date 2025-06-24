using PodcastBlog.Domain.Models;

namespace PodcastBlog.Application.Interfaces.Strategies
{
    public interface ICommentCleanupStrategy
    {
        Task CleanupAsync(Comment comment, CancellationToken cancellationToken);
    }
}