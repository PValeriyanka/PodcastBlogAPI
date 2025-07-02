using AutoMapper;
using Microsoft.Extensions.Logging;
using PodcastBlog.Application.Exceptions;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Application.ModelsDto.Tag;
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
        private readonly ILogger<TagService> _logger;

        public TagService(IUnitOfWork unitOfWork, ITagCleanupStrategy cleanup, IMapper mapper, ILogger<TagService> logger)
        {
            _unitOfWork = unitOfWork;
            _cleanup = cleanup;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedList<TagDto>> GetAllTagsPagedAsync(Parameters parameters, CancellationToken cancellationToken)
        {
            var tags = await _unitOfWork.Tags.GetAllTagsPagedAsync(parameters, cancellationToken);

            var tagsDto = _mapper.Map<IEnumerable<TagDto>>(tags).ToList();

            _logger.LogInformation("Теги успешно получены");

            return new PagedList<TagDto>(tagsDto, tags.MetaData.TotalCount, tags.MetaData.CurrentPage, tags.MetaData.PageSize);
        }

        public async Task<TagDto> GetTagByIdAsync(int id, CancellationToken cancellationToken)
        {
            var tag = await _unitOfWork.Tags.GetByIdAsync(id, cancellationToken);

            if (tag is null)
            {
                _logger.LogWarning("Получение. Тег не найден");

                throw new NotFoundException("Тег не найден");
            }

            var tagDto = _mapper.Map<TagDto>(tag);

            _logger.LogInformation("Тег успешно получен");

            return tagDto;
        }

        public async Task CreateTagAsync(CreateTagDto createTagDto, CancellationToken cancellationToken)
        {
            var tag = _mapper.Map<Tag>(createTagDto);

            await _unitOfWork.Tags.CreateAsync(tag, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Тег успешно создан");
        }

        public async Task DeleteTagAsync(int id, CancellationToken cancellationToken)
        {
            var tag = await _unitOfWork.Tags.GetByIdAsync(id, cancellationToken);

            if (tag is null)
            {
                _logger.LogWarning("Удаление. Тег не найден");

                throw new NotFoundException("Тег не найден");
            }

            await _cleanup.CleanupAsync(tag, cancellationToken);

            await _unitOfWork.Tags.DeleteAsync(tag, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Тег успешно удален");
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

                        _logger.LogInformation("Новый тег успешно создан");
                    }
                }
            }

            return result;
        }
    }
}