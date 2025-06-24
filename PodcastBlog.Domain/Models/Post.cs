namespace PodcastBlog.Domain.Models
{
    public class Post
    {
        public int PostId { get; set; }
        required public string Title { get; set; }
        public string? Content { get; set; }
        public int AuthorId { get; set; }
        public DateTime PublishedAt { get; set; }
        public int? PodcastId { get; set; }
        public PostStatus Status { get; set; } 
        public int Views { get; set; } = 0;

        public User Author { get; set; }
        public Podcast? Podcast { get; set; } 

        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<User> Likes { get; set; } = new List<User>();
    }

    public enum PostStatus
    {
        Draft,          // Черновик
        Scheduled,      // Запланировано 
        Published       // Опубликовано 
    }
}
