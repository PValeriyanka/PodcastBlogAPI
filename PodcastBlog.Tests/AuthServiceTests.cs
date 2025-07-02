using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PodcastBlog.Application.Exceptions;
using PodcastBlog.Application.ModelsDto.Authentication;
using PodcastBlog.Domain.Models;
using PodcastBlog.Infrastructure.Authentication.Options;
using PodcastBlog.Infrastructure.Authentication.Services;
using PodcastBlog.Tests.TestUtils;

namespace PodcastBlog.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly IOptions<JwtOptions> _jwtOptions;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userManagerMock = MockUserManager.Create<User>();
            _loggerMock = new Mock<ILogger<AuthService>>();
            _jwtOptions = Options.Create(new JwtOptions
            {
                Key = "s0m3$up3r$tr0ngL0ngJwtEncryptionKey!",
                Issuer = "TestIssuer",
                Audience = "TestAudience"
            });

            _authService = new AuthService(_userManagerMock.Object, _jwtOptions, _loggerMock.Object);
        }

        [Fact]
        public async Task AuthenticateAsync_Success()
        {
            var user = TestData.User;

            _userManagerMock.Setup(u => u.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.CheckPasswordAsync(user, "password")).ReturnsAsync(true);

            var token = await _authService.AuthenticateAsync(user.Email, "password", CancellationToken.None);

            Assert.NotNull(token);
            Assert.Contains("eyJ", token);
        }

        [Fact]
        public async Task AuthenticateAsync_InvalidEmail()
        {
            _userManagerMock.Setup(u => u.FindByEmailAsync("null@blog.com")).ReturnsAsync((User)null!);

            await Assert.ThrowsAsync<AuthException>(() => _authService.AuthenticateAsync("null@blog.com", "password", CancellationToken.None));
        }

        [Fact]
        public async Task AuthenticateAsync_InvalidPassword()
        {
            var user = TestData.User;

            _userManagerMock.Setup(u => u.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.CheckPasswordAsync(user, "null")).ReturnsAsync(false);

            await Assert.ThrowsAsync<AuthException>(() => _authService.AuthenticateAsync(user.Email, "null", CancellationToken.None));
        }

        [Fact]
        public async Task RegisterAsync_Success()
        {
            var registrationDto = new RegistrationDto
            {
                Email = "test@blog.com",
                UserName = "test",
                Name = "Test",
                Password = "test",
                EmailNotify = true
            };

            _userManagerMock.Setup(u => u.FindByEmailAsync(registrationDto.Email)).ReturnsAsync((User)null!);
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<User>(), registrationDto.Password)).ReturnsAsync(IdentityResult.Success);

            var result = await _authService.RegisterAsync(registrationDto, CancellationToken.None);

            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task RegisterAsync_InvalidData()
        {
            var registrationDto = new RegistrationDto
            {
                Email = TestData.User.Email,
                UserName = "test",
                Name = "Test",
                Password = "test"
            };

            _userManagerMock.Setup(u => u.FindByEmailAsync(registrationDto.Email)).ReturnsAsync(TestData.User);

            await Assert.ThrowsAsync<AuthException>(() => _authService.RegisterAsync(registrationDto, CancellationToken.None));
        }

        [Fact]
        public async Task RegisterAsync_Failed()
        {
            var dto = new RegistrationDto
            {
                Email = "test@blog.com",
                UserName = "test",
                Name = "Test",
                Password = "test",
                EmailNotify = false
            };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync((User)null!);
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<User>(), dto.Password)).ReturnsAsync(IdentityResult.Failed());

            await Assert.ThrowsAsync<AuthException>(() => _authService.RegisterAsync(dto, CancellationToken.None));
        }
    }
}
