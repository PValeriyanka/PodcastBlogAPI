using Microsoft.EntityFrameworkCore;
using PodcastBlog.Domain.Interfaces.Repositories;
using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;

namespace PodcastBlog.Infrastructure.Repositories
{
    public class CommentRepository : Repository<Comment>, ICommentRepository
    {
        public CommentRepository(PodcastBlogContext context) : base(context) { }

        public async Task<PagedList<Comment>> GetCommentsByPostPagedAsync(int postId, Parameters parameters, CancellationToken cancellationToken)
        {
            var commentsQuery = _context.Comments
                .Where(c => c.PostId == postId && c.Status == CommentStatus.Approved)
                .Include(c => c.User)
                .AsNoTracking()
                .AsQueryable();

            var count = await commentsQuery.CountAsync(cancellationToken);

            var comments = await commentsQuery
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedList<Comment>(comments, count, parameters.PageNumber, parameters.PageSize);
        }

        public override async Task<Comment?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _context.Comments
                .Include(c => c.Post)
                .Include(c => c.User)
                .Include(c => c.Parent)
                .Include(c => c.Replies)
                    .ThenInclude(reply => reply.User)
                .Include(c => c.Replies)
                    .ThenInclude(reply => reply.Post)
                .FirstOrDefaultAsync(c => c.CommentId == id, cancellationToken);
        }
    }
}
