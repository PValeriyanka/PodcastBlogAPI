using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Application.ModelsDto.Notification;
using PodcastBlog.Application.Services;
using PodcastBlog.Domain.Interfaces;
using PodcastBlog.Domain.Interfaces.Repositories;
using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;
using PodcastBlog.Tests.TestUtils;
using System.Security.Claims;

namespace PodcastBlog.Tests
{
    public class NotificationServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IEmailService> _emailServiceMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<INotificationCleanupStrategy> _cleanupMock = new();
        private readonly Mock<ILogger<NotificationService>> _loggerMock = new();
        private readonly NotificationService _notificationService;

        public NotificationServiceTests()
        {
            _unitOfWorkMock.Setup(u => u.Notifications).Returns(new Mock<INotificationRepository>().Object);
            _unitOfWorkMock.Setup(u => u.Users).Returns(new Mock<IUserRepository>().Object);

            _notificationService = new NotificationService(
                _unitOfWorkMock.Object,
                _emailServiceMock.Object,
                _cleanupMock.Object,
                _mapperMock.Object,
                _loggerMock.Object
            );
        }

        private ClaimsPrincipal GetClaims(int id) => new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()) }));

        [Fact]
        public async Task GetNotificationsByUserPagedAsync_Success()
        {
            var parameters = new Parameters();
            var user = TestData.User;
            var notification = TestData.Notification;
            var paged = new PagedList<Notification>(new List<Notification> { notification }, 1, 1, 10);
            var pagedDto = new List<NotificationDto> { new() { Message = notification.Message } };

            _unitOfWorkMock.Setup(u => u.Notifications.GetNotificationsByUserPagedAsync(user.Id, parameters, It.IsAny<CancellationToken>())).ReturnsAsync(paged);
            _mapperMock.Setup(m => m.Map<IEnumerable<NotificationDto>>(paged)).Returns(pagedDto);

            var result = await _notificationService.GetNotificationsByUserPagedAsync(GetClaims(user.Id), parameters, CancellationToken.None);

            Assert.Single(result);
            Assert.Equal(paged.MetaData.TotalCount, result.MetaData.TotalCount);
        }

        [Fact]
        public async Task GetNotificationByIdAsync_Success()
        {
            var notification = TestData.Notification;

            _unitOfWorkMock.Setup(u => u.Notifications.GetByIdAsync(notification.NotificationId, It.IsAny<CancellationToken>())).ReturnsAsync(notification);
            _mapperMock.Setup(m => m.Map<NotificationDto>(notification)).Returns(new NotificationDto { Message = notification.Message });

            var result = await _notificationService.GetNotificationByIdAsync(notification.NotificationId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(notification.Message, result.Message);
        }

        [Fact]
        public async Task GetNotificationByIdAsync_NotFound()
        {
            _unitOfWorkMock.Setup(u => u.Notifications.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Notification)null!);

            var result = await _notificationService.GetNotificationByIdAsync(999, CancellationToken.None);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreatePostNotificationAsync_Success()
        {
            var user = TestData.User;
            var another = TestData.Another;
            user.Followers = new List<UserSubscription> { new() { Subscriber = another } };

            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            await _notificationService.CreatePostNotificationAsync(user.Id, CancellationToken.None);

            _unitOfWorkMock.Verify(u => u.Notifications.CreateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _emailServiceMock.Verify(e => e.SendNotificationEmailAsync(It.IsAny<string>(), "Новое уведомление!", It.IsAny<string>()), Times.Exactly(2));
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task NewLikeNotificationAsync_Success()
        {
            var user = TestData.User;
            var another = TestData.Another;
            var post = TestData.Post;
            post.AuthorId = another.Id;

            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(post.AuthorId, It.IsAny<CancellationToken>())).ReturnsAsync(another);

            await _notificationService.NewLikeNotificationAsync(user.Id, post, CancellationToken.None);

            _unitOfWorkMock.Verify(u => u.Notifications.CreateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
            _emailServiceMock.Verify(e => e.SendNotificationEmailAsync(another.Email, "Новое уведомление!", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task NewCommentNotificationAsync_Success()
        {
            var comment = TestData.Comment;
            var post = TestData.Post;
            var user = TestData.User;
            var another = TestData.Another;
            post.AuthorId = another.Id;

            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(post.AuthorId, It.IsAny<CancellationToken>())).ReturnsAsync(another);

            await _notificationService.NewCommentNotificationAsync(comment, post, CancellationToken.None);

            _unitOfWorkMock.Verify(u => u.Notifications.CreateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
            _emailServiceMock.Verify(e => e.SendNotificationEmailAsync(another.Email, "Новое уведомление!", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task NewSubscriberNotificationAsync_Success()
        {
            var subscriber = TestData.User;
            var author = TestData.Another;

            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(subscriber.Id, It.IsAny<CancellationToken>())).ReturnsAsync(subscriber);
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(author.Id, It.IsAny<CancellationToken>())).ReturnsAsync(author);

            await _notificationService.NewSubscriberNotificationAsync(subscriber.Id, author.Id, CancellationToken.None);

            _unitOfWorkMock.Verify(u => u.Notifications.CreateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
            _emailServiceMock.Verify(e => e.SendNotificationEmailAsync(author.Email, "Новое уведомление!", It.IsAny<string>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ReadNotificationAsync_Success()
        {
            var notification = TestData.Notification;
            var user = TestData.User;

            _unitOfWorkMock.Setup(u => u.Notifications.GetByIdAsync(notification.NotificationId, It.IsAny<CancellationToken>())).ReturnsAsync(notification);

            await _notificationService.ReadNotificationAsync(notification.NotificationId, GetClaims(user.Id), CancellationToken.None);

            Assert.True(notification.IsRead);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ReadNotificationAsync_InvalidUser()
        {
            var notification = TestData.Notification;
            var another = TestData.Another;

            _unitOfWorkMock.Setup(u => u.Notifications.GetByIdAsync(notification.NotificationId, It.IsAny<CancellationToken>())).ReturnsAsync(notification);

            await _notificationService.ReadNotificationAsync(notification.NotificationId, GetClaims(another.Id), CancellationToken.None);

            Assert.False(notification.IsRead);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ReadNotificationAsync_NotFound()
        {
            var user = TestData.User;

            _unitOfWorkMock.Setup(u => u.Notifications.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Notification)null!);

            await _notificationService.ReadNotificationAsync(999, GetClaims(user.Id), CancellationToken.None);

            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteNotificationAsync_Success()
        {
            var notification = TestData.Notification;
            var user = TestData.User;

            _unitOfWorkMock.Setup(u => u.Notifications.GetByIdAsync(notification.NotificationId, It.IsAny<CancellationToken>())).ReturnsAsync(notification);
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            await _notificationService.DeleteNotificationAsync(notification.NotificationId, GetClaims(user.Id), CancellationToken.None);

            _cleanupMock.Verify(c => c.CleanupAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.Notifications.DeleteAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteNotificationAsync_InvalidUser()
        {
            var notification = TestData.Notification;
            var another = TestData.Another;

            _unitOfWorkMock.Setup(u => u.Notifications.GetByIdAsync(notification.NotificationId, It.IsAny<CancellationToken>())).ReturnsAsync(notification);
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(another.Id, It.IsAny<CancellationToken>())).ReturnsAsync(another);

            await _notificationService.DeleteNotificationAsync(notification.NotificationId, GetClaims(another.Id), CancellationToken.None);

            _cleanupMock.Verify(c => c.CleanupAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.Notifications.DeleteAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteNotificationAsync_NotFound()
        {
            var user = TestData.User;

            _unitOfWorkMock.Setup(u => u.Notifications.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Notification)null!);

            await _notificationService.DeleteNotificationAsync(999, GetClaims(user.Id), CancellationToken.None);

            _cleanupMock.Verify(c => c.CleanupAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.Notifications.DeleteAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
