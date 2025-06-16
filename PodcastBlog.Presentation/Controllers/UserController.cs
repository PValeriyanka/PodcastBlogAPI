using Microsoft.AspNetCore.Mvc;
using PodcastBlog.Application.IServices;
using PodcastBlog.Application.ModelsDTO;

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

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUserById(int id, CancellationToken cancellationToken)
        {
            var userDTO = await _userService.GetUserById(id, cancellationToken);

            return Ok(userDTO);
        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<UserDTO>> CreateUser([FromBody] UserDTO userDTO, CancellationToken cancellationToken)
        {
            await _userService.CreateUser(userDTO, cancellationToken);

            return CreatedAtAction(nameof(GetUserById), new { id = userDTO.UserId }, userDTO);
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser([FromBody] UserDTO userDTO, CancellationToken cancellationToken)
        {
            await _userService.UpdateUser(userDTO, cancellationToken);

            return NoContent();
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
        {
            await _userService.DeleteUser(id, cancellationToken);

            return NoContent();
        }
    }
}
