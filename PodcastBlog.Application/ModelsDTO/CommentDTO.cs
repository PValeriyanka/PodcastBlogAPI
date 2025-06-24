namespace PodcastBlog.Application.ModelsDto
{
    public class CommentDto
    {
        public int CommentId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public int? ParentId { get; set; }
        required public string Content { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
