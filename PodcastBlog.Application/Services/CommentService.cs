using AutoMapper;
using Microsoft.Extensions.Logging;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Application.ModelsDto.Comment;
using PodcastBlog.Domain.Interfaces;
using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;
using PodcastBlog.Infrastructure.ExceptionsHandler.Exceptions;
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
            var comments = await _unitOfWork.Comments.GetCommentsByPostPagedAsync(postId, parameters, cancellationToken);

            var commentsDto = _mapper.Map<IEnumerable<CommentDto>>(comments).ToList();

            _logger.LogInformation("Комментарии успешно загружены");

            return new PagedList<CommentDto>(commentsDto, comments.MetaData.TotalCount, comments.MetaData.CurrentPage, comments.MetaData.PageSize);
        }

        public async Task<CommentDto> GetCommentByIdAsync(int id, CancellationToken cancellationToken)
        {
            var comment = await _unitOfWork.Comments.GetByIdAsync(id, cancellationToken);

            if (comment is null)
            {
                _logger.LogWarning("Получение. Комментарий не найден");

                throw new NotFoundException("Комментарий не найден");
            }

            var commentDto = _mapper.Map<CommentDto>(comment);

            _logger.LogInformation("Комментарий успешно получен");

            return commentDto;
        }

        public async Task CreateCommentAsync(CreateCommentDto createCommentDto, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken)
        {
            int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);

            if (createCommentDto.ParentId is not null)
            {
                var comm = await _unitOfWork.Comments.GetByIdAsync(createCommentDto.ParentId.Value, cancellationToken);

                if (comm is null)
                {
                    createCommentDto.ParentId = null;
                }
            }

            var post = await _unitOfWork.Posts.GetByIdAsync(createCommentDto.PostId, cancellationToken);

            if (post is null)
            {
                _logger.LogWarning("Создание комментария. Пост не найден");

                throw new NotFoundException("Пост не найден");
            }

            var comment = _mapper.Map<Comment>(createCommentDto);
            comment.UserId = userId;
            comment.Status = CommentStatus.Pending;
            comment.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Comments.CreateAsync(comment, cancellationToken);

            await _notificationService.NewCommentNotificationAsync(comment, post, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Комментарий создан успешно");
        }

        public async Task PublishCommentAsync(int id, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken)
        {
            var comment = await _unitOfWork.Comments.GetByIdAsync(id, cancellationToken);

            if (comment is null)
            {
                _logger.LogWarning("Публикация. Комментарий не найден");

                throw new NotFoundException("Комментарий не найден");
            }

            int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);

            if (userId != comment.Post.AuthorId)
            {
                _logger.LogWarning("Комментарий опубликовать может только создатель поста");

                throw new ForbiddenException("Комментарий опубликовать может только создатель поста");
            }

            comment.Status = CommentStatus.Approved;
            comment.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Комментарий успешно опубликован");
        }

        public async Task DeleteCommentAsync(int id, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken)
        {
            var comment = await _unitOfWork.Comments.GetByIdAsync(id, cancellationToken);

            if (comment is null)
            {
                _logger.LogWarning("Удаление. Комментарий не найден");

                throw new NotFoundException("Комментарий не найден");
            }

            int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);

            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

            if (userId != comment.Post.AuthorId && userId != comment.UserId && user.Role != UserRole.Administrator)
            {
                _logger.LogWarning("Комментарий удалить может только автор, создатель поста или администатор");

                throw new ForbiddenException("Комментарий удалить может только автор, создатель поста или администатор");
            }

            await _cleanup.CleanupAsync(comment, cancellationToken);

            await _unitOfWork.Comments.DeleteAsync(comment, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Комментарий успешно удален");
        }
    }
}