using AutoMapper;
using Microsoft.Extensions.Logging;
using PodcastBlog.Application.Exceptions;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Application.ModelsDto.Notification;
using PodcastBlog.Domain.Interfaces;
using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;
using System.Security.Claims;

namespace PodcastBlog.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly INotificationCleanupStrategy _cleanup;
        private readonly IMapper _mapper;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IUnitOfWork unitOfWork, IEmailService emailService, INotificationCleanupStrategy cleanup, IMapper mapper, ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _cleanup = cleanup;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedList<NotificationDto>> GetNotificationsByUserPagedAsync(ClaimsPrincipal userPrincipal, Parameters parameters, CancellationToken cancellationToken)
        {
            int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);

            var notifications = await _unitOfWork.Notifications.GetNotificationsByUserPagedAsync(userId, parameters, cancellationToken);

            var notificationsDto = _mapper.Map<IEnumerable<NotificationDto>>(notifications).ToList();

            _logger.LogInformation("Уведомления пользователя успешно загружены");

            return new PagedList<NotificationDto>(notificationsDto, notifications.MetaData.TotalCount, notifications.MetaData.CurrentPage, notifications.MetaData.PageSize);
        }

        public async Task<NotificationDto> GetNotificationByIdAsync(int id, CancellationToken cancellationToken)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(id, cancellationToken);

            if (notification is null)
            {
                _logger.LogWarning("Получение. Уведомление не найдено");

                throw new NotFoundException("Уведомление не найдено");
            }

            var notificationDto = _mapper.Map<NotificationDto>(notification);

            _logger.LogInformation("Уведомление успешно получено");

            return notificationDto;
        }

        public async Task CreatePostNotificationAsync(int userId, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

            if (user is null)
            {
                _logger.LogWarning("Создание уведомления. Пользователь не найден");

                return;
            }

            var message = user.Name + " опубликовал новый пост.";

            foreach (var follower in user.Followers)
            {
                var notification = new Notification
                {
                    UserId = follower.Subscriber.Id,
                    Message = message,
                    IsRead = false
                };

                await _unitOfWork.Notifications.CreateAsync(notification, cancellationToken);

                await EmailNotification(follower.Subscriber, message);
            }

            var author_notification = new Notification
            {
                UserId = user.Id,
                Message = "Пост успешно опубликован!",
                IsRead = false
            };

            await _unitOfWork.Notifications.CreateAsync(author_notification, cancellationToken);

            await EmailNotification(user, author_notification.Message);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Уведомления о публикации поста успешно созданы");
        }

        public async Task NewSubscriberNotificationAsync(int userId, int authorId, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

            var author = await _unitOfWork.Users.GetByIdAsync(authorId, cancellationToken);

            if (user is null || author is null)
            {
                _logger.LogWarning("Создание уведомления. Один из пользователей не найден");

                return;
            }

            var message = user.Name + " подписался на Вас.";

            var notification = new Notification
            {
                UserId = authorId,
                Message = message,
                IsRead = false
            };

            await _unitOfWork.Notifications.CreateAsync(notification, cancellationToken);

            await EmailNotification(author, message);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Уведомление о подписке успешно создано");
        }

        public async Task NewLikeNotificationAsync(int userId, Post post, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

            var author = await _unitOfWork.Users.GetByIdAsync(post.AuthorId, cancellationToken);

            if (user is null || author is null)
            {
                _logger.LogWarning("Создание уведомления. Один из пользователей не найден");

                return;
            }

            var message = user.Name + " нравится Ваш пост '" + post.Title + "'.";

            var notification = new Notification
            {
                UserId = post.AuthorId,
                Message = message,
                IsRead = false
            };

            await _unitOfWork.Notifications.CreateAsync(notification, cancellationToken);

            await EmailNotification(author, message);

            _logger.LogInformation("Уведомление о лайке успешно создано");
        }

        public async Task NewCommentNotificationAsync(Comment comment, Post post, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(comment.UserId, cancellationToken);

            var author = await _unitOfWork.Users.GetByIdAsync(post.AuthorId, cancellationToken);

            if (user is null || author is null)
            {
                _logger.LogWarning("Создание уведомления. Один из пользователей не найден");

                return;
            }

            var message = user.Name + " оставил комментарий: '" + comment.Content + "' под Вашим постом '" + post.Title + "'.";

            var notification = new Notification
            {
                UserId = post.AuthorId,
                Message = message,
                IsRead = false
            };

            await _unitOfWork.Notifications.CreateAsync(notification, cancellationToken);

            await EmailNotification(author, message);

            _logger.LogInformation("Уведомление о комментарии успешно создано");
        }

        private async Task EmailNotification(User user, string message)
        {
            if (user.EmailNotify && !string.IsNullOrWhiteSpace(user.Email))
            {
                try
                {
                    await _emailService.SendNotificationEmailAsync(user.Email, "Новое уведомление!", message);

                    _logger.LogInformation("Email успешно отправлен");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Ошибка при отправке email");
                }
            }
        }

        public async Task ReadNotificationAsync(int id, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(id, cancellationToken);

            if (notification is null)
            {
                _logger.LogWarning("Чтение. Уведомление не найдено");

                throw new NotFoundException("Уведомление не найдено");
            }

            int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);

            if (userId != notification.UserId)
            {
                _logger.LogWarning("Уведомление отметить прочитанным может только пользователь, которому оно адресовано");

                throw new ForbiddenException("Уведомление отметить прочитанным может только пользователь, которому оно адресовано");
            }

            notification.IsRead = true;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Уведомление успешно помечено как прочитанное");
        }

        public async Task DeleteNotificationAsync(int id, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(id, cancellationToken);

            if (notification is null)
            {
                _logger.LogWarning("Удаление. Уведомление не найдено");

                throw new NotFoundException("Уведомление не найдено");
            }

            int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);

            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

            if (userId != notification.UserId && user.Role != UserRole.Administrator)
            {
                _logger.LogWarning("Уведомление удалить может только пользователь, которому оно адресовано или администратор");

                throw new ForbiddenException("Уведомление удалить может только пользователь, которому оно адресовано или администратор");
            }

            await _cleanup.CleanupAsync(notification, cancellationToken);

            await _unitOfWork.Notifications.DeleteAsync(notification, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Уведомление успешно удалено");
        }
    }
}
