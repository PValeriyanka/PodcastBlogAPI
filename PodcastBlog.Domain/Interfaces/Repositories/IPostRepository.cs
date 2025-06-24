using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;
using PodcastBlog.Domain.Parameters.ModelParameters;

namespace PodcastBlog.Domain.Interfaces.Repositories
{
    public interface IPostRepository : IRepository<Post>
    {
        Task<List<Post>> GetSheduledPostsAsync(CancellationToken cancellationToken);
        Task<PagedList<Post>> GetPostsPagedAsync(PostParameters parameters, User? author, string? type, CancellationToken cancellationToken);
        Task<Post?> GetByPodcastIdAsync(int podcastId, CancellationToken cancellationToken);
    }
}
