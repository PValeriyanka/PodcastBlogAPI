using Microsoft.AspNetCore.Http;

namespace PodcastBlog.Application.ModelsDto.Podcast
{
    public class UpdatePodcastDto
    {
        required public int PodcastId { get; set; }
        public string? Title { get; set; }
        public IFormFile? AudioUpload { get; set; }
        public IFormFile? CoverImageUpload { get; set; }
    }
}
