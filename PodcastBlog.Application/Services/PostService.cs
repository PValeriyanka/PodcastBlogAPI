using AutoMapper;
using Microsoft.Extensions.Logging;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Application.ModelsDto.Post;
using PodcastBlog.Domain.Interfaces;
using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;
using PodcastBlog.Domain.Parameters.ModelParameters;
using System.Security.Claims;

namespace PodcastBlog.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITagService _tagService;
        private readonly INotificationService _notificationService;
        private readonly IPostCleanupStrategy _cleanup;
        private readonly IMapper _mapper;
        private readonly ILogger<PostService> _logger;

        public PostService(IUnitOfWork unitOfWork, ITagService tagService, INotificationService notificationService, IPostCleanupStrategy cleanup, IMapper mapper, ILogger<PostService> logger)
        {
            _unitOfWork = unitOfWork;
            _tagService = tagService;
            _notificationService = notificationService;
            _cleanup = cleanup;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedList<PostDto>> GetPostsPagedAsync(PostParameters parameters, ClaimsPrincipal userPrincipal, string? type, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTime.UtcNow;

                var sheduledPosts = await _unitOfWork.Posts.GetSheduledPostsAsync(cancellationToken);

                foreach (var post in sheduledPosts)
                {
                    post.Status = PostStatus.Published;

                    await _notificationService.CreatePostNotificationAsync(post.AuthorId, cancellationToken);
                }

                int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int Id);

                var currentUser = await _unitOfWork.Users.GetByIdAsync(Id, cancellationToken);

                var posts = await _unitOfWork.Posts.GetPostsPagedAsync(parameters, currentUser, type, cancellationToken);

                var postsDto = _mapper.Map<IEnumerable<PostDto>>(posts).ToList();

                _logger.LogInformation("Посты успешно загружены");
                return new PagedList<PostDto>(postsDto, posts.MetaData.TotalCount, posts.MetaData.CurrentPage, posts.MetaData.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении постов");
                throw;
            }
        }

        public async Task<PostDto> GetPostByIdAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                var post = await _unitOfWork.Posts.GetByIdAsync(id, cancellationToken);

                if (post is null)
                {
                    _logger.LogWarning("Просмотр. Пост не найден");
                    return null;
                }

                post.Views += 1;

                var postDto = _mapper.Map<PostDto>(post);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Просмотр поста успешно засчитан");
                return postDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении поста");
                throw;
            }
        }

        public async Task CreatePostAsync(CreatePostDto createPostDto, ClaimsPrincipal userPrincipal, string status, CancellationToken cancellationToken)
        {
            try
            {
                int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int authorId);

                var post = _mapper.Map<Post>(createPostDto);
                post.AuthorId = authorId;

                await ApplyStatus(post, status, userPrincipal, cancellationToken);

                post.Tags = await _tagService.ResolveTagsFromStringAsync(createPostDto.Tags, cancellationToken);

                await _unitOfWork.Posts.CreateAsync(post, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Пост успешно создан");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании поста");
                throw;
            }
        }

        public async Task UpdatePostAsync(UpdatePostDto updatePostDto, ClaimsPrincipal userPrincipal, string? status, CancellationToken cancellationToken)
        {
            try
            {
                var post = await _unitOfWork.Posts.GetByIdAsync(updatePostDto.PostId, cancellationToken);

                if (post is null)
                {
                    _logger.LogWarning("Обновление. Пост не найден");
                    return;
                }

                int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);

                var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

                if (userId != post.AuthorId && user.Role != UserRole.Administrator)
                {
                    _logger.LogWarning("Пост изменить может только автор или администратор");
                    return;
                }

                _mapper.Map(updatePostDto, post);

                if (status is not null)
                {
                    await ApplyStatus(post, status, userPrincipal, cancellationToken);
                }

                post.Tags = await _tagService.ResolveTagsFromStringAsync(updatePostDto.Tags, cancellationToken);

                await _unitOfWork.Posts.UpdateAsync(post, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Пост успешно обновлен");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении поста");
                throw;
            }
        }

        public async Task DeletePostAsync(int id, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken)
        {
            try
            {
                var post = await _unitOfWork.Posts.GetByIdAsync(id, cancellationToken);

                if (post is null)
                {
                    _logger.LogWarning("Удаление. Пост не найден");
                    return;
                }

                int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);

                var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

                if (userId != post.AuthorId && user.Role != UserRole.Administrator)
                {
                    _logger.LogWarning("Пост удалить может только автор или администратор");
                    return;
                }

                await _cleanup.CleanupAsync(post, userPrincipal, cancellationToken);

                await _unitOfWork.Posts.DeleteAsync(post, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Пост успешно удален");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении поста");
                throw;
            }
        }

        private async Task ApplyStatus(Post post, string status, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken)
        {
            try
            {
                if ((post.PublishedAt == default || post.PublishedAt <= DateTime.UtcNow) && status == "Publish")
                {
                    post.PublishedAt = DateTime.UtcNow;
                    post.Status = PostStatus.Published;

                    int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);

                    var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

                    await _notificationService.CreatePostNotificationAsync(user.Id, cancellationToken);
                }
                else if (post.PublishedAt > DateTime.UtcNow && status == "Publish")
                {
                    post.Status = PostStatus.Scheduled;
                }
                else
                {
                    post.Status = PostStatus.Draft;
                }

                _logger.LogInformation("Статус поста успешно установлен");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при установке статуса поста");
                throw;
            }
        }
    }
}