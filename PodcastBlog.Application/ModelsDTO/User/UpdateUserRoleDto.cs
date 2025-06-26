using PodcastBlog.Domain.Models;

namespace PodcastBlog.Application.ModelsDto.User
{
    public class UpdateUserRoleDto
    {
        required public int UserId { get; set; }
        required public UserRole Role { get; set; }
    }
}
