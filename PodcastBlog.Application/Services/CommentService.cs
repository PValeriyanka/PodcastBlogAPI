using AutoMapper;
using Microsoft.Extensions.Logging;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Application.ModelsDto.Comment;
using PodcastBlog.Domain.Interfaces;
using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;
using System.Security.Claims;

namespace PodcastBlog.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly ICommentCleanupStrategy _cleanup;
        private readonly IMapper _mapper;
        private readonly ILogger<CommentService> _logger;

        public CommentService(IUnitOfWork unitOfWork, INotificationService notificationService, ICommentCleanupStrategy cleanup, IMapper mapper, ILogger<CommentService> logger)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _cleanup = cleanup;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedList<CommentDto>> GetCommentsByPostPagedAsync(int postId, Parameters parameters, CancellationToken cancellationToken)
        {
            try
            {
                var comments = await _unitOfWork.Comments.GetCommentsByPostPagedAsync(postId, parameters, cancellationToken);

                var commentsDto = _mapper.Map<IEnumerable<CommentDto>>(comments).ToList();

                _logger.LogInformation("Комментарии успешно загружены");
                return new PagedList<CommentDto>(commentsDto, comments.MetaData.TotalCount, comments.MetaData.CurrentPage, comments.MetaData.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке получения комментариев");
                throw;
            }
        }

        public async Task<CommentDto> GetCommentByIdAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                var comment = await _unitOfWork.Comments.GetByIdAsync(id, cancellationToken);

                if (comment is null)
                {
                    _logger.LogWarning("Получение. Комментарий не найден");
                    return null;
                }

                var commentDto = _mapper.Map<CommentDto>(comment);
                _logger.LogInformation("Комментарий успешно получен");

                return commentDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении комментария");
                throw;
            }
        }

        public async Task CreateCommentAsync(CreateCommentDto createCommentDto, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken)
        {
            try
            {
                int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);

                var comment = _mapper.Map<Comment>(createCommentDto);
                comment.UserId = userId;
                comment.Status = CommentStatus.Pending;
                comment.CreatedAt = DateTime.UtcNow;

                await _unitOfWork.Comments.CreateAsync(comment, cancellationToken);

                var post = await _unitOfWork.Posts.GetByIdAsync(comment.PostId, cancellationToken);

                await _notificationService.NewCommentNotificationAsync(comment, post, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Комментарий создан успешно");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании комментария");
                throw;
            }
        }

        public async Task PublishCommentAsync(int id, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken)
        {
            try
            {
                var comment = await _unitOfWork.Comments.GetByIdAsync(id, cancellationToken);

                if (comment is null)
                {
                    _logger.LogWarning("Публикация. Комментарий не найден");
                    return;
                }

                int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);

                if (userId != comment.Post.AuthorId)
                {
                    _logger.LogWarning("Комментарий опубликовать может только создатель поста");
                    return;
                }

                comment.Status = CommentStatus.Approved;
                comment.CreatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Комментарий успешно опубликован");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при публикации комментария");
            }
        }

        public async Task DeleteCommentAsync(int id, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken)
        {
            try
            {
                var comment = await _unitOfWork.Comments.GetByIdAsync(id, cancellationToken);

                if (comment is null)
                {
                    _logger.LogWarning("Удаление. Комментарий не найден");
                    return;
                }

                int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);

                var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);

                if (userId != comment.Post.AuthorId && userId != comment.UserId && user.Role != UserRole.Administrator)
                {
                    _logger.LogWarning("Комментарий удалить может только автор, создатель поста или администатор");
                    return;
                }

                await _cleanup.CleanupAsync(comment, cancellationToken);

                await _unitOfWork.Comments.DeleteAsync(comment, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Комментарий успешно удален");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении комментария");
                throw;
            }
        }
    }
}