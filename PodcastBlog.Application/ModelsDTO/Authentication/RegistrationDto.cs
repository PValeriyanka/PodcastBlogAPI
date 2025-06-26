namespace PodcastBlog.Application.ModelsDto.Authentication
{
    public class RegistrationDto
    {
        required public string UserName { get; set; }
        required public string Name { get; set; }
        required public string Email { get; set; }
        required public string Password { get; set; }
        public bool EmailNotify { get; set; } = true;
    }
}
