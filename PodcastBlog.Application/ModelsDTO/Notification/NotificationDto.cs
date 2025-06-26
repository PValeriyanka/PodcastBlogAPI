namespace PodcastBlog.Application.ModelsDto.Notification
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        required public string Message { get; set; }
        public bool IsRead { get; set; }

        public int UserId { get; set; }
        public string? UserName { get; set; }
    }
}
