using PodcastBlog.Application.ModelsDTO;

namespace PodcastBlog.Application.IServices
{
    public interface ITagService
    {
        Task<TagDTO> GetTagById(int id, CancellationToken cancellationToken);
        Task CreateTag(TagDTO tagDTO, CancellationToken cancellationToken);
        Task UpdateTag(TagDTO tagDTO, CancellationToken cancellationToken);
        Task DeleteTag(int id, CancellationToken cancellationToken);
    }
}
