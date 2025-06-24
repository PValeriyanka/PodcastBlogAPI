namespace PodcastBlog.Application.ModelsDto
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        required public int UserId { get; set; }
        required public string Message { get; set; }
        public bool IsRead { get; set; }
    }
}
