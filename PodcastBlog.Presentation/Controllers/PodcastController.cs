using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.ModelsDto.Podcast;

namespace PodcastBlog.Presentation.Controllers
{
    [ApiController]
    [Route("api/podcasts")]
    public class PodcastsController : ControllerBase
    {
        private readonly IPodcastService _podcastService;

        public PodcastsController(IPodcastService podcastService)
        {
            _podcastService = podcastService;
        }

        // GET: api/podcasts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PodcastDto>> GetPodcastByIdAsync(int id, CancellationToken cancellationToken)
        {
            var podcastDto = await _podcastService.GetPodcastByIdAsync(id, cancellationToken);

            return Ok(podcastDto);
        }

        // POST: api/podcasts
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> CreatePodcastAsync([FromBody] CreatePodcastDto createPodcastDto, CancellationToken cancellationToken)
        {
            await _podcastService.CreatePodcastAsync(createPodcastDto, cancellationToken);

            return Ok();
        }

        // PUT: api/podcasts/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdatePodcastAsync([FromBody] UpdatePodcastDto updatePodcastDto, CancellationToken cancellationToken)
        {
            await _podcastService.UpdatePodcastAsync(updatePodcastDto, User, cancellationToken);

            return NoContent();
        }

        // DELETE: api/podcasts/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePodcastAsync(int id, CancellationToken cancellationToken)
        {
            await _podcastService.DeletePodcastAsync(id, User, cancellationToken);

            return NoContent();
        }

        // POST: api/podcasts/{id}/listen
        [HttpPost("{id}/listen")]
        public async Task<IActionResult> IncrementListeningAsync(int id, CancellationToken cancellationToken)
        {
            await _podcastService.ListeningAsync(id, cancellationToken);

            return NoContent();
        }

    }
}
