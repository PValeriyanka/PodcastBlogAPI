using Microsoft.EntityFrameworkCore;
using PodcastBlog.Domain.IRepositories;
using PodcastBlog.Domain.Models;

namespace PodcastBlog.Infrastructure.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly PodcastBlogContext _context;

        public TagRepository(PodcastBlogContext context)
        {
            _context = context;
        }

        public async Task<Tag> GetTagById(int id, CancellationToken cancellationToken)
        {
            return await _context.Tags.FirstOrDefaultAsync(t => t.TagId == id, cancellationToken);
        }

        public async Task CreateTag(Tag tag, CancellationToken cancellationToken)
        {
            await _context.Tags.AddAsync(tag, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateTag(Tag tag, CancellationToken cancellationToken)
        {
            _context.Tags.Update(tag);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteTag(int id, CancellationToken cancellationToken)
        {
            var tag = await GetTagById(id, cancellationToken);
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
