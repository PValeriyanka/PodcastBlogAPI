using PodcastBlog.Domain.Models;

namespace PodcastBlog.Domain.IRepositories
{
    public interface ITagRepository
    {
        Task<Tag> GetTagById(int id, CancellationToken cancellationToken);
        Task CreateTag(Tag tag, CancellationToken cancellationToken);
        Task UpdateTag(Tag tag, CancellationToken cancellationToken);
        Task DeleteTag(int id, CancellationToken cancellationToken);
    }
}
