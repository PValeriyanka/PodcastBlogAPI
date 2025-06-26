using PodcastBlog.Application.ModelsDto.Tag;
using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;

namespace PodcastBlog.Application.Interfaces.Services
{
    public interface ITagService
    {
        Task<PagedList<TagDto>> GetAllTagsPagedAsync(Parameters parameters, CancellationToken cancellationToken);
        Task<TagDto> GetTagByIdAsync(int id, CancellationToken cancellationToken);
        Task CreateTagAsync(CreateTagDto createTagDto, CancellationToken cancellationToken);
        Task DeleteTagAsync(int id, CancellationToken cancellationToken);
        Task<List<Tag>> ResolveTagsFromStringAsync(string? tags, CancellationToken cancellationToken);
    }
}