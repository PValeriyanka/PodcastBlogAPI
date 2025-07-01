using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Application.ModelsDto.User;
using PodcastBlog.Application.Services;
using PodcastBlog.Domain.Interfaces;
using PodcastBlog.Domain.Interfaces.Repositories;
using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;
using PodcastBlog.Infrastructure.ExceptionsHandler.Exceptions;
using PodcastBlog.Tests.TestUtils;
using System.Security.Claims;

namespace PodcastBlog.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<ILogger<UserService>> _loggerMock = new();
        private readonly Mock<INotificationService> _notificationServiceMock = new();
        private readonly Mock<IUserCleanupStrategy> _cleanupMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IPostRepository> _postRepositoryMock = new();
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.Posts).Returns(_postRepositoryMock.Object);

            _userService = new UserService(
                _unitOfWorkMock.Object,
                _notificationServiceMock.Object,
                _cleanupMock.Object,
                _mapperMock.Object,
                _loggerMock.Object
            );
        }

        private ClaimsPrincipal GetClaims(int userId) => new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));

        [Fact]
        public async Task GetAllUsersPagedAsync_Success()
        {
            var user = TestData.User;
            var paged = new PagedList<User>(new List<User> { user }, 1, 1, 10);

            _unitOfWorkMock.Setup(u => u.Users.GetAllUsersPagedAsync(It.IsAny<Parameters>(), It.IsAny<CancellationToken>())).ReturnsAsync(paged);
            _mapperMock.Setup(m => m.Map<IEnumerable<UserDto>>(paged)).Returns(new List<UserDto> { new() { UserId = user.Id } });

            var result = await _userService.GetAllUsersPagedAsync(new Parameters(), CancellationToken.None);

            Assert.Single(result);
            Assert.Equal(user.Id, result.First().UserId);
        }

        [Fact]
        public async Task GetUserByIdAsync_Success()
        {
            var user = TestData.User;

            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _mapperMock.Setup(m => m.Map<UserDto>(user)).Returns(new UserDto { UserId = user.Id });

            var result = await _userService.GetUserByIdAsync(user.Id, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(user.Id, result.UserId);
        }

        [Fact]
        public async Task GetUserByIdAsync_NotFound()
        {
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((User)null!);

            await Assert.ThrowsAsync<NotFoundException>(() => _userService.GetUserByIdAsync(999, CancellationToken.None));
        }

        [Fact]
        public async Task UpdateUserAsync_Success()
        {
            var updateUserDto = new UpdateUserDto { EmailNotify = true };
            var user = TestData.User;

            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            await _userService.UpdateUserAsync(updateUserDto, GetClaims(user.Id), CancellationToken.None);

            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_InvalidUser()
        {
            var updateUserDto = new UpdateUserDto();
            var user = TestData.User;
            var another = TestData.Another;

            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(another.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            await Assert.ThrowsAsync<ForbiddenException>(() => _userService.UpdateUserAsync(updateUserDto, GetClaims(another.Id), CancellationToken.None));
        }

        [Fact]
        public async Task UpdateUserAsync_NotFound()
        {
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((User)null!);

            await Assert.ThrowsAsync<NotFoundException>(() => _userService.UpdateUserAsync(new UpdateUserDto(), GetClaims(999), CancellationToken.None));
        }


        [Fact]
        public async Task UpdateUserRoleAsync_Success()
        {
            var user = TestData.User;
            var updateUserRoleDto = new UpdateUserRoleDto { UserId = user.Id, Role = UserRole.Administrator };

            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(updateUserRoleDto.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            await _userService.UpdateUserRoleAsync(updateUserRoleDto, CancellationToken.None);

            Assert.Equal(UserRole.Administrator, user.Role);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_Success()
        {
            var user = TestData.User;
            var claims = GetClaims(user.Id);

            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            await _userService.DeleteUserAsync(user.Id, claims, CancellationToken.None);

            _cleanupMock.Verify(c => c.CleanupAsync(user, claims, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.Users.DeleteAsync(user, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_InvalidUser()
        {
            var user = TestData.User;
            var another = TestData.Another;

            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(another.Id, It.IsAny<CancellationToken>())).ReturnsAsync(another);

            await Assert.ThrowsAsync<ForbiddenException>(() => _userService.DeleteUserAsync(user.Id, GetClaims(another.Id), CancellationToken.None));
        }

        [Fact]
        public async Task DeleteUserAsync_NotFound()
        {
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((User)null!);

            await Assert.ThrowsAsync<NotFoundException>(() => _userService.DeleteUserAsync(999, GetClaims(999), CancellationToken.None));
        }

        [Fact]
        public async Task SubscriptionAsync_Add_Success()
        {
            var user = TestData.User;
            var author = TestData.Another;
            user.Subscriptions.Clear();
            author.Followers.Clear();

            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(author.Id, It.IsAny<CancellationToken>())).ReturnsAsync(author);

            await _userService.SubscriptionAsync(author.Id, GetClaims(user.Id), CancellationToken.None);

            Assert.Single(user.Subscriptions);
            Assert.Single(author.Followers);
            _notificationServiceMock.Verify(n => n.NewSubscriberNotificationAsync(user.Id, author.Id, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SubscriptionAsync_Remove_Success()
        {
            var user = TestData.User;
            var author = TestData.Another;
            var subscription = new UserSubscription { AuthorId = author.Id, SubscriberId = user.Id };
            user.Subscriptions = new List<UserSubscription> { subscription };
            author.Followers = new List<UserSubscription> { subscription };

            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(author.Id, It.IsAny<CancellationToken>())).ReturnsAsync(author);

            await _userService.SubscriptionAsync(author.Id, GetClaims(user.Id), CancellationToken.None);

            Assert.Empty(user.Subscriptions);
            Assert.Empty(author.Followers);
            _notificationServiceMock.Verify(n => n.NewSubscriberNotificationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task SubscriptionAsync_Failed()
        {
            var user = TestData.User;

            await Assert.ThrowsAsync<ForbiddenException>(() => _userService.SubscriptionAsync(user.Id, GetClaims(user.Id), CancellationToken.None));
        }

        [Fact]
        public async Task PostLikeAsync_Add_Success()
        {
            var user = TestData.User;
            var post = TestData.Post;
            post.Likes.Clear();
            user.Liked.Clear();

            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.Posts.GetByIdAsync(post.PostId, It.IsAny<CancellationToken>())).ReturnsAsync(post);

            await _userService.PostLikeAsync(post.PostId, GetClaims(user.Id), CancellationToken.None);

            Assert.Single(post.Likes);
            Assert.Single(user.Liked);
            _notificationServiceMock.Verify(n => n.NewLikeNotificationAsync(user.Id, post, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task PostLikeAsync_Remove_Success()
        {
            var user = TestData.User;
            var post = TestData.Post;
            post.Likes = new List<User> { user };
            user.Liked = new List<Post> { post };

            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.Posts.GetByIdAsync(post.PostId, It.IsAny<CancellationToken>())).ReturnsAsync(post);

            await _userService.PostLikeAsync(post.PostId, GetClaims(user.Id), CancellationToken.None);

            Assert.Empty(post.Likes);
            Assert.Empty(user.Liked);
            _notificationServiceMock.Verify(n => n.NewLikeNotificationAsync(It.IsAny<int>(), It.IsAny<Post>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task PostLikeAsync_UserNotFound()
        {
            var post = TestData.Post;

            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((User)null!);
            _unitOfWorkMock.Setup(u => u.Posts.GetByIdAsync(post.PostId, It.IsAny<CancellationToken>())).ReturnsAsync(post);

            await Assert.ThrowsAsync<NotFoundException>(() => _userService.PostLikeAsync(post.PostId, GetClaims(999), CancellationToken.None));
        }

        [Fact]
        public async Task PostLikeAsync_PostNotFound()
        {
            var user = TestData.User;

            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.Posts.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Post)null!);

            await Assert.ThrowsAsync<NotFoundException>(() => _userService.PostLikeAsync(999, GetClaims(user.Id), CancellationToken.None));
        }
    }
}
