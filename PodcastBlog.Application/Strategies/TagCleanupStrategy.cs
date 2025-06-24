using PodcastBlog.Domain.Models;
using PodcastBlog.Application.Interfaces.Strategies;

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