using Microsoft.AspNetCore.Identity;
using PodcastBlog.Application.ModelsDto.Authentication;

namespace PodcastBlog.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<string> AuthenticateAsync(string email, string password, CancellationToken cancellationToken);
        Task<IdentityResult> RegisterAsync(RegistrationDto registerDto, CancellationToken cancellationToken);
    }
}
