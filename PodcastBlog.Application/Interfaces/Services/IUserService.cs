using PodcastBlog.Application.ModelsDto;
using PodcastBlog.Domain.Parameters;
using System.Security.Claims;

namespace PodcastBlog.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<PagedList<UserDto>> GetAllUsersPagedAsync(Parameters parameters, CancellationToken cancellationToken);
        Task<UserDto> GetUserByIdAsync(int id, CancellationToken cancellationToken);
        Task CreateUserAsync(UserDto userDto, CancellationToken cancellationToken);
        Task UpdateUserAsync(UserDto userDto, CancellationToken cancellationToken);
        Task DeleteUserAsync(int id, CancellationToken cancellationToken);
        Task SubscriptionAsync(ClaimsPrincipal userPrincipal, int authorId, CancellationToken cancellationToken);
        Task PostLikeAsync(ClaimsPrincipal userPrincipal, int postId, CancellationToken cancellationToken);
    }
}