using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.ModelsDto.Comment;
using PodcastBlog.Domain.Parameters;
using System.Text.Json;

namespace PodcastBlog.Presentation.Controllers
{
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
        [Authorize]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsByPostPagedAsync(int postId, [FromQuery] Parameters parameters, CancellationToken cancellationToken)
        {
            var commentsDto = await _commentService.GetCommentsByPostPagedAsync(postId, parameters, cancellationToken);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(commentsDto.MetaData));

            return Ok(commentsDto);
        }

        // GET: api/comments/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<CommentDto>> GetCommentByIdAsync(int id, CancellationToken cancellationToken)
        {
            var commentDto = await _commentService.GetCommentByIdAsync(id, cancellationToken);

            return Ok(commentDto);
        }

        // POST: api/comments
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> CreateCommentAsync([FromBody] CreateCommentDto createCommentDto, CancellationToken cancellationToken)
        {
            await _commentService.CreateCommentAsync(createCommentDto, User, cancellationToken);

            return Ok();
        }

        // PUT: api/comments/publish/{id}
        [HttpPut("publish/{id}")]
        [Authorize]
        public async Task<IActionResult> PublishCommentAsync(int id, CancellationToken cancellationToken)
        {
            await _commentService.PublishCommentAsync(id, User, cancellationToken);

            return NoContent();
        }

        // DELETE: api/comments/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCommentAsync(int id, CancellationToken cancellationToken)
        {
            await _commentService.DeleteCommentAsync(id, User, cancellationToken);

            return NoContent();
        }
    }
}
