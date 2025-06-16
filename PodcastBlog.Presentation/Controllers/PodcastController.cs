using Microsoft.AspNetCore.Mvc;
using PodcastBlog.Application.IServices;
using PodcastBlog.Application.ModelsDTO;

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
        public async Task<ActionResult<PodcastDTO>> GetPodcastById(int id, CancellationToken cancellationToken)
        {
            var podcastDTO = await _podcastService.GetPodcastById(id, cancellationToken);

            return Ok(podcastDTO);
        }

        // POST: api/podcasts
        [HttpPost]
        public async Task<ActionResult<PodcastDTO>> CreatePodcast([FromBody] PodcastDTO podcastDTO, CancellationToken cancellationToken)
        {
            await _podcastService.CreatePodcast(podcastDTO, cancellationToken);

            return CreatedAtAction(nameof(GetPodcastById), new { id = podcastDTO.PodcastId }, podcastDTO);
        }

        // PUT: api/podcasts/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePodcast([FromBody] PodcastDTO podcastDTO, CancellationToken cancellationToken)
        {
            await _podcastService.UpdatePodcast(podcastDTO, cancellationToken);

            return NoContent();
        }

        // DELETE: api/podcasts/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePodcast(int id, CancellationToken cancellationToken)
        {
            await _podcastService.DeletePodcast(id, cancellationToken);

            return NoContent();
        }
    }
}
