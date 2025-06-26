namespace PodcastBlog.Application.ModelsDto.Post
{
    public class UpdatePostDto
    {
        required public int PostId { get; set; }
        required public string Title { get; set; }
        public string? Content { get; set; }
        public DateTime? PublishedAt { get; set; }

        public int? PodcastId { get; set; }
        public string? Tags { get; set; }
    }
}
