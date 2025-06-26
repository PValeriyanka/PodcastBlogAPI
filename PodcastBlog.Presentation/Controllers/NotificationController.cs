using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.ModelsDto;
using PodcastBlog.Domain.Parameters;
using System.Text.Json;

namespace PodcastBlog.Presentation.Controllers
{
    [ApiController]
    [Route("api/notification")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET: api/notifications
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotificationsByPostPagedAsync([FromQuery] Parameters parameters, CancellationToken cancellationToken)
        {
            var notificationsDto = await _notificationService.GetNotificationsByUserPagedAsync(User, parameters, cancellationToken);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(notificationsDto.MetaData));

            return Ok(notificationsDto);
        }

        // PATCH: api/notifications/{id}
        [HttpPatch("{id}")]
        [Authorize]
        public async Task<ActionResult<NotificationDto>> ReadNotificationAsync(int id, CancellationToken cancellationToken)
        {
            await _notificationService.ReadNotificationAsync(id, cancellationToken);

            return Ok();
        }

        // DELETE: api/notifications/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteNotificationAsync(int id, CancellationToken cancellationToken)
        {
            await _notificationService.DeleteNotificationAsync(id, cancellationToken);

            return NoContent();
        }
    }
}
