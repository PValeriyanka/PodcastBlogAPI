using PodcastBlog.Application.ModelsDto.Notification;
using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;
using System.Security.Claims;

namespace PodcastBlog.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task<PagedList<NotificationDto>> GetNotificationsByUserPagedAsync(ClaimsPrincipal userPrincipal, Parameters parameters, CancellationToken cancellationToken);
        Task<NotificationDto> GetNotificationByIdAsync(int id, CancellationToken cancellationToken);
        Task CreatePostNotificationAsync(int userId, CancellationToken cancellationToken);
        Task NewSubscriberNotificationAsync(int userId, int authorId, CancellationToken cancellationToken);
        Task NewLikeNotificationAsync(int userId, Post post, CancellationToken cancellationToken);
        Task NewCommentNotificationAsync(Comment comment, Post post, CancellationToken cancellationToken);
        Task ReadNotificationAsync(int id, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken);
        Task DeleteNotificationAsync(int id, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken);
    }
}
