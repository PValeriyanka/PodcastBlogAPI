using Microsoft.AspNetCore.Mvc;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.ModelsDto;
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
        public async Task<ActionResult<IEnumerable<TagDto>>> GetAllTagsPagedAsync([FromQuery] Parameters parameters, CancellationToken cancellationToken)
        {
            var tagsDto = await _tagService.GetAllTagsPagedAsync(parameters, cancellationToken);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(tagsDto.MetaData));

            return Ok(tagsDto);
        }

        // GET: api/tags/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TagDto>> GetTagByIdAsync(int id, CancellationToken cancellationToken)
        {
            var tagDto = await _tagService.GetTagByIdAsync(id, cancellationToken);

            return Ok(tagDto);
        }

        // POST: api/tags
        [HttpPost]
        public async Task<ActionResult<TagDto>> CreateTagAsync([FromBody] TagDto tagDto, CancellationToken cancellationToken)
        {
            await _tagService.CreateTagAsync(tagDto, cancellationToken);

            return CreatedAtAction(nameof(GetTagByIdAsync), new { id = tagDto.TagId }, tagDto);
        }

        // PUT: api/tags/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTagAsync([FromBody] TagDto tagDto, CancellationToken cancellationToken)
        {
            await _tagService.UpdateTagAsync(tagDto, cancellationToken);

            return NoContent();
        }

        // DELETE: api/tags/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTagAsync(int id, CancellationToken cancellationToken)
        {
            await _tagService.DeleteTagAsync(id, cancellationToken);

            return NoContent();
        }
    }
}
