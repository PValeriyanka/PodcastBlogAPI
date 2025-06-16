using Microsoft.AspNetCore.Identity;

namespace PodcastBlog.Domain.Models
{
    public class User : IdentityUser<int>
    {
        public UserRole Role { get; set; }
        public string Name { get; set; }
        public bool NotifyNewEpisodes { get; set; }

        public ICollection<Post> Posts { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<UserSubscription> Subscriptions { get; set; }
        public ICollection<UserSubscription> Followers { get; set; }
        public ICollection<Post> Likes { get; set; }
        public ICollection<Post> Views { get; set; }
    }

    public enum UserRole
    {
        Author,
        Administrator
    }
}
