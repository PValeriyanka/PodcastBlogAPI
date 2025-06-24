namespace PodcastBlog.Domain.Models
{
    public class Tag
    {
        public int TagId { get; set; }
        required public string Name { get; set; }

        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
