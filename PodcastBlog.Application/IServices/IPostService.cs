using PodcastBlog.Application.ModelsDTO;
using PodcastBlog.Domain.Parameters;
using PodcastBlog.Domain.Parameters.ModelParameters;

namespace PodcastBlog.Application.IServices
{
    public interface IPostService
    {
        Task<PagedList<PostDTO>> GetAllPostsPaged(PostParameters parameters, CancellationToken cancellationToken);
        Task<PostDTO> GetPostById(int id, CancellationToken cancellationToken);
        Task CreatePost(PostDTO postDTO, CancellationToken cancellationToken);
        Task UpdatePost(PostDTO postDTO, CancellationToken cancellationToken);
        Task DeletePost(int id, CancellationToken cancellationToken);
    }
}
