namespace PodcastBlog.Domain.Models
{
    public class Tag
    {
        public int TagId { get; set; }
        public string Name { get; set; }

        public ICollection<Post> Posts { get; set; }
    }
}
