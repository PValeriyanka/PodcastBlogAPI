using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Application.ModelsDto.Podcast;
using PodcastBlog.Application.Services;
using PodcastBlog.Domain.Interfaces;
using PodcastBlog.Domain.Interfaces.Repositories;
using PodcastBlog.Domain.Models;
using PodcastBlog.Infrastructure.ExceptionsHandler.Exceptions;
using PodcastBlog.Tests.TestUtils;
using System.Security.Claims;

namespace PodcastBlog.Tests
{
    public class PodcastServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IMediaService> _mediaServiceMock = new();
        private readonly Mock<IPodcastCleanupStrategy> _cleanupMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<ILogger<PodcastService>> _loggerMock = new();
        private readonly PodcastService _podcastService;
        private readonly Mock<IPodcastRepository> _podcastRepositoryMock = new();
        private readonly Mock<IPostRepository> _postRepositoryMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();

        public PodcastServiceTests()
        {
            _unitOfWorkMock.Setup(u => u.Podcasts).Returns(_podcastRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.Posts).Returns(_postRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);

            _podcastService = new PodcastService(
                _unitOfWorkMock.Object,
                _mediaServiceMock.Object,
                _cleanupMock.Object,
                _mapperMock.Object,
                _loggerMock.Object
            );
        }

        private ClaimsPrincipal GetClaims(int userId) => new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));

        [Fact]
        public async Task GetPodcastByIdAsync_Success()
        {
            var podcast = TestData.Podcast;

            _unitOfWorkMock.Setup(u => u.Podcasts.GetByIdAsync(podcast.PodcastId, It.IsAny<CancellationToken>())).ReturnsAsync(podcast);
            _mapperMock.Setup(m => m.Map<PodcastDto>(podcast)).Returns(new PodcastDto { Title = podcast.Title });

            var result = await _podcastService.GetPodcastByIdAsync(podcast.PodcastId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(podcast.Title, result.Title);
        }

        [Fact]
        public async Task GetPodcastByIdAsync_NotFound()
        {
            _unitOfWorkMock.Setup(u => u.Podcasts.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Podcast)null!);

            await Assert.ThrowsAsync<NotFoundException>(() => _podcastService.GetPodcastByIdAsync(999, CancellationToken.None));
        }

        [Fact]
        public async Task CreatePodcastAsync_Success()
        {
            var createPodcastDto = new CreatePodcastDto
            {
                Title = "New Podcast",
                AudioUpload = new Mock<IFormFile>().Object,
                CoverImageUpload = new Mock<IFormFile>().Object
            };
            var podcast = new Podcast { Title = createPodcastDto.Title, AudioFile = "audio.mp3", CoverImage = "cover.jpg" };

            _mapperMock.Setup(m => m.Map<Podcast>(createPodcastDto)).Returns(podcast);
            _mediaServiceMock.Setup(m => m.GetCoverImageAsync(createPodcastDto.CoverImageUpload, It.IsAny<CancellationToken>())).ReturnsAsync("cover.jpg");
            _mediaServiceMock.Setup(m => m.GetAudioMetadataAsync(createPodcastDto.AudioUpload, It.IsAny<CancellationToken>())).ReturnsAsync(("audio.mp3", 123, 128, "transcript"));

            await _podcastService.CreatePodcastAsync(createPodcastDto, CancellationToken.None);

            Assert.Equal("cover.jpg", podcast.CoverImage);
            Assert.Equal("audio.mp3", podcast.AudioFile);
            Assert.Equal(123, podcast.Duration);
            Assert.Equal(128, podcast.Bitrate);
            Assert.Equal("transcript", podcast.Transcript);

            _unitOfWorkMock.Verify(u => u.Podcasts.CreateAsync(podcast, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdatePodcastAsync_Success()
        {
            var podcast = TestData.Podcast;
            var post = TestData.Post;
            var user = TestData.User;
            var updatePodcastDto = new UpdatePodcastDto { PodcastId = podcast.PodcastId };

            _unitOfWorkMock.Setup(u => u.Podcasts.GetByIdAsync(updatePodcastDto.PodcastId, It.IsAny<CancellationToken>())).ReturnsAsync(podcast);
            _unitOfWorkMock.Setup(u => u.Posts.GetByPodcastIdAsync(updatePodcastDto.PodcastId, It.IsAny<CancellationToken>())).ReturnsAsync(post);
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            await _podcastService.UpdatePodcastAsync(updatePodcastDto, GetClaims(user.Id), CancellationToken.None);

            _unitOfWorkMock.Verify(u => u.Podcasts.UpdateAsync(podcast, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdatePodcastAsync_InvalidUser()
        {
            var podcast = TestData.Podcast;
            var post = TestData.Post;
            var another = TestData.Another;
            var updatePodcastDto = new UpdatePodcastDto { PodcastId = podcast.PodcastId };

            _unitOfWorkMock.Setup(u => u.Podcasts.GetByIdAsync(updatePodcastDto.PodcastId, It.IsAny<CancellationToken>())).ReturnsAsync(podcast);
            _unitOfWorkMock.Setup(u => u.Posts.GetByPodcastIdAsync(updatePodcastDto.PodcastId, It.IsAny<CancellationToken>())).ReturnsAsync(post);
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(another.Id, It.IsAny<CancellationToken>())).ReturnsAsync(another);

            await Assert.ThrowsAsync<ForbiddenException>(() => _podcastService.UpdatePodcastAsync(updatePodcastDto, GetClaims(another.Id), CancellationToken.None));
        }

        [Fact]
        public async Task UpdatePodcastAsync_NotFound()
        {
            var user = TestData.User;
            var updatePodcastDto = new UpdatePodcastDto { PodcastId = 999 };

            _unitOfWorkMock.Setup(u => u.Podcasts.GetByIdAsync(updatePodcastDto.PodcastId, It.IsAny<CancellationToken>())).ReturnsAsync((Podcast)null!);

            await Assert.ThrowsAsync<NotFoundException>(() => _podcastService.UpdatePodcastAsync(updatePodcastDto, GetClaims(user.Id), CancellationToken.None));
        }

        [Fact]
        public async Task DeletePodcastAsync_Success()
        {
            var podcast = TestData.Podcast;
            var post = TestData.Post;
            var user = TestData.User;

            _unitOfWorkMock.Setup(u => u.Podcasts.GetByIdAsync(podcast.PodcastId, It.IsAny<CancellationToken>())).ReturnsAsync(podcast);
            _unitOfWorkMock.Setup(u => u.Posts.GetByPodcastIdAsync(podcast.PodcastId, It.IsAny<CancellationToken>())).ReturnsAsync(post);
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            await _podcastService.DeletePodcastAsync(podcast.PodcastId, GetClaims(user.Id), CancellationToken.None);

            _cleanupMock.Verify(c => c.CleanupAsync(podcast, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.Podcasts.DeleteAsync(podcast, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeletePodcastAsync_InvalidUser()
        {
            var podcast = TestData.Podcast;
            var post = TestData.Post;
            var another = TestData.Another;

            _unitOfWorkMock.Setup(u => u.Podcasts.GetByIdAsync(podcast.PodcastId, It.IsAny<CancellationToken>())).ReturnsAsync(podcast);
            _unitOfWorkMock.Setup(u => u.Posts.GetByPodcastIdAsync(podcast.PodcastId, It.IsAny<CancellationToken>())).ReturnsAsync(post);
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(another.Id, It.IsAny<CancellationToken>())).ReturnsAsync(another);

            await Assert.ThrowsAsync<ForbiddenException>(() => _podcastService.DeletePodcastAsync(podcast.PodcastId, GetClaims(another.Id), CancellationToken.None));
        }

        [Fact]
        public async Task DeletePodcastAsync_NotFound()
        {
            var user = TestData.User;

            _unitOfWorkMock.Setup(u => u.Podcasts.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Podcast)null!);

            await Assert.ThrowsAsync<NotFoundException>(() => _podcastService.DeletePodcastAsync(999, GetClaims(user.Id), CancellationToken.None));
        }

        [Fact]
        public async Task ListeningAsync_Success()
        {
            var podcast = TestData.Podcast;

            _unitOfWorkMock.Setup(u => u.Podcasts.GetByIdAsync(podcast.PodcastId, It.IsAny<CancellationToken>())).ReturnsAsync(podcast);

            await _podcastService.ListeningAsync(podcast.PodcastId, CancellationToken.None);

            Assert.Equal(1, podcast.ListenCount);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ListeningAsync_NotFound()
        {
            _unitOfWorkMock.Setup(u => u.Podcasts.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Podcast)null!);

            await Assert.ThrowsAsync<NotFoundException>(() => _podcastService.ListeningAsync(999, CancellationToken.None));
        }
    }
}
