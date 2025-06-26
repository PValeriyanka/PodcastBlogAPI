using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.ModelsDto;

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
        public async Task<ActionResult<PodcastDto>> CreatePodcastAsync([FromBody] PodcastDto podcastDto, CancellationToken cancellationToken)
        {
            await _podcastService.CreatePodcastAsync(podcastDto, cancellationToken);

            return CreatedAtAction(nameof(GetPodcastByIdAsync), new { id = podcastDto.PodcastId }, podcastDto);
        }

        // PUT: api/podcasts/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdatePodcastAsync([FromBody] PodcastDto podcastDto, CancellationToken cancellationToken)
        {
            await _podcastService.UpdatePodcastAsync(podcastDto, cancellationToken);

            return NoContent();
        }

        // DELETE: api/podcasts/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePodcastAsync(int id, CancellationToken cancellationToken)
        {
            await _podcastService.DeletePodcastAsync(id, cancellationToken);

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
