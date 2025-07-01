using AutoMapper;
using Microsoft.Extensions.Logging;
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
            try
            {
                var tags = await _unitOfWork.Tags.GetAllTagsPagedAsync(parameters, cancellationToken);

                var tagsDto = _mapper.Map<IEnumerable<TagDto>>(tags).ToList();

                _logger.LogInformation("Теги успешно получены");
                return new PagedList<TagDto>(tagsDto, tags.MetaData.TotalCount, tags.MetaData.CurrentPage, tags.MetaData.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении тегов");
                throw;
            }
        }

        public async Task<TagDto> GetTagByIdAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                var tag = await _unitOfWork.Tags.GetByIdAsync(id, cancellationToken);

                if (tag is null)
                {
                    _logger.LogWarning("Получение. Тег не найден");
                    return null;
                }

                var tagDto = _mapper.Map<TagDto>(tag);

                _logger.LogInformation("Тег успешно получен");
                return tagDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении тега");
                throw;
            }
        }

        public async Task CreateTagAsync(CreateTagDto createTagDto, CancellationToken cancellationToken)
        {
            try
            {
                var tag = _mapper.Map<Tag>(createTagDto);

                await _unitOfWork.Tags.CreateAsync(tag, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Тег успешно создан");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании тега");
                throw;
            }
        }

        public async Task DeleteTagAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                var tag = await _unitOfWork.Tags.GetByIdAsync(id, cancellationToken);

                if (tag is null)
                {
                    _logger.LogWarning("Удаление. Тег не найден");
                    return;
                }

                await _cleanup.CleanupAsync(tag, cancellationToken);

                await _unitOfWork.Tags.DeleteAsync(tag, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Тег успешно удален");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении тега");
                throw;
            }
        }

        public async Task<List<Tag>> ResolveTagsFromStringAsync(string? tags, CancellationToken cancellationToken)
        {
            var result = new List<Tag>();

            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке строки тегов");
                throw;
            }
        }
    }
}