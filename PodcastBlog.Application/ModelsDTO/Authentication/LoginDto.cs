namespace PodcastBlog.Application.ModelsDto.Authentication
{
    public class LoginDto
    {
        required public string Email { get; set; }
        required public string Password { get; set; }
    }
}
