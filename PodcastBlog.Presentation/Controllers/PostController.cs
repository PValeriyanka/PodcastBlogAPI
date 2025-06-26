using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.ModelsDto.Post;
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
        [Authorize]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetRecommendsPostsPagedAsync([FromQuery] PostParameters parameters, CancellationToken cancellationToken)
        {
            var postsDto = await _postService.GetPostsPagedAsync(parameters, User, "Recommends", cancellationToken);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(postsDto.MetaData));

            return Ok(postsDto);
        }

        // GET: api/posts/published
        [HttpGet("published")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetPublishedPostsPagedAsync([FromQuery] PostParameters parameters, CancellationToken cancellationToken)
        {
            var postsDto = await _postService.GetPostsPagedAsync(parameters, User, "Published", cancellationToken);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(postsDto.MetaData));

            return Ok(postsDto);
        }

        // GET: api/posts/sheduled
        [HttpGet("sheduled")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetSheduledPostsPagedAsync([FromQuery] PostParameters parameters, CancellationToken cancellationToken)
        {
            var postsDto = await _postService.GetPostsPagedAsync(parameters, User, "Sheduled", cancellationToken);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(postsDto.MetaData));

            return Ok(postsDto);
        }

        // GET: api/posts/draft
        [HttpGet("draft")]
        [Authorize]
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
        [Authorize]
        public async Task<ActionResult<PostDto>> CreatePublicPostAsync([FromBody] CreatePostDto createPostDto, CancellationToken cancellationToken)
        {
            await _postService.CreatePostAsync(createPostDto, User, "Publish", cancellationToken);

            return Ok();
        }

        // POST: api/posts/draft
        [HttpPost("draft")]
        [Authorize]
        public async Task<ActionResult<PostDto>> CreateDraftPostAsync([FromBody] CreatePostDto createPostDto, CancellationToken cancellationToken)
        {
            await _postService.CreatePostAsync(createPostDto, User, "Draft", cancellationToken);

            return Ok();
        }

        // PUT: api/posts/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdatePublicPostAsync([FromBody] UpdatePostDto updatePostDto, CancellationToken cancellationToken)
        {
            await _postService.UpdatePostAsync(updatePostDto, User, null, cancellationToken);

            return NoContent();
        }

        // PUT: api/posts/draft/{id}
        [HttpPut("draft/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateDraftPostAsync([FromBody] UpdatePostDto updatePostDto, CancellationToken cancellationToken)
        {
            await _postService.UpdatePostAsync(updatePostDto, User, "Draft", cancellationToken);

            return NoContent();
        }

        // PUT: api/posts/publish/{id}
        [HttpPut("publish/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateAndPublicDraftPostAsync([FromBody] UpdatePostDto updatePostDto, CancellationToken cancellationToken)
        {
            await _postService.UpdatePostAsync(updatePostDto, User, "Publish", cancellationToken);

            return NoContent();
        }

        // DELETE: api/posts/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePostAsync(int id, CancellationToken cancellationToken)
        {
            await _postService.DeletePostAsync(id, User, cancellationToken);

            return NoContent();
        }

        // POST: api/posts/{id}/like
        [HttpPost("{postId}/like")]
        [Authorize]
        public async Task<IActionResult> PostLikeAsync(int postId, CancellationToken cancellationToken)
        {
            await _userService.PostLikeAsync(postId, User, cancellationToken);

            return Ok();
        }
    }
}
