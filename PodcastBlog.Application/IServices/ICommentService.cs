using PodcastBlog.Application.ModelsDTO;
using PodcastBlog.Domain.Parameters;

namespace PodcastBlog.Application.IServices
{
    public interface ICommentService
    {
        Task<PagedList<CommentDTO>> GetCommentsByPostPaged(int postId, Parameters parameters, CancellationToken cancellationToken);
        Task<CommentDTO> GetCommentById(int id, CancellationToken cancellationToken);
        Task CreateComment(CommentDTO commentDTO, CancellationToken cancellationToken);
        Task UpdateComment(CommentDTO commentDTO, CancellationToken cancellationToken);
        Task DeleteComment(int id, CancellationToken cancellationToken);
    }
}
