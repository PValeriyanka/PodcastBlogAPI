using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Application.ModelsDto.Comment;
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
    public class CommentServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<INotificationService> _notificationServiceMock = new();
        private readonly Mock<ICommentCleanupStrategy> _cleanupMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<ILogger<CommentService>> _loggerMock = new();
        private readonly CommentService _commentService;
        private readonly Mock<ICommentRepository> _commentRepositoryMock = new();

        public CommentServiceTests()
        {
            _unitOfWorkMock.Setup(u => u.Comments).Returns(_commentRepositoryMock.Object);

            _commentService = new CommentService(
                _unitOfWorkMock.Object,
                _notificationServiceMock.Object,
                _cleanupMock.Object,
                _mapperMock.Object,
                _loggerMock.Object
            );
        }

        private ClaimsPrincipal GetClaims(int id) => new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()) }));

        [Fact]
        public async Task GetCommentsByPostPagedAsync_Success()
        {
            var parameters = new Parameters();
            var postId = TestData.Post.PostId;
            var comment = TestData.Comment;
            var comments = new List<Comment> { comment };
            var paged = new PagedList<Comment>(comments, 1, 1, 10);
            var commentsDto = new List<CommentDto>
            {
                new CommentDto { CommentId = comment.CommentId, Content = comment.Content, UserName = comment.User.UserName }
            };

            _unitOfWorkMock.Setup(u => u.Comments.GetCommentsByPostPagedAsync(postId, parameters, It.IsAny<CancellationToken>())).ReturnsAsync(paged);
            _mapperMock.Setup(m => m.Map<IEnumerable<CommentDto>>(paged)).Returns(commentsDto);

            var result = await _commentService.GetCommentsByPostPagedAsync(postId, parameters, CancellationToken.None);

            Assert.Single(result);
            Assert.Equal(comment.Content, result.First().Content);
            Assert.Equal(paged.MetaData.TotalCount, result.MetaData.TotalCount);
        }

        [Fact]
        public async Task GetCommentsByPostPagedAsync_Empty()
        {
            var parameters = new Parameters();
            var postId = TestData.Post.PostId;
            var paged = new PagedList<Comment>(new List<Comment>(), 0, 1, 10);
            var commentsDto = new List<CommentDto>();

            _unitOfWorkMock.Setup(u => u.Comments.GetCommentsByPostPagedAsync(postId, parameters, It.IsAny<CancellationToken>())).ReturnsAsync(paged);
            _mapperMock.Setup(m => m.Map<IEnumerable<CommentDto>>(paged)).Returns(commentsDto);

            var result = await _commentService.GetCommentsByPostPagedAsync(postId, parameters, CancellationToken.None);

            Assert.Empty(result);
            Assert.Equal(0, result.MetaData.TotalCount);
        }

        [Fact]
        public async Task GetCommentByIdAsync_Success()
        {
            var comment = TestData.Comment;

            _unitOfWorkMock.Setup(u => u.Comments.GetByIdAsync(comment.CommentId, It.IsAny<CancellationToken>())).ReturnsAsync(comment);
            _mapperMock.Setup(m => m.Map<CommentDto>(comment)).Returns(new CommentDto { Content = comment.Content, UserName = comment.User.UserName });

            var result = await _commentService.GetCommentByIdAsync(comment.CommentId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(comment.Content, result.Content);
        }

        [Fact]
        public async Task GetCommentByIdAsync_NotFound()
        {
            _unitOfWorkMock.Setup(u => u.Comments.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Comment)null!);

            await Assert.ThrowsAsync<NotFoundException>(() => _commentService.GetCommentByIdAsync(999, CancellationToken.None));
        }

        [Fact]
        public async Task CreateCommentAsync_Success()
        {
            var post = TestData.Post;
            var user = TestData.User;
            var createCommentDto = new CreateCommentDto
            {
                Content = "New comment",
                PostId = post.PostId
            };
            var comment = new Comment { Content = createCommentDto.Content, PostId = createCommentDto.PostId };

            _mapperMock.Setup(m => m.Map<Comment>(createCommentDto)).Returns(comment);
            _unitOfWorkMock.Setup(u => u.Posts.GetByIdAsync(createCommentDto.PostId, It.IsAny<CancellationToken>())).ReturnsAsync(post);
            _notificationServiceMock.Setup(n => n.NewCommentNotificationAsync(It.IsAny<Comment>(), It.IsAny<Post>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await _commentService.CreateCommentAsync(createCommentDto, GetClaims(user.Id), CancellationToken.None);

            _unitOfWorkMock.Verify(u => u.Comments.CreateAsync(comment, It.IsAny<CancellationToken>()), Times.Once);
            _notificationServiceMock.Verify(n => n.NewCommentNotificationAsync(It.IsAny<Comment>(), It.IsAny<Post>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateCommentAsync_NotFound()
        {
            var user = TestData.User;
            var createCommentDto = new CreateCommentDto
            {
                Content = "New comment",
                PostId = 999
            };
            var comment = new Comment { Content = createCommentDto.Content, PostId = 999 };

            _mapperMock.Setup(m => m.Map<Comment>(createCommentDto)).Returns(comment);
            _unitOfWorkMock.Setup(u => u.Posts.GetByIdAsync(createCommentDto.PostId, It.IsAny<CancellationToken>())).ReturnsAsync((Post)null!);

            await Assert.ThrowsAsync<NotFoundException>(() => _commentService.CreateCommentAsync(createCommentDto, GetClaims(user.Id), CancellationToken.None));
        }

        [Fact]
        public async Task PublishCommentAsync_Success()
        {
            var comment = TestData.Comment;
            var user = TestData.User;

            _unitOfWorkMock.Setup(u => u.Comments.GetByIdAsync(comment.CommentId, It.IsAny<CancellationToken>())).ReturnsAsync(comment);

            await _commentService.PublishCommentAsync(comment.CommentId, GetClaims(user.Id), CancellationToken.None);

            Assert.Equal(CommentStatus.Approved, comment.Status);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task PublishCommentAsync_InvalidAuthor()
        {
            var comment = TestData.Comment;
            var another = TestData.Another;

            _unitOfWorkMock.Setup(u => u.Comments.GetByIdAsync(comment.CommentId, It.IsAny<CancellationToken>())).ReturnsAsync(comment);

            await Assert.ThrowsAsync<ForbiddenException>(() => _commentService.PublishCommentAsync(comment.CommentId, GetClaims(another.Id), CancellationToken.None));
        }

        [Fact]
        public async Task PublishCommentAsync_NotFound()
        {
            var user = TestData.User;

            _unitOfWorkMock.Setup(u => u.Comments.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Comment)null!);

            await Assert.ThrowsAsync<NotFoundException>(() => _commentService.PublishCommentAsync(999, GetClaims(user.Id), CancellationToken.None));
        }

        [Fact]
        public async Task DeleteCommentAsync_Success()
        {
            var comment = TestData.Comment;
            var user = TestData.User;

            _unitOfWorkMock.Setup(u => u.Comments.GetByIdAsync(comment.CommentId, It.IsAny<CancellationToken>())).ReturnsAsync(comment);
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            await _commentService.DeleteCommentAsync(comment.CommentId, GetClaims(user.Id), CancellationToken.None);

            _cleanupMock.Verify(c => c.CleanupAsync(comment, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.Comments.DeleteAsync(comment, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteCommentAsync_InvalidUser()
        {
            var comment = TestData.Comment;
            var user = TestData.User;
            var another = TestData.Another;

            _unitOfWorkMock.Setup(u => u.Comments.GetByIdAsync(comment.CommentId, It.IsAny<CancellationToken>())).ReturnsAsync(comment);
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            await Assert.ThrowsAsync<ForbiddenException>(() => _commentService.DeleteCommentAsync(comment.CommentId, GetClaims(another.Id), CancellationToken.None));
        }

        [Fact]
        public async Task DeleteCommentAsync_NotFound()
        {
            var user = TestData.User;

            _unitOfWorkMock.Setup(u => u.Comments.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Comment)null!);

            await Assert.ThrowsAsync<NotFoundException>(() => _commentService.DeleteCommentAsync(999, GetClaims(user.Id), CancellationToken.None));
        }
    }
}
