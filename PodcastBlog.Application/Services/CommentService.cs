using AutoMapper;
using PodcastBlog.Application.IServices;
using PodcastBlog.Application.ModelsDTO;
using PodcastBlog.Domain.IRepositories;
using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;

namespace PodcastBlog.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IMapper _mapper;

        public CommentService(ICommentRepository commentRepository, IMapper mapper)
        {
            _commentRepository = commentRepository;
            _mapper = mapper;
        }

        public async Task<PagedList<CommentDTO>> GetCommentsByPostPaged(int postId, Parameters parameters, CancellationToken cancellationToken)
        {
            var comments = await _commentRepository.GetCommentsByPostPaged(postId, parameters, cancellationToken);
            var commentsDTO = _mapper.Map<IEnumerable<CommentDTO>>(comments).ToList();

            return new PagedList<CommentDTO>(commentsDTO, comments.MetaData.TotalCount, comments.MetaData.CurrentPage, comments.MetaData.PageSize);
        }

        public async Task<CommentDTO> GetCommentById(int id, CancellationToken cancellationToken)
        {
            var comment = await _commentRepository.GetCommentById(id, cancellationToken);
            var commentDTO = _mapper.Map<CommentDTO>(comment);

            return commentDTO;
        }

        public async Task CreateComment(CommentDTO commentDTO, CancellationToken cancellationToken)
        {
            var comment = _mapper.Map<Comment>(commentDTO);

            await _commentRepository.CreateComment(comment, cancellationToken);
        }

        public async Task UpdateComment(CommentDTO commentDTO, CancellationToken cancellationToken)
        {
            var comment = _mapper.Map<Comment>(commentDTO);

            await _commentRepository.UpdateComment(comment, cancellationToken);
        }

        public async Task DeleteComment(int id, CancellationToken cancellationToken)
        {
            await _commentRepository.DeleteComment(id, cancellationToken);
        }
    }
}
