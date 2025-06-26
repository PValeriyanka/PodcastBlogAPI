namespace PodcastBlog.Application.ModelsDto.Post
{
    public class CreatePostDto
    {
        required public string Title { get; set; }
        required public string Content { get; set; }
        public DateTime? PublishedAt { get; set; }

        public int? PodcastId { get; set; }
        public string? Tags { get; set; }
    }
}
