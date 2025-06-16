using AutoMapper;
using PodcastBlog.Application.IServices;
using PodcastBlog.Application.ModelsDTO;
using PodcastBlog.Domain.IRepositories;
using PodcastBlog.Domain.Models;

namespace PodcastBlog.Application.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;
        private readonly IMapper _mapper;

        public TagService(ITagRepository tagRepository, IMapper mapper)
        {
            _tagRepository = tagRepository;
            _mapper = mapper;
        }

        public async Task<TagDTO> GetTagById(int id, CancellationToken cancellationToken)
        {
            var tag = await _tagRepository.GetTagById(id, cancellationToken);
            var tagDTO = _mapper.Map<TagDTO>(tag);

            return tagDTO;
        }

        public async Task CreateTag(TagDTO tagDTO, CancellationToken cancellationToken)
        {
            var tag = _mapper.Map<Tag>(tagDTO);

            await _tagRepository.CreateTag(tag, cancellationToken);
        }

        public async Task UpdateTag(TagDTO tagDTO, CancellationToken cancellationToken)
        {
            var tag = _mapper.Map<Tag>(tagDTO);

            await _tagRepository.UpdateTag(tag, cancellationToken);
        }

        public async Task DeleteTag(int id, CancellationToken cancellationToken)
        {
            await _tagRepository.DeleteTag(id, cancellationToken);
        }
    }
}
