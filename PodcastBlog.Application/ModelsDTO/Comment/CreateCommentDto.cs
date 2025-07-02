namespace PodcastBlog.Application.ModelsDto.Comment
{
    public class CreateCommentDto
    {
        required public string Content { get; set; }
        required public int PostId { get; set; }
        public int? ParentId { get; set; }
    }
}
