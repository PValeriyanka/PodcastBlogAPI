using PodcastBlog.Domain.Interfaces.Repositories;

namespace PodcastBlog.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        ICommentRepository Comments { get; }
        INotificationRepository Notifications { get; }
        IPodcastRepository Podcasts { get; }
        IPostRepository Posts { get; }
        ITagRepository Tags { get; }
        IUserRepository Users { get; }

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
