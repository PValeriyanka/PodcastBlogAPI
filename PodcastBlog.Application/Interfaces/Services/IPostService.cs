using PodcastBlog.Application.ModelsDto.Post;
using PodcastBlog.Domain.Parameters;
using PodcastBlog.Domain.Parameters.ModelParameters;
using System.Security.Claims;

namespace PodcastBlog.Application.Interfaces.Services
{
    public interface IPostService
    {
        Task<PagedList<PostDto>> GetPostsPagedAsync(PostParameters parameters, ClaimsPrincipal userPrincipal, string? type, CancellationToken cancellationToken);
        Task<PostDto> GetPostByIdAsync(int id, CancellationToken cancellationToken);
        Task CreatePostAsync(CreatePostDto createPostDto, ClaimsPrincipal claimsPrincipal, string status, CancellationToken cancellationToken);
        Task UpdatePostAsync(UpdatePostDto updatePostDto, ClaimsPrincipal userPrincipal, string? status, CancellationToken cancellationToken);
        Task DeletePostAsync(int id, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken);
    }
}