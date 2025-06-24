namespace PodcastBlog.Domain.Models
{
    public class UserSubscription
    {
        public int SubscriberId { get; set; }
        public int AuthorId { get; set; }

        public User? Subscriber { get; set; }
        public User? Author { get; set; } 
    }
}
