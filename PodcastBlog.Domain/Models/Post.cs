namespace PodcastBlog.Domain.Models
{
    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string? Content { get; set; }
        public int AuthorId { get; set; }
        public DateTime PublishedAt { get; set; }
        public int? PodcastId { get; set; }
        public PostStatus Status { get; set; }
        public int Views { get; set; }
        public int Likes { get; set; }     

        public User Author { get; set; }
        public Podcast Podcast { get; set; }

        public ICollection<Tag> Tags { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }

    public enum PostStatus
    {
        Rejected,       // ? Отклонено 
        Draft,          // Черновик
        Scheduled,      // Запланировано 
        Pending,        // ? На модерации
        Published       // Опубликовано 
    }
}
