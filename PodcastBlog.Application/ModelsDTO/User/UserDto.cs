using PodcastBlog.Domain.Models;

namespace PodcastBlog.Application.ModelsDto.User
{
    public class UserDto
    {
        public int UserId { get; set; }
        required public string Name { get; set; }
        required public string Email { get; set; }
        public bool EmailNotify { get; set; }
        public UserRole Role { get; set; }

        public int PostsCount { get; set; }
        public int FollowersCount { get; set; }
        public int SubscriptionsCount { get; set; }
    }
}
