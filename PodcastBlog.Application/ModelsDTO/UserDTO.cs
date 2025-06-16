using PodcastBlog.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace PodcastBlog.Application.ModelsDTO
{
    public class UserDTO
    {
        public int UserId { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public bool NotifyNewEpisodes { get; set; }
    }
}
