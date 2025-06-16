namespace PodcastBlog.Domain.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public int? ParentId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public CommentStatus Status { get; set; }

        public Post Post { get; set; }
        public User User { get; set; }
        public Comment? Parent { get; set; }

        public ICollection<Comment>? Replies { get; set; }
    }

    public enum CommentStatus
    {
        Pending,        // На модерации 
        Approved,       // Одобрено 
        Rejected        // Отклонено 
    }
}
