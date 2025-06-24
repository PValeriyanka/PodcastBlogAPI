using Microsoft.EntityFrameworkCore;
using PodcastBlog.Domain.Interfaces.Repositories;
using PodcastBlog.Domain.Models;

namespace PodcastBlog.Infrastructure.Repositories
{
    public class PodcastRepository : Repository<Podcast>, IPodcastRepository
    {
        public PodcastRepository(PodcastBlogContext context) : base(context) { }

        public override async Task<Podcast?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _context.Podcasts
                .FirstOrDefaultAsync(p => p.PodcastId == id, cancellationToken);
        }
    }
}
