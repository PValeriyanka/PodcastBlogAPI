using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Domain.Interfaces;
using PodcastBlog.Domain.Models;
using System.Security.Claims;

namespace PodcastBlog.Application.Strategies
{
    public class PostCleanupStrategy : IPostCleanupStrategy
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommentService _commentService;

        public PostCleanupStrategy(IUnitOfWork unitOfWork, ICommentService commentService)
        {
            _unitOfWork = unitOfWork;
            _commentService = commentService;
        }

        public async Task CleanupAsync(Post post, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken)
        {
            foreach (var tag in post.Tags.ToList())
            {
                tag.Posts.Remove(post);
            }

            foreach (var comment in post.Comments.ToList())
            {
                await _commentService.DeleteCommentAsync(comment.CommentId, userPrincipal,cancellationToken);
            }

            foreach (var user in post.Likes.ToList())
            {
                user.Liked.Remove(post);
            }
            
            if (post.PodcastId is not null)
            {
                var podcast = await _unitOfWork.Podcasts.GetByIdAsync(post.PodcastId.Value, cancellationToken);

                if (podcast is not null)
                {
                    await _unitOfWork.Podcasts.DeleteAsync(podcast, cancellationToken);
                }
            }
        }
    }
}