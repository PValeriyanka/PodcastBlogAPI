using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Application.ModelsDto.Tag;
using PodcastBlog.Application.Services;
using PodcastBlog.Domain.Interfaces;
using PodcastBlog.Domain.Interfaces.Repositories;
using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;
using PodcastBlog.Tests.TestUtils;

namespace PodcastBlog.Tests
{
    public class TagServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<ILogger<TagService>> _loggerMock = new();
        private readonly Mock<ITagCleanupStrategy> _cleanupMock = new();
        private readonly Mock<ITagRepository> _tagRepositoryMock = new();
        private readonly TagService _tagService;

        public TagServiceTests()
        {
            _unitOfWorkMock.Setup(u => u.Tags).Returns(_tagRepositoryMock.Object);

            _tagService = new TagService(
                _unitOfWorkMock.Object,
                _cleanupMock.Object,
                _mapperMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task GetAllTagsPagedAsync_Success()
        {
            var tag = TestData.TagTest;
            var tags = new List<Tag> { tag };
            var paged = new PagedList<Tag>(tags, 1, 1, 10);
            var dto = new List<TagDto> { new() { Name = tag.Name } };

            _unitOfWorkMock.Setup(u => u.Tags.GetAllTagsPagedAsync(It.IsAny<Parameters>(), It.IsAny<CancellationToken>())).ReturnsAsync(paged);
            _mapperMock.Setup(m => m.Map<IEnumerable<TagDto>>(paged)).Returns(dto);

            var result = await _tagService.GetAllTagsPagedAsync(new Parameters(), CancellationToken.None);

            Assert.Single(result);
            Assert.Equal(tag.Name, result.First().Name);
        }

        [Fact]
        public async Task GetTagByIdAsync_Success()
        {
            var tag = TestData.TagTest;
            var tagDto = new TagDto { Name = tag.Name };

            _unitOfWorkMock.Setup(u => u.Tags.GetByIdAsync(tag.TagId, It.IsAny<CancellationToken>())).ReturnsAsync(tag);
            _mapperMock.Setup(m => m.Map<TagDto>(tag)).Returns(tagDto);

            var result = await _tagService.GetTagByIdAsync(tag.TagId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(tag.Name, result.Name);
        }

        [Fact]
        public async Task GetTagByIdAsync_NotFound()
        {
            _unitOfWorkMock.Setup(u => u.Tags.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Tag)null!);

            var result = await _tagService.GetTagByIdAsync(999, CancellationToken.None);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateTagAsync_Success()
        {
            var createTagDto = new CreateTagDto { Name = TestData.TagTest.Name };
            var tag = new Tag { Name = createTagDto.Name };

            _mapperMock.Setup(m => m.Map<Tag>(createTagDto)).Returns(tag);

            await _tagService.CreateTagAsync(createTagDto, CancellationToken.None);

            _unitOfWorkMock.Verify(u => u.Tags.CreateAsync(tag, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteTagAsync_Success()
        {
            var tag = TestData.TagTest;

            _unitOfWorkMock.Setup(u => u.Tags.GetByIdAsync(tag.TagId, It.IsAny<CancellationToken>())).ReturnsAsync(tag);

            await _tagService.DeleteTagAsync(tag.TagId, CancellationToken.None);

            _cleanupMock.Verify(c => c.CleanupAsync(tag, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.Tags.DeleteAsync(tag, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteTagAsync_NotFound()
        {
            _unitOfWorkMock.Setup(u => u.Tags.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Tag)null!);

            await _tagService.DeleteTagAsync(999, CancellationToken.None);

            _cleanupMock.Verify(c => c.CleanupAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.Tags.DeleteAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ResolveTagsFromStringAsync_IsExisting_Success()
        {
            var tag = TestData.TagTest;

            _unitOfWorkMock.Setup(u => u.Tags.GetTagByNameAsync(tag.Name, It.IsAny<CancellationToken>())).ReturnsAsync(tag);

            var result = await _tagService.ResolveTagsFromStringAsync("#" + tag.Name, CancellationToken.None);

            Assert.Single(result);
            Assert.Equal(tag.Name, result.First().Name);
        }

        [Fact]
        public async Task ResolveTagsFromStringAsync_Create_Success()
        {
            _unitOfWorkMock.Setup(u => u.Tags.GetTagByNameAsync("tag", It.IsAny<CancellationToken>())).ReturnsAsync((Tag)null!);

            var result = await _tagService.ResolveTagsFromStringAsync("tag", CancellationToken.None);

            _unitOfWorkMock.Verify(u => u.Tags.CreateAsync(It.Is<Tag>(t => t.Name == "tag"), It.IsAny<CancellationToken>()), Times.Once);
            Assert.Single(result);
            Assert.Equal("tag", result.First().Name);
        }

        [Fact]
        public async Task ResolveTagsFromStringAsync_Ignore_Success()
        {
            var result = await _tagService.ResolveTagsFromStringAsync(null, CancellationToken.None);

            Assert.Empty(result);
        }
    }
}
