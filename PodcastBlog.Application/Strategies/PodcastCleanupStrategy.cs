using PodcastBlog.Domain.Interfaces;
using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Domain.Models;

namespace PodcastBlog.Application.Strategies
{
    public class PodcastCleanupStrategy : IPodcastCleanupStrategy
    {
        private readonly IUnitOfWork _unitOfWork;

        public PodcastCleanupStrategy(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CleanupAsync(Podcast podcast, CancellationToken cancellationToken)
        {
            var post = await _unitOfWork.Posts.GetByPodcastIdAsync(podcast.PodcastId, cancellationToken);

            if (post is not null)
                post.PodcastId = null;
        }
    }
}
