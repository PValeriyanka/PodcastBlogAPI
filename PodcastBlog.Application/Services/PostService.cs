using AutoMapper;
using PodcastBlog.Application.IServices;
using PodcastBlog.Application.ModelsDTO;
using PodcastBlog.Domain.IRepositories;
using PodcastBlog.Domain.Models;
using PodcastBlog.Domain.Parameters;
using PodcastBlog.Domain.Parameters.ModelParameters;

namespace PodcastBlog.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;

        public PostService(IPostRepository postRepository, IMapper mapper)
        {
            _postRepository = postRepository;
            _mapper = mapper;
        }

        public async Task<PagedList<PostDTO>> GetAllPostsPaged(PostParameters parameters, CancellationToken cancellationToken)
        {
            var posts = await _postRepository.GetAllPostsPaged(parameters, cancellationToken);
            var postsDTO = _mapper.Map<IEnumerable<PostDTO>>(posts).ToList();

            return new PagedList<PostDTO>(postsDTO, posts.MetaData.TotalCount, posts.MetaData.CurrentPage, posts.MetaData.PageSize);
        }

        public async Task<PostDTO> GetPostById(int id, CancellationToken cancellationToken)
        {
            var post = await _postRepository.GetPostById(id, cancellationToken);
            var postDTO = _mapper.Map<PostDTO>(post);

            return postDTO;
        }

        public async Task CreatePost(PostDTO postDTO, CancellationToken cancellationToken)
        {
            var post = _mapper.Map<Post>(postDTO);

            await _postRepository.CreatePost(post, cancellationToken);
        }

        public async Task UpdatePost(PostDTO postDTO, CancellationToken cancellationToken)
        {
            var post = _mapper.Map<Post>(postDTO);

            await _postRepository.UpdatePost(post, cancellationToken);
        }

        public async Task DeletePost(int id, CancellationToken cancellationToken)
        {
            await _postRepository.DeletePost(id, cancellationToken);
        }
    }
}
