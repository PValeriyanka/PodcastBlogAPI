using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.ModelsDto;
using PodcastBlog.Domain.Parameters;
using System.Text.Json;

namespace PodcastBlog.Presentation.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/users
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsersPagedAsync([FromQuery] Parameters parameters, CancellationToken cancellationToken)
        {
            var usersDto = await _userService.GetAllUsersPagedAsync(parameters, cancellationToken);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(usersDto.MetaData));

            return Ok(usersDto);
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<UserDto>> GetUserByIdAsync(int id, CancellationToken cancellationToken)
        {
            var userDto = await _userService.GetUserByIdAsync(id, cancellationToken);

            return Ok(userDto);
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteUserAsync(int id, CancellationToken cancellationToken)
        {
            await _userService.DeleteUserAsync(id, cancellationToken);

            return NoContent();
        }

        // POST: api/users/{id}/subscription
        [HttpPost("{authorId}/subscription")]
        [Authorize]
        public async Task<IActionResult> SubscriptionAsync(int authorId, CancellationToken cancellationToken)
        {
            await _userService.SubscriptionAsync(User, authorId, cancellationToken);

            return Ok();
        }
    }
}
