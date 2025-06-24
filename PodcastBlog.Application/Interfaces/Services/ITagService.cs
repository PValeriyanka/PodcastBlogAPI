using PodcastBlog.Application.ModelsDto;
using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;

namespace PodcastBlog.Application.Interfaces.Services
{
    public interface ITagService
    {
        Task<PagedList<TagDto>> GetAllTagsPagedAsync(Parameters parameters, CancellationToken cancellationToken);
        Task<TagDto> GetTagByIdAsync(int id, CancellationToken cancellationToken);
        Task CreateTagAsync(TagDto tagDto, CancellationToken cancellationToken);
        Task UpdateTagAsync(TagDto tagDto, CancellationToken cancellationToken);
        Task DeleteTagAsync(int id, CancellationToken cancellationToken);
        Task<List<Tag>> ResolveTagsFromStringAsync(string? tags, CancellationToken cancellationToken);
    }
}