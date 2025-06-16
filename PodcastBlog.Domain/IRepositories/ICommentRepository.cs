using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;

namespace PodcastBlog.Domain.IRepositories
{
    public interface ICommentRepository
    {
        Task<PagedList<Comment>> GetCommentsByPostPaged(int postId, Parameters.Parameters parameters, CancellationToken cancellationToken);
        Task<Comment> GetCommentById(int id, CancellationToken cancellationToken);
        Task CreateComment(Comment comment, CancellationToken cancellationToken);
        Task UpdateComment(Comment comment, CancellationToken cancellationToken);
        Task DeleteComment(int id, CancellationToken cancellationToken);
    }
}
