using PodcastBlog.Application.ModelsDTO;

namespace PodcastBlog.Application.IServices
{
    public interface IPodcastService
    {
        Task<PodcastDTO> GetPodcastById(int id, CancellationToken cancellationToken);
        Task CreatePodcast(PodcastDTO podcastDTO, CancellationToken cancellationToken);
        Task UpdatePodcast(PodcastDTO podcastDTO, CancellationToken cancellationToken);
        Task DeletePodcast(int id, CancellationToken cancellationToken);
    }
}
