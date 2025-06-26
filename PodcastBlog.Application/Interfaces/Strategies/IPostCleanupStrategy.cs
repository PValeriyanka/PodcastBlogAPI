using PodcastBlog.Domain.Models;
using System.Security.Claims;

namespace PodcastBlog.Application.Interfaces.Strategies
{
    public interface IPostCleanupStrategy
    {
        Task CleanupAsync(Post post, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken);
    }
}