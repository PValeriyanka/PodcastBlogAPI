using PodcastBlog.Application.ModelsDto;
using PodcastBlog.Domain.Parameters;

namespace PodcastBlog.Application.Interfaces.Services
{
    public interface ICommentService
    {
        Task<PagedList<CommentDto>> GetCommentsByPostPagedAsync(int postId, Parameters parameters, CancellationToken cancellationToken);
        Task<CommentDto> GetCommentByIdAsync(int id, CancellationToken cancellationToken);
        Task CreateCommentAsync(CommentDto commentDto, int? parentId, CancellationToken cancellationToken);
        Task PublishCommentAsync(int id, CancellationToken cancellationToken);
        Task DeleteCommentAsync(int id, CancellationToken cancellationToken);
    }
}