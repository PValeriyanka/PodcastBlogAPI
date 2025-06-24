using AutoMapper;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Application.ModelsDto;
using PodcastBlog.Domain.Interfaces;
using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;

namespace PodcastBlog.Application.Services
{
    public class TagService : ITagService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITagCleanupStrategy _cleanup;
        private readonly IMapper _mapper;

        public TagService(IUnitOfWork unitOfWork, ITagCleanupStrategy cleanup, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _cleanup = cleanup;
            _mapper = mapper;
        }

        public async Task<PagedList<TagDto>> GetAllTagsPagedAsync(Parameters parameters, CancellationToken cancellationToken)
        {
            var tags = await _unitOfWork.Tags.GetAllTagsPagedAsync(parameters, cancellationToken);

            var tagsDto = _mapper.Map<IEnumerable<TagDto>>(tags).ToList();

            return new PagedList<TagDto>(tagsDto, tags.MetaData.TotalCount, tags.MetaData.CurrentPage, tags.MetaData.PageSize);
        }

        public async Task<TagDto> GetTagByIdAsync(int id, CancellationToken cancellationToken)
        {
            var tag = await _unitOfWork.Tags.GetByIdAsync(id, cancellationToken);

            var tagDto = _mapper.Map<TagDto>(tag);

            return tagDto;
        }

        public async Task CreateTagAsync(TagDto tagDto, CancellationToken cancellationToken)
        {
            var tag = _mapper.Map<Tag>(tagDto);

            await _unitOfWork.Tags.CreateAsync(tag, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateTagAsync(TagDto tagDto, CancellationToken cancellationToken)
        {
            var tag = await _unitOfWork.Tags.GetByIdAsync(tagDto.TagId, cancellationToken);

            _mapper.Map(tagDto, tag);

            await _unitOfWork.Tags.UpdateAsync(tag, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteTagAsync(int id, CancellationToken cancellationToken)
        {
            var tag = await _unitOfWork.Tags.GetByIdAsync(id, cancellationToken);

            await _cleanup.CleanupAsync(tag, cancellationToken);

            await _unitOfWork.Tags.DeleteAsync(tag, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Tag>> ResolveTagsFromStringAsync(string? tags, CancellationToken cancellationToken)
        {
            var result = new List<Tag>();

            if (!string.IsNullOrWhiteSpace(tags))
            {
                var tagNames = tags
                    .Split(new[] { '#', ' ', ',', '.', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                foreach (var name in tagNames)
                {
                    var tag = await _unitOfWork.Tags.GetTagByNameAsync(name, cancellationToken);

                    if (tag is not null)
                    {
                        result.Add(tag);
                    }
                    else
                    {
                        var newTag = new Tag { Name = name };

                        await _unitOfWork.Tags.CreateAsync(newTag, cancellationToken);

                        result.Add(newTag);
                    }
                }
            }

            return result;
        }
    }
}