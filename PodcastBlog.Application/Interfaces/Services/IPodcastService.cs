using PodcastBlog.Application.ModelsDto;

namespace PodcastBlog.Application.Interfaces.Services
{
    public interface IPodcastService
    {
        Task<PodcastDto> GetPodcastByIdAsync(int id, CancellationToken cancellationToken);
        Task CreatePodcastAsync(PodcastDto podcastDto, CancellationToken cancellationToken);
        Task UpdatePodcastAsync(PodcastDto podcastDto, CancellationToken cancellationToken);
        Task DeletePodcastAsync(int id, CancellationToken cancellationToken);
        Task ListeningAsync(int podcastId, CancellationToken cancellationToken);
    }
}