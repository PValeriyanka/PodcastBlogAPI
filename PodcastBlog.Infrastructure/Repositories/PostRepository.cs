using Microsoft.EntityFrameworkCore;
using PodcastBlog.Domain.IRepositories;
using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;
using PodcastBlog.Domain.Parameters.ModelParameters;

namespace PodcastBlog.Infrastructure.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly PodcastBlogContext _context;

        public PostRepository(PodcastBlogContext context)
        {
            _context = context;
        }

        public async Task<PagedList<Post>> GetAllPostsPaged(PostParameters parameters, CancellationToken cancellationToken)
        {
            var postsQuery = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Podcast)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(parameters.searchByDate) && DateTime.TryParse(parameters.searchByDate, out DateTime date))
                postsQuery = postsQuery.Where(p => p.PublishedAt.Day == date.Day && p.PublishedAt.Month == date.Month && p.PublishedAt.Year == date.Year);
            if (!string.IsNullOrWhiteSpace(parameters.searchByAuthor))
                postsQuery = postsQuery.Where(p => p.Author.UserName == parameters.searchByAuthor || p.Author.Name == parameters.searchByAuthor);
            if (!string.IsNullOrWhiteSpace(parameters.searchByContent))
                postsQuery = postsQuery.Where(p => p.Content.Contains(parameters.searchByContent));
            if (!string.IsNullOrWhiteSpace(parameters.searchByTags))
                foreach (string tag in parameters.searchByTags.Split(',', '#', ' '))
                    if (!string.IsNullOrWhiteSpace(tag))
                        postsQuery = postsQuery.Where(p => p.Tags.Any(t => t.Name == tag)).Distinct();

            if (parameters.searchByDuring.HasValue)
                postsQuery = postsQuery.Where(p => p.Podcast.Duration >= parameters.searchByDuring.Value * 60 - 60 * 5 && p.Podcast.Duration <= parameters.searchByDuring.Value * 60 + 60 * 5);  // Допуск в +-5 минут
            switch (parameters.sortBy)
            {
                case "DateUp":
                    postsQuery = postsQuery.OrderBy(p => p.PublishedAt);
                    break;
                case "DateDown":
                    postsQuery = postsQuery.OrderByDescending(p => p.PublishedAt);
                    break;
                case "PopUp":
                    postsQuery = postsQuery.OrderBy(p => p.Likes);
                    break;
                case "PopDown":
                    postsQuery = postsQuery.OrderByDescending(p => p.Likes);
                    break;
            }

            var count = await postsQuery.CountAsync(cancellationToken);

            var posts = await postsQuery
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedList<Post>(posts, count, parameters.PageNumber, parameters.PageSize);
        }

        public async Task<Post> GetPostById(int id, CancellationToken cancellationToken)
        {
            return await _context.Posts.Include(p => p.Author).Include(p => p.Podcast).FirstOrDefaultAsync(p => p.PostId == id, cancellationToken);
        }

        public async Task CreatePost(Post post, CancellationToken cancellationToken)
        {
            await _context.Posts.AddAsync(post, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdatePost(Post post, CancellationToken cancellationToken)
        {
            _context.Posts.Update(post);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeletePost(int id, CancellationToken cancellationToken)
        {
            var post = await GetPostById(id, cancellationToken);
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
