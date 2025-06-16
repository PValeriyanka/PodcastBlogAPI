using PodcastBlog.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace PodcastBlog.Application.ModelsDTO
{
    public class CommentDTO
    {
        public int CommentId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public int? ParentId { get; set; }
        [Required]
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public CommentStatus Status { get; set; }
    }
}
