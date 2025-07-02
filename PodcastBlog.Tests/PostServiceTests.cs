using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using PodcastBlog.Application.Exceptions;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Application.ModelsDto.Post;
using PodcastBlog.Application.Services;
using PodcastBlog.Domain.Interfaces;
using PodcastBlog.Domain.Interfaces.Repositories;
using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;
using PodcastBlog.Domain.Parameters.ModelParameters;
using PodcastBlog.Tests.TestUtils;
using System.Security.Claims;

namespace PodcastBlog.Tests
{
    public class PostServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<ITagService> _tagServiceMock = new();
        private readonly Mock<INotificationService> _notificationServiceMock = new();
        private readonly Mock<IPostCleanupStrategy> _cleanupMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<ILogger<PostService>> _loggerMock = new();
        private readonly PostService _postService;
        private readonly Mock<IPostRepository> _postRepositoryMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();

        public PostServiceTests()
        {
            _unitOfWorkMock.Setup(u => u.Posts).Returns(_postRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);

            _postService = new PostService(
                _unitOfWorkMock.Object,
                _tagServiceMock.Object,
                _notificationServiceMock.Object,
                _cleanupMock.Object,
                _mapperMock.Object,
                _loggerMock.Object
            );
        }

        private ClaimsPrincipal GetClaims(int userId) => new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));

        [Fact]
        public async Task GetPostsPagedAsync_Success()
        {
            var parameters = new PostParameters();
            var user = TestData.User;
            var scheduledPost = TestData.SheduledPost;
            var publishedPost = TestData.Post;
            var scheduledPosts = new List<Post> { scheduledPost };
            var paged = new PagedList<Post>(new List<Post> { publishedPost }, 1, 1, 10);
            var pagedDtos = new List<PostDto> { new() { PostId = publishedPost.PostId, Title = publishedPost.Title, Content = publishedPost.Content, AuthorName = publishedPost.Author.Name } };

            _unitOfWorkMock.Setup(u => u.Posts.GetSheduledPostsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(scheduledPosts);
            _notificationServiceMock.Setup(n => n.CreatePostNotificationAsync(scheduledPost.AuthorId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.Posts.GetPostsPagedAsync(parameters, user, null, It.IsAny<CancellationToken>())).ReturnsAsync(paged);
            _mapperMock.Setup(m => m.Map<IEnumerable<PostDto>>(paged)).Returns(pagedDtos);

            var result = await _postService.GetPostsPagedAsync(parameters, GetClaims(user.Id), null, CancellationToken.None);

            Assert.Single(result);
            Assert.Equal(publishedPost.Title, result.First().Title);
            Assert.Equal(PostStatus.Published, scheduledPost.Status);
            _notificationServiceMock.Verify(n => n.CreatePostNotificationAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.Posts.GetPostsPagedAsync(parameters, user, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetPostByIdAsync_Success()
        {
            var post = TestData.Post;

            _unitOfWorkMock.Setup(u => u.Posts.GetByIdAsync(post.PostId, It.IsAny<CancellationToken>())).ReturnsAsync(post);
            _mapperMock.Setup(m => m.Map<PostDto>(post)).Returns(new PostDto { Title = post.Title, AuthorName = post.Author.Name });

            var result = await _postService.GetPostByIdAsync(post.PostId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(post.Title, result.Title);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetPostByIdAsync_IncrementViews_Success()
        {
            var post = TestData.Post;
            int initialViews = post.Views;

            _unitOfWorkMock.Setup(u => u.Posts.GetByIdAsync(post.PostId, It.IsAny<CancellationToken>())).ReturnsAsync(post);
            _mapperMock.Setup(m => m.Map<PostDto>(post)).Returns(new PostDto { Title = post.Title, AuthorName = post.Author.Name });

            await _postService.GetPostByIdAsync(post.PostId, CancellationToken.None);

            Assert.Equal(initialViews + 1, post.Views);
        }

        [Fact]
        public async Task GetPostByIdAsync_NotFound()
        {
            _unitOfWorkMock.Setup(u => u.Posts.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Post)null!);

            await Assert.ThrowsAsync<NotFoundException>(() => _postService.GetPostByIdAsync(999, CancellationToken.None));
        }

        [Fact]
        public async Task CreatePostAsync_Published_Success()
        {
            var createPostDto = new CreatePostDto { Title = "New", Content = "Content", Tags = "" };
            var post = new Post { Title = createPostDto.Title };
            var user = TestData.User;

            _mapperMock.Setup(m => m.Map<Post>(createPostDto)).Returns(post);
            _tagServiceMock.Setup(t => t.ResolveTagsFromStringAsync(createPostDto.Tags, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Tag>());
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _notificationServiceMock.Setup(n => n.CreatePostNotificationAsync(user.Id, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await _postService.CreatePostAsync(createPostDto, GetClaims(user.Id), "Publish", CancellationToken.None);

            Assert.Equal(PostStatus.Published, post.Status);
            Assert.Equal(user.Id, post.AuthorId);

            _unitOfWorkMock.Verify(u => u.Posts.CreateAsync(post, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreatePostAsync_Draft_Success()
        {
            var createPostDto = new CreatePostDto { Title = "Draft Post", Content = "Content", Tags = "" };
            var post = new Post { Title = createPostDto.Title };
            var user = TestData.User;

            _mapperMock.Setup(m => m.Map<Post>(createPostDto)).Returns(post);
            _tagServiceMock.Setup(t => t.ResolveTagsFromStringAsync(createPostDto.Tags, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Tag>());
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            await _postService.CreatePostAsync(createPostDto, GetClaims(user.Id), "Draft", CancellationToken.None);

            Assert.Equal(PostStatus.Draft, post.Status);
        }


        [Fact]
        public async Task CreatePostAsync_Sheduled_Success()
        {
            var createPostDto = new CreatePostDto { Title = "Future", Content = "Content", Tags = "tag" };
            var post = new Post { Title = createPostDto.Title, PublishedAt = DateTime.UtcNow.AddDays(1) };
            var user = TestData.User;

            _mapperMock.Setup(m => m.Map<Post>(createPostDto)).Returns(post);
            _tagServiceMock.Setup(t => t.ResolveTagsFromStringAsync(createPostDto.Tags, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Tag>());
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            await _postService.CreatePostAsync(createPostDto, GetClaims(user.Id), "Publish", CancellationToken.None);

            Assert.Equal(PostStatus.Scheduled, post.Status);
        }

        [Fact]
        public async Task UpdatePostAsync_Success()
        {
            var post = TestData.Post;
            var user = TestData.User;
            var updatePostDto = new UpdatePostDto { PostId = post.PostId, Title = post.Title, Content = post.Content, Tags = "" };

            _unitOfWorkMock.Setup(u => u.Posts.GetByIdAsync(post.PostId, It.IsAny<CancellationToken>())).ReturnsAsync(post);
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _tagServiceMock.Setup(t => t.ResolveTagsFromStringAsync(updatePostDto.Tags, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Tag>());

            await _postService.UpdatePostAsync(updatePostDto, GetClaims(user.Id), null, CancellationToken.None);

            _unitOfWorkMock.Verify(u => u.Posts.UpdateAsync(post, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdatePostAsync_InvalidUser()
        {
            var post = TestData.Post;
            var another = TestData.Another;
            var updatePostDto = new UpdatePostDto { PostId = post.PostId, Title = post.Title, Content = post.Content, Tags = "" };

            _unitOfWorkMock.Setup(u => u.Posts.GetByIdAsync(post.PostId, It.IsAny<CancellationToken>())).ReturnsAsync(post);
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(another.Id, It.IsAny<CancellationToken>())).ReturnsAsync(another);

            await Assert.ThrowsAsync<ForbiddenException>(() => _postService.UpdatePostAsync(updatePostDto, GetClaims(another.Id), null, CancellationToken.None));
        }

        [Fact]
        public async Task UpdatePostAsync_NotFound()
        {
            var user = TestData.User;
            var updatePostDto = new UpdatePostDto { Title = "title", PostId = 999 };

            _unitOfWorkMock.Setup(u => u.Posts.GetByIdAsync(updatePostDto.PostId, It.IsAny<CancellationToken>())).ReturnsAsync((Post)null!);

            await Assert.ThrowsAsync<NotFoundException>(() => _postService.UpdatePostAsync(updatePostDto, GetClaims(user.Id), null, CancellationToken.None));
        }

        [Fact]
        public async Task DeletePostAsync_Success()
        {
            var user = TestData.User;
            var post = TestData.Post;

            _unitOfWorkMock.Setup(u => u.Posts.GetByIdAsync(post.PostId, It.IsAny<CancellationToken>())).ReturnsAsync(post);
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(post.AuthorId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            await _postService.DeletePostAsync(post.PostId, GetClaims(user.Id), CancellationToken.None);

            _cleanupMock.Verify(c => c.CleanupAsync(post, It.IsAny<ClaimsPrincipal>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.Posts.DeleteAsync(post, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeletePostAsync_InvalidUser()
        {
            var user = TestData.User;
            var another = TestData.Another;
            var post = TestData.Post;

            _unitOfWorkMock.Setup(u => u.Posts.GetByIdAsync(post.PostId, It.IsAny<CancellationToken>())).ReturnsAsync(post);
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(another.Id, It.IsAny<CancellationToken>())).ReturnsAsync(another);

            await Assert.ThrowsAsync<ForbiddenException>(() => _postService.DeletePostAsync(post.PostId, GetClaims(another.Id), CancellationToken.None));
        }

        [Fact]
        public async Task DeletePostAsync_NotFound()
        {
            var user = TestData.User;

            _unitOfWorkMock.Setup(u => u.Posts.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Post)null!);

            await Assert.ThrowsAsync<NotFoundException>(() => _postService.DeletePostAsync(999, GetClaims(user.Id), CancellationToken.None));
        }
    }
}