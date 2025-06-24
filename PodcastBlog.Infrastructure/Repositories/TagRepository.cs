using Microsoft.EntityFrameworkCore;
using PodcastBlog.Domain.Interfaces.Repositories;
using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;

namespace PodcastBlog.Infrastructure.Repositories
{
    public class TagRepository : Repository<Tag>, ITagRepository
    {
        public TagRepository(PodcastBlogContext context) : base(context) { }

        public async Task<PagedList<Tag>> GetAllTagsPagedAsync(Parameters parameters, CancellationToken cancellationToken)
        {
            var tagsQuery = _context.Tags.OrderBy(t => t.Name).AsQueryable();

            var count = await tagsQuery.CountAsync(cancellationToken);

            var tags = await tagsQuery
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedList<Tag>(tags, count, parameters.PageNumber, parameters.PageSize);
        }

        public override async Task<Tag?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _context.Tags
                .Include(t => t.Posts)
                .FirstOrDefaultAsync(t => t.TagId == id, cancellationToken);
        }

        public async Task<Tag?> GetTagByNameAsync(string name, CancellationToken cancellationToken)
        {
            return await _context.Tags
                .Include(t => t.Posts)
                .FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
        }
    }
}
