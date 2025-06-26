namespace PodcastBlog.Application.ModelsDto.Authentication
{
    public class RegistrationDto
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool EmailNotify { get; set; } = true;
    }
}
