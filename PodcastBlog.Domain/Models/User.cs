using Microsoft.AspNetCore.Identity;

namespace PodcastBlog.Domain.Models
{
    public class User : IdentityUser<int>
    {
        required public string Name { get; set; }
        public bool EmailNotify { get; set; } = true;
        public UserRole Role { get; set; }

        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
        public ICollection<UserSubscription> Followers { get; set; } = new List<UserSubscription>();
        public ICollection<Post> Liked { get; set; } = new List<Post>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }

    public enum UserRole
    {
        Author,
        Administrator
    }
}
