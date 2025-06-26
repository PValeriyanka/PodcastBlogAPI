using PodcastBlog.Application.ModelsDto.User;
using PodcastBlog.Domain.Parameters;
using System.Security.Claims;

namespace PodcastBlog.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<PagedList<UserDto>> GetAllUsersPagedAsync(Parameters parameters, CancellationToken cancellationToken);
        Task<UserDto> GetUserByIdAsync(int id, CancellationToken cancellationToken);
        Task UpdateUserAsync(UpdateUserDto updateUserDto, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken);
        Task UpdateUserRoleAsync(UpdateUserRoleDto updateUserRoleDto, CancellationToken cancellationToken);
        Task DeleteUserAsync(int id, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken);
        Task SubscriptionAsync(int authorId, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken);
        Task PostLikeAsync(int postId, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken);
    }
}