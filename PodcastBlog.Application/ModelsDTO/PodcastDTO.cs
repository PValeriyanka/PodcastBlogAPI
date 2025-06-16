using System.ComponentModel.DataAnnotations;

namespace PodcastBlog.Application.ModelsDTO
{
    public class PodcastDTO
    {
        public int PodcastId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string AudioFile { get; set; }
        public string? Transcript { get; set; }
        public string? CoverImage { get; set; }
        public int Duration { get; set; }
        public int EpisodeNumber { get; set; }
        public int ListenCount { get; set; }
    }
}
