using Microsoft.AspNetCore.Mvc;
using PodcastBlog.Application.IServices;
using PodcastBlog.Domain.Parameters.ModelParameters;
using PodcastBlog.Application.ModelsDTO;
using System.Text.Json;

namespace PodcastBlog.Presentation.Controllers
{
    [ApiController]
    [Route("api/posts")]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        // GET: api/posts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostDTO>>> GetAllPostsPaged([FromQuery] PostParameters parameters, CancellationToken cancellationToken)
        {
            var postsDTO = await _postService.GetAllPostsPaged(parameters, cancellationToken);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(postsDTO.MetaData));

            return Ok(postsDTO);
        }

        // GET: api/posts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PostDTO>> GetPostById(int id, CancellationToken cancellationToken)
        {
            var postDTO = await _postService.GetPostById(id, cancellationToken);

            return Ok(postDTO);
        }

        // POST: api/posts
        [HttpPost]
        public async Task<ActionResult<PostDTO>> CreatePost([FromBody] PostDTO postDTO, CancellationToken cancellationToken)
        {
            await _postService.CreatePost(postDTO, cancellationToken);

            return CreatedAtAction(nameof(GetPostById), new { id = postDTO.PostId }, postDTO);
        }

        // PUT: api/posts/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost([FromBody] PostDTO postDTO, CancellationToken cancellationToken)
        {
            await _postService.UpdatePost(postDTO, cancellationToken);

            return NoContent();
        }

        // DELETE: api/posts/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id, CancellationToken cancellationToken)
        {
            await _postService.DeletePost(id, cancellationToken);

            return NoContent();
        }
    }
}
