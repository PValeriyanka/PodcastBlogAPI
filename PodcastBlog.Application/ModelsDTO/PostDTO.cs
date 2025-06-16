using PodcastBlog.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace PodcastBlog.Application.ModelsDTO
{
    public class PostDTO
    {
        public int PostId { get; set; }
        [Required]
        public string Title { get; set; }
        public string? Content { get; set; }
        public int AuthorId { get; set; }
        public DateTime PublishedAt { get; set; }
        public int? PodcastId { get; set; }
        public PostStatus Status { get; set; }
        public int Views { get; set; }
        public int Likes { get; set; }
    }
}
