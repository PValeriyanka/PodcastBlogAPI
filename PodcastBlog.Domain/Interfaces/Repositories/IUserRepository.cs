using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;
namespace PodcastBlog.Domain.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<PagedList<User>> GetAllUsersPagedAsync(Parameters.Parameters parameters, CancellationToken cancellationToken);
    }
}
