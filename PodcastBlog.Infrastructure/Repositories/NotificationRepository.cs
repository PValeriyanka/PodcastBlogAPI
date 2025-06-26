using Microsoft.EntityFrameworkCore;
using PodcastBlog.Domain.Interfaces.Repositories;
using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;

namespace PodcastBlog.Infrastructure.Repositories
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(PodcastBlogContext context) : base(context) { }

        public async Task<PagedList<Notification>> GetNotificationsByUserPagedAsync(int userId, Parameters parameters, CancellationToken cancellationToken)
        {
            var notificationsQuery = _context.Notifications
                .Where(n => n.UserId == userId)
                .Include(n => n.User)
                .AsNoTracking()
                .AsQueryable();

            var count = await notificationsQuery.CountAsync(cancellationToken);

            var notifications = await notificationsQuery
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedList<Notification>(notifications, count, parameters.PageNumber, parameters.PageSize);
        }

        public override async Task<Notification?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _context.Notifications
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.NotificationId == id, cancellationToken);
        }
    }
}
