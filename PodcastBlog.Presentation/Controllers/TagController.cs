using Microsoft.AspNetCore.Mvc;
using PodcastBlog.Application.IServices;
using PodcastBlog.Application.ModelsDTO;

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

        // GET: api/tags/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TagDTO>> GetTagById(int id, CancellationToken cancellationToken)
        {
            var tagDTO = await _tagService.GetTagById(id, cancellationToken);

            return Ok(tagDTO);
        }

        // POST: api/tags
        [HttpPost]
        public async Task<ActionResult<TagDTO>> CreateTag([FromBody] TagDTO tagDTO, CancellationToken cancellationToken)
        {
            await _tagService.CreateTag(tagDTO, cancellationToken);

            return CreatedAtAction(nameof(GetTagById), new { id = tagDTO.TagId }, tagDTO);
        }

        // PUT: api/tags/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTag([FromBody] TagDTO tagDTO, CancellationToken cancellationToken)
        {
            await _tagService.UpdateTag(tagDTO, cancellationToken);

            return NoContent();
        }

        // DELETE: api/tags/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag(int id, CancellationToken cancellationToken)
        {
            await _tagService.DeleteTag(id, cancellationToken);

            return NoContent();
        }
    }
}
