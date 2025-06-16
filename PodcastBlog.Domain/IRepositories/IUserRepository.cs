using PodcastBlog.Domain.Models;

namespace PodcastBlog.Domain.IRepositories
{
    public interface IUserRepository
    {
        Task<User> GetUserById(int id, CancellationToken cancellationToken);
        Task CreateUser(User user, CancellationToken cancellationToken);
        Task UpdateUser(User user, CancellationToken cancellationToken);
        Task DeleteUser(int id, CancellationToken cancellationToken);
    }
}
