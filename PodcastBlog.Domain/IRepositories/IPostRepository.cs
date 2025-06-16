using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;
using PodcastBlog.Domain.Parameters.ModelParameters;

namespace PodcastBlog.Domain.IRepositories
{
    public interface IPostRepository
    {
        Task<PagedList<Post>> GetAllPostsPaged(PostParameters parameters, CancellationToken cancellationToken);
        Task<Post> GetPostById(int id, CancellationToken cancellationToken);
        Task CreatePost(Post post, CancellationToken cancellationToken);
        Task UpdatePost(Post post, CancellationToken cancellationToken);
        Task DeletePost(int id, CancellationToken cancellationToken);
    }
}
