using Microsoft.AspNetCore.Mvc;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.ModelsDto;
using PodcastBlog.Domain.Parameters.ModelParameters;
using System.Text.Json;

namespace PodcastBlog.Presentation.Controllers
{
    [ApiController]
    [Route("api/posts")]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IUserService _userService;
        public PostsController(IPostService postService, IUserService userService)
        {
            _postService = postService;
            _userService = userService;
        }

        // GET: api/posts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetPostsPagedAsync([FromQuery] PostParameters parameters, CancellationToken cancellationToken)
        {
            var postsDto = await _postService.GetPostsPagedAsync(parameters, User, null, cancellationToken);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(postsDto.MetaData));

            return Ok(postsDto);
        }


        // GET: api/posts/recommends
        [HttpGet("recommends")]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetRecommendsPostsPagedAsync([FromQuery] PostParameters parameters, CancellationToken cancellationToken)
        {
            var postsDto = await _postService.GetPostsPagedAsync(parameters, User, "Recommends", cancellationToken);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(postsDto.MetaData));

            return Ok(postsDto);
        }

        // GET: api/posts/published
        [HttpGet("published")]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetPublishedPostsPagedAsync([FromQuery] PostParameters parameters, CancellationToken cancellationToken)
        {
            var postsDto = await _postService.GetPostsPagedAsync(parameters, User, "Published", cancellationToken);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(postsDto.MetaData));

            return Ok(postsDto);
        }

        // GET: api/posts/sheduled
        [HttpGet("sheduled")]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetSheduledPostsPagedAsync([FromQuery] PostParameters parameters, CancellationToken cancellationToken)
        {
            var postsDto = await _postService.GetPostsPagedAsync(parameters, User, "Sheduled", cancellationToken);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(postsDto.MetaData));

            return Ok(postsDto);
        }

        // GET: api/posts/draft
        [HttpGet("draft")]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetDraftPostsPagedAsync([FromQuery] PostParameters parameters, CancellationToken cancellationToken)
        {
            var postsDto = await _postService.GetPostsPagedAsync(parameters, User, "Draft", cancellationToken);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(postsDto.MetaData));

            return Ok(postsDto);
        }

        // GET: api/posts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PostDto>> GetPostByIdAsync(int id, CancellationToken cancellationToken)
        {
            var postDto = await _postService.GetPostByIdAsync(id, cancellationToken);

            return Ok(postDto);
        }

        // POST: api/posts/publish
        [HttpPost("publish")]
        public async Task<ActionResult<PostDto>> CreatePublicPostAsync([FromBody] PostDto postDto, CancellationToken cancellationToken)
        {
            await _postService.CreatePostAsync(postDto, User, "Publish", cancellationToken);

            return CreatedAtAction(nameof(GetPostByIdAsync), new { id = postDto.PostId }, postDto);
        }

        // POST: api/posts/draft
        [HttpPost("draft")]
        public async Task<ActionResult<PostDto>> CreateDraftPostAsync([FromBody] PostDto postDto, CancellationToken cancellationToken)
        {
            await _postService.CreatePostAsync(postDto, User, "Draft", cancellationToken);

            return CreatedAtAction(nameof(GetPostByIdAsync), new { id = postDto.PostId }, postDto);
        }

        // PUT: api/posts/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePublicPostAsync([FromBody] PostDto postDto, CancellationToken cancellationToken)
        {
            await _postService.UpdatePostAsync(postDto, User, null, cancellationToken);

            return NoContent();
        }

        // PUT: api/posts/draft/{id}
        [HttpPut("draft/{id}")]
        public async Task<IActionResult> UpdateDraftPostAsync([FromBody] PostDto postDto, CancellationToken cancellationToken)
        {
            await _postService.UpdatePostAsync(postDto, User, "Draft", cancellationToken);

            return NoContent();
        }

        // PUT: api/posts/publish/{id}
        [HttpPut("publish/{id}")]
        public async Task<IActionResult> UpdateAndPublicDraftPostAsync([FromBody] PostDto postDto, CancellationToken cancellationToken)
        {
            await _postService.UpdatePostAsync(postDto, User,"Publish", cancellationToken);

            return NoContent();
        }

        // DELETE: api/posts/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePostAsync(int id, CancellationToken cancellationToken)
        {
            await _postService.DeletePostAsync(id, cancellationToken);

            return NoContent();
        }

        // POST: api/posts/{id}/like
        [HttpPost("{postId}/like")]
        public async Task<IActionResult> PostLikeAsync(int postId, CancellationToken cancellationToken)
        {
            await _userService.PostLikeAsync(User, postId, cancellationToken);

            return Ok();
        }
    }
}
