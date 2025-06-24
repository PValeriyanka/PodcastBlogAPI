using PodcastBlog.Domain.Models;

namespace PodcastBlog.Application.ModelsDto
{
    public class UserDto
    {
        public int UserId { get; set; }    
        required public string UserName { get; set; }
        required public string Name { get; set; }
        required public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public bool EmailNotify { get; set; }
    }
}
