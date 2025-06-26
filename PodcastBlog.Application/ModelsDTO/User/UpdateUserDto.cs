namespace PodcastBlog.Application.ModelsDto.User
{
    public class UpdateUserDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public bool EmailNotify { get; set; }
    }
}
