using System.ComponentModel.DataAnnotations;

namespace PodcastBlog.Application.ModelsDTO
{
    public class TagDTO
    {
        public int TagId { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
