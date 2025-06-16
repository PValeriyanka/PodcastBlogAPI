using Microsoft.AspNetCore.Mvc;
using PodcastBlog.Application.IServices;
using PodcastBlog.Application.ModelsDTO;
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
        public async Task<ActionResult<IEnumerable<PostDTO>>> GetCommentsByPostPaged(int postId, [FromQuery] Parameters parameters, CancellationToken cancellationToken)
        {
            var commentsDTO = await _commentService.GetCommentsByPostPaged(postId, parameters, cancellationToken);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(commentsDTO.MetaData));

            return Ok(commentsDTO);
        }

        // GET: api/comments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CommentDTO>> GetCommentById(int id, CancellationToken cancellationToken)
        {
            var commentDTO = await _commentService.GetCommentById(id, cancellationToken);

            return Ok(commentDTO);
        }

        // POST: api/comments
        [HttpPost]
        public async Task<ActionResult<CommentDTO>> CreateComment([FromBody] CommentDTO commentDTO, CancellationToken cancellationToken)
        {
            await _commentService.CreateComment(commentDTO, cancellationToken);

            return CreatedAtAction(nameof(GetCommentById), new { id = commentDTO.CommentId }, commentDTO);
        }

        // PUT: api/comments/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment([FromBody] CommentDTO commentDTO, CancellationToken cancellationToken)
        {
            await _commentService.UpdateComment(commentDTO, cancellationToken);

            return NoContent();
        }

        // DELETE: api/comments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id, CancellationToken cancellationToken)
        {
            await _commentService.DeleteComment(id, cancellationToken);

            return NoContent();
        }
    }
}
