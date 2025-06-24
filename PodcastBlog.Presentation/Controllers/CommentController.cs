using Microsoft.AspNetCore.Mvc;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.ModelsDto;
using PodcastBlog.Domain.Parameters;
using System.Text.Json;

namespace PodcastBlog.Presentation.Controllers
{
    [ApiController]
    [Route("api/comments")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        // GET: api/comments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsByPostPagedAsync(int postId, [FromQuery] Parameters parameters, CancellationToken cancellationToken)
        {
            var commentsDto = await _commentService.GetCommentsByPostPagedAsync(postId, parameters, cancellationToken);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(commentsDto.MetaData));

            return Ok(commentsDto);
        }

        // GET: api/comments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CommentDto>> GetCommentByIdAsync(int id, CancellationToken cancellationToken)
        {
            var commentDto = await _commentService.GetCommentByIdAsync(id, cancellationToken);

            return Ok(commentDto);
        }

        // POST: api/comments
        [HttpPost]
        public async Task<ActionResult<CommentDto>> CreateCommentAsync([FromBody] CommentDto commentDto, int? parentId, CancellationToken cancellationToken)
        {
            await _commentService.CreateCommentAsync(commentDto, parentId, cancellationToken);

            return CreatedAtAction(nameof(GetCommentByIdAsync), new { id = commentDto.CommentId }, commentDto);
        }

        // PUT: api/comments/publish/{id}
        [HttpPut("publish/{id}")]
        public async Task<IActionResult> PublishCommentAsync(int id, CancellationToken cancellationToken)
        {
            await _commentService.PublishCommentAsync(id, cancellationToken);

            return NoContent();
        }

        // DELETE: api/comments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCommentAsync(int id, CancellationToken cancellationToken)
        {
            await _commentService.DeleteCommentAsync(id, cancellationToken);

            return NoContent();
        }
    }
}
