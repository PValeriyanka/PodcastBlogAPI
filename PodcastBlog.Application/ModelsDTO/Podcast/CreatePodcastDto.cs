using Microsoft.AspNetCore.Http;

namespace PodcastBlog.Application.ModelsDto.Podcast
{
    public class CreatePodcastDto
    {
        required public string Title { get; set; }
        required public IFormFile AudioUpload { get; set; }
        public IFormFile? CoverImageUpload { get; set; }
    }
}
