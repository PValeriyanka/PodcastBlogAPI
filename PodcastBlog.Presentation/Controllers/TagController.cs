using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.ModelsDto.Tag;
using PodcastBlog.Domain.Parameters;
using System.Text.Json;

namespace PodcastBlog.Presentation.Controllers
{
    [ApiController]
    [Route("api/tags")]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        // GET: api/tags
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IEnumerable<TagDto>>> GetAllTagsPagedAsync([FromQuery] Parameters parameters, CancellationToken cancellationToken)
        {
            var tagsDto = await _tagService.GetAllTagsPagedAsync(parameters, cancellationToken);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(tagsDto.MetaData));

            return Ok(tagsDto);
        }

        // GET: api/tags/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<TagDto>> GetTagByIdAsync(int id, CancellationToken cancellationToken)
        {
            var tagDto = await _tagService.GetTagByIdAsync(id, cancellationToken);

            return Ok(tagDto);
        }

        // POST: api/tags
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<TagDto>> CreateTagAsync([FromBody] CreateTagDto createTagDto, CancellationToken cancellationToken)
        {
            await _tagService.CreateTagAsync(createTagDto, cancellationToken);

            return Ok();
        }

        // DELETE: api/tags/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteTagAsync(int id, CancellationToken cancellationToken)
        {
            await _tagService.DeleteTagAsync(id, cancellationToken);

            return NoContent();
        }
    }
}
