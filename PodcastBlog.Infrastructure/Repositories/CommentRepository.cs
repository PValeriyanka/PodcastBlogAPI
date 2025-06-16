using Microsoft.EntityFrameworkCore;
using PodcastBlog.Domain.IRepositories;
using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;

namespace PodcastBlog.Infrastructure.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly PodcastBlogContext _context;

        public CommentRepository(PodcastBlogContext context)
        {
            _context = context;
        }

        public async Task<PagedList<Comment>> GetCommentsByPostPaged(int postId, Parameters parameters, CancellationToken cancellationToken)
        {
            var commentsQuery = _context.Comments
                .Where(c => c.PostId == postId)
                .Include(c => c.User)
                .AsQueryable();

            var count = await commentsQuery.CountAsync(cancellationToken);

            var comments = await commentsQuery
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedList<Comment>(comments, count, parameters.PageNumber, parameters.PageSize);
        }

        public async Task<Comment> GetCommentById(int id, CancellationToken cancellationToken)
        {
            return await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == id, cancellationToken);
        }

        public async Task CreateComment(Comment comment, CancellationToken cancellationToken)
        {
            await _context.Comments.AddAsync(comment, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateComment(Comment comment, CancellationToken cancellationToken)
        {
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteComment(int id, CancellationToken cancellationToken)
        {
            var comment = await GetCommentById(id, cancellationToken);
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
