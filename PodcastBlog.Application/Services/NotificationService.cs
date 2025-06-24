using AutoMapper;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Application.ModelsDto;
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

        public NotificationService(IUnitOfWork unitOfWork, IEmailService emailService, INotificationCleanupStrategy cleanup, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _cleanup = cleanup;
            _mapper = mapper;
        }

        public async Task<PagedList<NotificationDto>> GetNotificationsByUserPagedAsync(ClaimsPrincipal userPrincipal, Parameters parameters, CancellationToken cancellationToken)
        {
            int.TryParse(userPrincipal.FindFirstValue("sub"), out int userId); // !

            var notifications = await _unitOfWork.Notifications.GetNotificationsByUserPagedAsync(userId, parameters, cancellationToken);

            var notificationsDto = _mapper.Map<IEnumerable<NotificationDto>>(notifications).ToList();

            return new PagedList<NotificationDto>(notificationsDto, notifications.MetaData.TotalCount, notifications.MetaData.CurrentPage, notifications.MetaData.PageSize);
        }

        public async Task<NotificationDto> GetNotificationByIdAsync(int id, CancellationToken cancellationToken)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(id, cancellationToken);

            var notificationDto = _mapper.Map<NotificationDto>(notification);

            return notificationDto;
        }

        public async Task CreatePostNotificationAsync(int userId, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

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

            message = "Пост успешно опубликован!";

            var author_notification = new Notification
            {
                UserId = user.Id,
                Message = message,
                IsRead = false
            };

            await _unitOfWork.Notifications.CreateAsync(author_notification, cancellationToken);

            await EmailNotification(user, message);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task NewSubscriberNotificationAsync(int userId, int authorId, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

            var message = user.Name + " подписался на Вас.";

            var notification = new Notification
            {
                UserId = authorId,
                Message = message,
                IsRead = false
            };

            await _unitOfWork.Notifications.CreateAsync(notification, cancellationToken);

            var author = await _unitOfWork.Users.GetByIdAsync(authorId, cancellationToken);

            await EmailNotification(author, message);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task NewLikeNotificationAsync(int userId, Post post, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

            var message = user.Name + " нравится Ваш пост '" + post.Title + "'.";

            var notification = new Notification
            {
                UserId = post.AuthorId,
                Message = message,
                IsRead = false
            };

            await _unitOfWork.Notifications.CreateAsync(notification, cancellationToken);

            var author = await _unitOfWork.Users.GetByIdAsync(post.AuthorId, cancellationToken);

            await EmailNotification(author, message);
        }

        public async Task NewCommentNotificationAsync(Comment comment, Post post, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(comment.UserId, cancellationToken);

            var message = user.Name + " оставил комментарий: '" + comment.Content + "' под Вашим постом '" + post.Title + "'.";

            var notification = new Notification
            {
                UserId = post.AuthorId,
                Message = message,
                IsRead = false
            };

            await _unitOfWork.Notifications.CreateAsync(notification, cancellationToken);

            var author = await _unitOfWork.Users.GetByIdAsync(post.AuthorId, cancellationToken);

            await EmailNotification(author, message);
        }

        private async Task EmailNotification(User user, string message)
        {
            if (user.EmailNotify && !string.IsNullOrWhiteSpace(user.Email))
            {
                await _emailService.SendNotificationEmailAsync(user.Email, "Новое уведомление!", message);
                Console.WriteLine("Отправлено на Email");
            }
        }

        public async Task ReadNotificationAsync(int id, CancellationToken cancellationToken)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(id, cancellationToken);

            notification.IsRead = true;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteNotificationAsync(int id, CancellationToken cancellationToken)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(id, cancellationToken);

            await _cleanup.CleanupAsync(notification, cancellationToken);

            await _unitOfWork.Notifications.DeleteAsync(notification, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
