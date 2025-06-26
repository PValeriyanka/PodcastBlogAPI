namespace PodcastBlog.Domain.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        required public string Message { get; set; }
        public bool IsRead { get; set; } = false;

        public User User { get; set; }
    }
}
