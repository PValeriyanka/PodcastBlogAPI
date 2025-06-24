using PodcastBlog.Application.ModelsDto;
using PodcastBlog.Domain.Parameters;
using PodcastBlog.Domain.Parameters.ModelParameters;
using System.Security.Claims;

namespace PodcastBlog.Application.Interfaces.Services
{
    public interface IPostService
    {
        Task<PagedList<PostDto>> GetPostsPagedAsync(PostParameters parameters, ClaimsPrincipal userPrincipal, string? type, CancellationToken cancellationToken);
        Task<PostDto> GetPostByIdAsync(int id, CancellationToken cancellationToken);
        Task CreatePostAsync(PostDto postDto, ClaimsPrincipal claimsPrincipal, string status, CancellationToken cancellationToken);
        Task UpdatePostAsync(PostDto postDto, ClaimsPrincipal userPrincipal, string? status, CancellationToken cancellationToken);
        Task DeletePostAsync(int id, CancellationToken cancellationToken);
    }
}