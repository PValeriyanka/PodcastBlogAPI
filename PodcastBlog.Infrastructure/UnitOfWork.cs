using PodcastBlog.Domain.Interfaces;
using PodcastBlog.Domain.Interfaces.Repositories;

namespace PodcastBlog.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PodcastBlogContext _context;

        public ICommentRepository Comments { get; }
        public INotificationRepository Notifications { get; }
        public IPodcastRepository Podcasts { get; }
        public IPostRepository Posts { get; }
        public ITagRepository Tags { get; }
        public IUserRepository Users { get; }

        public UnitOfWork(
            PodcastBlogContext context,
            ICommentRepository comments,
            INotificationRepository notifications,
            IPodcastRepository podcasts,
            IPostRepository posts,
            ITagRepository tags,
            IUserRepository users)
        {
            _context = context;
            Comments = comments;
            Notifications = notifications;
            Podcasts = podcasts;
            Posts = posts;
            Tags = tags;
            Users = users;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }

}
