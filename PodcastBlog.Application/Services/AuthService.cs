using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.ModelsDto.Authentication;
using PodcastBlog.Domain.Models;
using PodcastBlog.Infrastructure.Authentication;
using PodcastBlog.Infrastructure.ExceptionsHandler.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PodcastBlog.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly JwtOptions _jwtOptions;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserManager<User> userManager, IOptions<JwtOptions> jwtOptions, ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _jwtOptions = jwtOptions.Value;
            _logger = logger;
        }

        public async Task<string> AuthenticateAsync(string email, string password, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null || !await _userManager.CheckPasswordAsync(user, password))
            {
                _logger.LogWarning("Неудачная попытка входа");

                throw new AuthException("Неверный логин или пароль");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: creds
            );

            _logger.LogInformation("Вход выполнен успешно");

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<IdentityResult> RegisterAsync(RegistrationDto registerDto, CancellationToken cancellationToken)
        {
            var existing = await _userManager.FindByEmailAsync(registerDto.Email);

            if (existing is not null)
            {
                _logger.LogWarning("Регистрация отклонена");

                throw new AuthException("Пользователь с таким email уже существует");
            }

            var user = new User
            {
                UserName = registerDto.UserName,
                Name = registerDto.Name,
                Email = registerDto.Email,
                Role = UserRole.Author,
                EmailNotify = registerDto.EmailNotify
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Ошибка регистрации");

                throw new AuthException("Регистрация не удалась");
            }

            _logger.LogInformation("Регистрация выполнена успешно");

            return result;
        }
    }
}
