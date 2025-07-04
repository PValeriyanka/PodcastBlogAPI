﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.ModelsDto.Authentication;

namespace PodcastBlog.Presentation.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto, CancellationToken cancellationToken)
        {
            var token = await _authService.AuthenticateAsync(loginDto.Email, loginDto.Password, cancellationToken);

            return Ok(new { token });
        }

        // POST: api/auth/registration
        [HttpPost("registration")]
        public async Task<IActionResult> Register([FromBody] RegistrationDto registrationDto, CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync(registrationDto, cancellationToken);

            return Ok();
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Redirect("/");
        }
    }
}
