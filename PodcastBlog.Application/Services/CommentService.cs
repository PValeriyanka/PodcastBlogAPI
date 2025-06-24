using AutoMapper;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Application.ModelsDto;
using PodcastBlog.Domain.Interfaces;
using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;

namespace PodcastBlog.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly ICommentCleanupStrategy _cleanup;
        private readonly IMapper _mapper;

        public CommentService(IUnitOfWork unitOfWork, INotificationService notificationService, ICommentCleanupStrategy cleanup, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _cleanup = cleanup;
            _mapper = mapper;
        }

        public async Task<PagedList<CommentDto>> GetCommentsByPostPagedAsync(int postId, Parameters parameters, CancellationToken cancellationToken)
        {
            var comments = await _unitOfWork.Comments.GetCommentsByPostPagedAsync(postId, parameters, cancellationToken);

            var commentsDto = _mapper.Map<IEnumerable<CommentDto>>(comments).ToList();

            return new PagedList<CommentDto>(commentsDto, comments.MetaData.TotalCount, comments.MetaData.CurrentPage, comments.MetaData.PageSize);
        }

        public async Task<CommentDto> GetCommentByIdAsync(int id, CancellationToken cancellationToken)
        {
            var comment = await _unitOfWork.Comments.GetByIdAsync(id, cancellationToken);

            var commentDto = _mapper.Map<CommentDto>(comment);

            return commentDto;
        }

        public async Task CreateCommentAsync(CommentDto commentDto, int? parentId, CancellationToken cancellationToken)
        {
            if (parentId is not null) 
            { 
                commentDto.ParentId = parentId;
            }

            var comment = _mapper.Map<Comment>(commentDto);
            comment.Status = CommentStatus.Pending;

            await _unitOfWork.Comments.CreateAsync(comment, cancellationToken);

            var post = await _unitOfWork.Posts.GetByIdAsync(comment.PostId, cancellationToken);

            await _notificationService.NewCommentNotificationAsync(comment, post, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task PublishCommentAsync(int id, CancellationToken cancellationToken)
        {
            var comment = await _unitOfWork.Comments.GetByIdAsync(id, cancellationToken);
            
            comment.Status = CommentStatus.Approved;
            comment.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteCommentAsync(int id, CancellationToken cancellationToken)
        {
            var comment = await _unitOfWork.Comments.GetByIdAsync(id, cancellationToken);

            await _cleanup.CleanupAsync(comment, cancellationToken);

            await _unitOfWork.Comments.DeleteAsync(comment, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}