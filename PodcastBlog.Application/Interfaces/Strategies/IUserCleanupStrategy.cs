using PodcastBlog.Domain.Models;
using System.Security.Claims;

namespace PodcastBlog.Application.Interfaces.Strategies
{
    public interface IUserCleanupStrategy
    {
        Task CleanupAsync(User user, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken);
    }
}