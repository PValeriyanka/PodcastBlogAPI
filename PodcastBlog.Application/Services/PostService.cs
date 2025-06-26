using AutoMapper;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Application.ModelsDto;
using PodcastBlog.Domain.Interfaces;
using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;
using PodcastBlog.Domain.Parameters.ModelParameters;
using System.Security.Claims;

namespace PodcastBlog.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITagService _tagService;
        private readonly INotificationService _notificationService;
        private readonly IPostCleanupStrategy _cleanup;
        private readonly IMapper _mapper;

        public PostService(IUnitOfWork unitOfWork, ITagService tagService, INotificationService notificationService, IPostCleanupStrategy cleanup, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _tagService = tagService;
            _notificationService = notificationService;
            _cleanup = cleanup;
            _mapper = mapper;
        }

        public async Task<PagedList<PostDto>> GetPostsPagedAsync(PostParameters parameters, ClaimsPrincipal userPrincipal, string? type, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var sheduledPosts = await _unitOfWork.Posts.GetSheduledPostsAsync(cancellationToken);

            foreach (var post in sheduledPosts)
            {
                post.Status = PostStatus.Published;

                await _notificationService.CreatePostNotificationAsync(post.AuthorId, cancellationToken);
            }

            int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int Id);

            var currentUser = await _unitOfWork.Users.GetByIdAsync(Id, cancellationToken);

            var posts = await _unitOfWork.Posts.GetPostsPagedAsync(parameters, currentUser, type, cancellationToken);

            var postsDto = _mapper.Map<IEnumerable<PostDto>>(posts).ToList();

            return new PagedList<PostDto>(postsDto, posts.MetaData.TotalCount, posts.MetaData.CurrentPage, posts.MetaData.PageSize);
        }

        public async Task<PostDto> GetPostByIdAsync(int id, CancellationToken cancellationToken)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(id, cancellationToken);

            post.Views += 1;

            var postDto = _mapper.Map<PostDto>(post);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return postDto;
        }

        public async Task CreatePostAsync(PostDto postDto, ClaimsPrincipal userPrincipal, string status, CancellationToken cancellationToken)
        {
            int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int authorId);
            
            postDto.AuthorId = authorId;

            await ApplyStatus(postDto, status, userPrincipal, cancellationToken);

            var post = _mapper.Map<Post>(postDto);

            post.Tags = await _tagService.ResolveTagsFromStringAsync(postDto.Tags, cancellationToken);

            await _unitOfWork.Posts.CreateAsync(post, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdatePostAsync(PostDto postDto, ClaimsPrincipal userPrincipal, string? status, CancellationToken cancellationToken)
        {
            if (status is not null)
            {
                await ApplyStatus(postDto, status, userPrincipal, cancellationToken);
            }

            var post = await _unitOfWork.Posts.GetByIdAsync(postDto.PostId, cancellationToken);

            _mapper.Map(postDto, post);

            post.Tags = await _tagService.ResolveTagsFromStringAsync(postDto.Tags, cancellationToken);

            await _unitOfWork.Posts.UpdateAsync(post, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeletePostAsync(int id, CancellationToken cancellationToken)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(id, cancellationToken);

            await _cleanup.CleanupAsync(post, cancellationToken);

            await _unitOfWork.Posts.DeleteAsync(post, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task ApplyStatus(PostDto postDto, string status, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken)
        {
            if ((postDto.PublishedAt == default || postDto.PublishedAt <= DateTime.UtcNow) && status == "Publish")
            {
                postDto.PublishedAt = DateTime.UtcNow;
                postDto.Status = PostStatus.Published;

                int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);

                var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
                
                await _notificationService.CreatePostNotificationAsync(user.Id, cancellationToken);
            }
            else if (postDto.PublishedAt > DateTime.UtcNow && status == "Publish")
            {
                postDto.Status = PostStatus.Scheduled;
            }
            else
            {
                postDto.Status = PostStatus.Draft;
            }
        }
    }
}