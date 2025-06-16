namespace PodcastBlog.Domain.Models
{
    public class Podcast
    {
        public int PodcastId { get; set; }
        public string Title { get; set; }
        public string AudioFile { get; set; }
        public string? Transcript { get; set; }
        public string? CoverImage { get; set; }
        public int Duration { get; set; }
        public int EpisodeNumber { get; set; }
        public int ListenCount { get; set; }
    }
}
