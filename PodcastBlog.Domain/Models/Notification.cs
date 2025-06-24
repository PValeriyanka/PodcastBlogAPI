namespace PodcastBlog.Domain.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        required public int UserId { get; set; }
        required public string Message { get; set; }
        public bool IsRead { get; set; } = false;

        public User User { get; set; }
    }
}
