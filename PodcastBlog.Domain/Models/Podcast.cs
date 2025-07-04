﻿namespace PodcastBlog.Domain.Models
{
    public class Podcast
    {
        public int PodcastId { get; set; }
        required public string Title { get; set; }
        required public string AudioFile { get; set; }
        public string? CoverImage { get; set; }
        public int? Duration { get; set; }
        public int? Bitrate { get; set; }
        public string? Transcript { get; set; }
        public int ListenCount { get; set; } = 0;
    }
}
