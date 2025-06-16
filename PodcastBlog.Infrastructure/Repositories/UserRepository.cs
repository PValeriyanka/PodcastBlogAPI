using Microsoft.EntityFrameworkCore;
using PodcastBlog.Domain.IRepositories;
using PodcastBlog.Domain.Models;

namespace PodcastBlog.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly PodcastBlogContext _context;

        public UserRepository(PodcastBlogContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserById(int id, CancellationToken cancellationToken)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task CreateUser(User user, CancellationToken cancellationToken)
        {
            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateUser(User user, CancellationToken cancellationToken)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteUser(int id, CancellationToken cancellationToken)
        {
            var user = await GetUserById(id, cancellationToken);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
