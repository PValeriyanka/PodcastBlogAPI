using Microsoft.EntityFrameworkCore;
using PodcastBlog.Domain.IRepositories;
using PodcastBlog.Domain.Models;

namespace PodcastBlog.Infrastructure.Repositories
{
    public class PodcastRepository : IPodcastRepository
    {
        private readonly PodcastBlogContext _context;

        public PodcastRepository(PodcastBlogContext context)
        {
            _context = context;
        }

        public async Task<Podcast> GetPodcastById(int id, CancellationToken cancellationToken)
        {
            return await _context.Podcasts.FirstOrDefaultAsync(p => p.PodcastId == id, cancellationToken);
        }

        public async Task CreatePodcast(Podcast podcast, CancellationToken cancellationToken)
        {
            await _context.Podcasts.AddAsync(podcast, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdatePodcast(Podcast podcast, CancellationToken cancellationToken)
        {
            _context.Podcasts.Update(podcast);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeletePodcast(int id, CancellationToken cancellationToken)
        {
            var podcast = await GetPodcastById(id, cancellationToken);
            _context.Podcasts.Remove(podcast);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
