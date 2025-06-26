using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.ModelsDto.User;
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
        [Authorize]
        public async Task<ActionResult<UserDto>> GetUserByIdAsync(int id, CancellationToken cancellationToken)
        {
            var userDto = await _userService.GetUserByIdAsync(id, cancellationToken);

            return Ok(userDto);
        }

        // PUT : api/users/profile
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateUserAsync([FromBody] UpdateUserDto updateUserDto, CancellationToken cancellationToken)
        {
            await _userService.UpdateUserAsync(updateUserDto, User, cancellationToken);

            return NoContent();
        }

        // PUT : api/users/role
        [HttpPut("role")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateUserRoleAsync([FromBody] UpdateUserRoleDto updateUserRoleDto, CancellationToken cancellationToken)
        {
            await _userService.UpdateUserRoleAsync(updateUserRoleDto, cancellationToken);

            return NoContent();
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUserAsync(int id, CancellationToken cancellationToken)
        {
            await _userService.DeleteUserAsync(id, User, cancellationToken);

            return NoContent();
        }

        // POST: api/users/{id}/subscription
        [HttpPost("{authorId}/subscription")]
        [Authorize]
        public async Task<IActionResult> SubscriptionAsync(int authorId, CancellationToken cancellationToken)
        {
            await _userService.SubscriptionAsync(authorId, User, cancellationToken);

            return Ok();
        }
    }
}
