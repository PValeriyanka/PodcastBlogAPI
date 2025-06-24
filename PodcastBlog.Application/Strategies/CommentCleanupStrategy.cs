using PodcastBlog.Domain.Interfaces.Repositories;
using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Domain.Models;

namespace PodcastBlog.Application.Strategies
{
    public class CommentCleanupStrategy : ICommentCleanupStrategy
    {
        private readonly ICommentRepository _commentRepository;

        public CommentCleanupStrategy(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task CleanupAsync(Comment comment, CancellationToken cancellationToken)
        {
            foreach (var reply in comment.Replies.ToList())
            {
                await _commentRepository.DeleteAsync(reply, cancellationToken);
            }
        }
    }
}