using PodcastBlog.Application.ModelsDto.Podcast;
using System.Security.Claims;

namespace PodcastBlog.Application.Interfaces.Services
{
    public interface IPodcastService
    {
        Task<PodcastDto> GetPodcastByIdAsync(int id, CancellationToken cancellationToken);
        Task CreatePodcastAsync(CreatePodcastDto createPodcastDto, CancellationToken cancellationToken);
        Task UpdatePodcastAsync(UpdatePodcastDto updatePodcastDto, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken);
        Task DeletePodcastAsync(int id, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken);
        Task ListeningAsync(int podcastId, CancellationToken cancellationToken);
    }
}