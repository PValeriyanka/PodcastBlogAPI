using PodcastBlog.Domain.Models;

namespace PodcastBlog.Application.ModelsDto.Comment
{
    public class CommentDto
    {
        public int CommentId { get; set; }
        required public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public CommentStatus Status { get; set; }

        public int PostId { get; set; }
        public int? ParentId { get; set; }

        public int UserId { get; set; }
        required public string UserName { get; set; }

        public List<CommentDto>? Replies { get; set; }
    }
}
