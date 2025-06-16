using PodcastBlog.Domain.Models;

namespace PodcastBlog.Domain.IRepositories
{
    public interface IPodcastRepository
    {
        Task<Podcast> GetPodcastById(int id, CancellationToken cancellationToken);
        Task CreatePodcast(Podcast podcast, CancellationToken cancellationToken);
        Task UpdatePodcast(Podcast podcast, CancellationToken cancellationToken);
        Task DeletePodcast(int id, CancellationToken cancellationToken);
    }
}
