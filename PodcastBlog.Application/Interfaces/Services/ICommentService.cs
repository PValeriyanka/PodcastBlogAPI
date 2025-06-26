using PodcastBlog.Application.ModelsDto.Comment;
using PodcastBlog.Domain.Parameters;
using System.Security.Claims;

namespace PodcastBlog.Application.Interfaces.Services
{
    public interface ICommentService
    {
        Task<PagedList<CommentDto>> GetCommentsByPostPagedAsync(int postId, Parameters parameters, CancellationToken cancellationToken);
        Task<CommentDto> GetCommentByIdAsync(int id, CancellationToken cancellationToken);
        Task CreateCommentAsync(CreateCommentDto createCommentDto, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken);
        Task PublishCommentAsync(int id, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken);
        Task DeleteCommentAsync(int id, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken);
    }
}