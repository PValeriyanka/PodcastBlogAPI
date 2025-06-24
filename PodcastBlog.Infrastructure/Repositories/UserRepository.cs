using Microsoft.EntityFrameworkCore;
using PodcastBlog.Domain.Interfaces.Repositories;
using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;

namespace PodcastBlog.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(PodcastBlogContext context) : base(context) { }

        public async Task<PagedList<User>> GetAllUsersPagedAsync(Parameters parameters, CancellationToken cancellationToken)
        {
            var usersQuery = _context.Users.OrderBy(u => u.Name).AsQueryable();

            var count = await usersQuery.CountAsync(cancellationToken);

            var users = await usersQuery
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedList<User>(users, count, parameters.PageNumber, parameters.PageSize);
        }

        public override async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Include(u => u.Posts)
                    .ThenInclude(p => p.Tags)
                .Include(u => u.Comments)
                .Include(u => u.Subscriptions)
                .Include(u => u.Followers)
                    .ThenInclude(f => f.Subscriber)
                .Include(u => u.Liked).FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }
    }
}
