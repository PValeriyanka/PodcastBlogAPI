using PodcastBlog.Application.ModelsDto.Podcast;
using PodcastBlog.Domain.Models;

namespace PodcastBlog.Application.ModelsDto.Post
{
    public class PostDto
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string? Content { get; set; }
        public DateTime PublishedAt { get; set; }
        public PostStatus Status { get; set; }
        public int Views { get; set; }

        public int AuthorId { get; set; }
        public string AuthorName { get; set; }

        public int? PodcastId { get; set; }
        public PodcastDto? Podcast { get; set; }

        public List<string> Tags { get; set; }
        public int LikesCount { get; set; }
    }
}
