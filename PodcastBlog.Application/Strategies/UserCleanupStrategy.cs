using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Domain.Models;

namespace PodcastBlog.Application.Strategies
{
    public class UserCleanupStrategy : IUserCleanupStrategy
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly INotificationService _notificationService;

        public UserCleanupStrategy(IPostService postService, ICommentService commentService, INotificationService notificationServise)
        {
            _postService = postService;
            _commentService = commentService;
            _notificationService = notificationServise;
        }

        public async Task CleanupAsync(User user, CancellationToken cancellationToken)
        {
            foreach (var post in user.Posts.ToList())
            {
                await _postService.DeletePostAsync(post.PostId, cancellationToken);
            }

            foreach (var comment in user.Comments.ToList())
            {
                await _commentService.DeleteCommentAsync(comment.CommentId, cancellationToken);
            }

            foreach (var likedPost in user.Liked.ToList())
            {
                likedPost.Likes.Remove(user);
            }

            foreach (var notification in user.Notifications.ToList())
            {
                await _notificationService.DeleteNotificationAsync(notification.NotificationId, cancellationToken);
            }

            user.Subscriptions.Clear();
            user.Followers.Clear();
        }
    }
}