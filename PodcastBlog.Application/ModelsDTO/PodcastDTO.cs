using Microsoft.AspNetCore.Http;

namespace PodcastBlog.Application.ModelsDto
{
    public class PodcastDto
    {
        public int PodcastId { get; set; }
        required public string Title { get; set; }
        required public IFormFile? AudioUpload { get; set; }
        public IFormFile? CoverImageUpload { get; set; }
        public string? Transcript { get; set; }
        public int? ListenCount { get; set; }
    }
}
