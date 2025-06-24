using PodcastBlog.Domain.Models;

namespace PodcastBlog.Application.ModelsDto
{
    public class PostDto
    {
        public int PostId { get; set; }
        required public string Title { get; set; }
        public string? Content { get; set; }
        public int? AuthorId { get; set; }
        public DateTime? PublishedAt { get; set; }
        public PostStatus? Status { get; set; }
        public int? PodcastId { get; set; }
        public int? Views { get; set; }
        public string? Tags { get; set; }
    }
}
