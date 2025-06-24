using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;

namespace PodcastBlog.Domain.Interfaces.Repositories
{
    public interface ICommentRepository : IRepository<Comment>
    {
        Task<PagedList<Comment>> GetCommentsByPostPagedAsync(int postId, Parameters.Parameters parameters, CancellationToken cancellationToken);
    }
}
