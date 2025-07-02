namespace PodcastBlog.Domain.Parameters.ModelParameters
{
    public class PostParameters : Parameters
    {
        public string? searchByDate { get; set; }
        public string? searchByAuthor { get; set; }
        public string? searchByContent { get; set; }
        public string? searchByTags { get; set; }
        public int? searchByDuring { get; set; }

        public string sortBy { get; set; } = "DateDown";
    }
}
