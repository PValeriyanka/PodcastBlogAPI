using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;

namespace PodcastBlog.Domain.Interfaces.Repositories
{
    public interface ITagRepository : IRepository<Tag>
    {
        Task<PagedList<Tag>> GetAllTagsPagedAsync(Parameters.Parameters parameters, CancellationToken cancellationToken);
        Task<Tag?> GetTagByNameAsync(string name, CancellationToken cancellationToken);
    }
}
