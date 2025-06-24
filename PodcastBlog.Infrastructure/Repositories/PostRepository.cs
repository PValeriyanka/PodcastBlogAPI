using Microsoft.EntityFrameworkCore;
using PodcastBlog.Domain.Interfaces.Repositories;
using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;
using PodcastBlog.Domain.Parameters.ModelParameters;

namespace PodcastBlog.Infrastructure.Repositories
{
    public class PostRepository : Repository<Post>, IPostRepository
    {
        public PostRepository(PodcastBlogContext _context) : base(_context) { }

        public async Task<List<Post>> GetSheduledPostsAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            return await _context.Posts
                .Where(p => p.Status == PostStatus.Scheduled && p.PublishedAt <= now)
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .Include(p => p.Podcast)
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedList<Post>> GetPostsPagedAsync(PostParameters parameters, User? author, string? type, CancellationToken cancellationToken)
        {
            var postsQuery = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Podcast)
                .Include(p => p.Tags)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Replies)
                .AsQueryable();

            switch (type)
            {
                case "Recommends":
                    var subscribedAuthorIds = await _context.UserSubscriptions
                        .Where(s => s.SubscriberId == author.Id)
                        .Select(s => s.AuthorId)
                        .ToListAsync(cancellationToken);

                    postsQuery = postsQuery.Where(p => subscribedAuthorIds.Contains(p.Author.Id) && p.Status == PostStatus.Published);
                    break;
                case "Published":
                    postsQuery = postsQuery.Where(p => p.Author.Id == author.Id && p.Status == PostStatus.Published);
                    break;
                case "Sheduled":
                    postsQuery = postsQuery.Where(p => p.Author.Id == author.Id && p.Status == PostStatus.Scheduled);
                    break;
                case "Draft":
                    postsQuery = postsQuery.Where(p => p.Author.Id == author.Id && p.Status == PostStatus.Draft);
                    break;
                default:
                    postsQuery = postsQuery.Where(p => p.Status == PostStatus.Published);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(parameters.searchByDate) && DateTime.TryParse(parameters.searchByDate, out DateTime date))
            {
                postsQuery = postsQuery.Where(p => p.PublishedAt.Day == date.Day && p.PublishedAt.Month == date.Month && p.PublishedAt.Year == date.Year);
            }

            if (!string.IsNullOrWhiteSpace(parameters.searchByAuthor))
            {
                postsQuery = postsQuery.Where(p => p.Author.UserName == parameters.searchByAuthor || p.Author.Name == parameters.searchByAuthor);
            }

            if (!string.IsNullOrWhiteSpace(parameters.searchByContent))
            {
                postsQuery = postsQuery.Where(p => p.Content.Contains(parameters.searchByContent));
            }

            if (!string.IsNullOrWhiteSpace(parameters.searchByTags))
            {
                foreach (string tag in parameters.searchByTags.Split(',', '#', ' '))
                {
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        postsQuery = postsQuery.Where(p => p.Tags.Any(t => t.Name == tag)).Distinct();
                    }
                }
            }

            if (parameters.searchByDuring.HasValue)
            {
                postsQuery = postsQuery.Where(p => p.Podcast.Duration >= parameters.searchByDuring.Value * 60 - 60 * 5 && p.Podcast.Duration <= parameters.searchByDuring.Value * 60 + 60 * 5);  // Допуск в +-5 минут
            }

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

        public Task<Post?> GetByPodcastIdAsync(int podcastId, CancellationToken cancellationToken)
        {
            return _context.Posts
                .FirstOrDefaultAsync(p => p.PodcastId == podcastId, cancellationToken);
        }

        public override async Task<Post?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Podcast)
                .Include(p => p.Tags)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Replies)
                .FirstOrDefaultAsync(p => p.PostId == id, cancellationToken);
        }
    }
}
