using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Domain.Models;

namespace PodcastBlog.Application.Strategies
{
    public class TagCleanupStrategy : ITagCleanupStrategy
    {
        public Task CleanupAsync(Tag tag, CancellationToken cancellationToken)
        {
            foreach (var post in tag.Posts.ToList())
            {
                post.Tags.Remove(tag);
            }

            return Task.CompletedTask;
        }
    }
}