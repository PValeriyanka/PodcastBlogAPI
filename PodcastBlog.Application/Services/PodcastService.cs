using AutoMapper;
using PodcastBlog.Application.IServices;
using PodcastBlog.Application.ModelsDTO;
using PodcastBlog.Domain.IRepositories;
using PodcastBlog.Domain.Models;

namespace PodcastBlog.Application.Services
{
    public class PodcastService : IPodcastService
    {
        private readonly IPodcastRepository _podcastRepository;
        private readonly IMapper _mapper;

        public PodcastService(IPodcastRepository podcastRepository, IMapper mapper)
        {
            _podcastRepository = podcastRepository;
            _mapper = mapper;
        }

        public async Task<PodcastDTO> GetPodcastById(int id, CancellationToken cancellationToken)
        {
            var podcast = await _podcastRepository.GetPodcastById(id, cancellationToken);
            var podcastDTO = _mapper.Map<PodcastDTO>(podcast);

            return podcastDTO;
        }

        public async Task CreatePodcast(PodcastDTO podcastDTO, CancellationToken cancellationToken)
        {
            var podcast = _mapper.Map<Podcast>(podcastDTO);

            await _podcastRepository.CreatePodcast(podcast, cancellationToken);
        }

        public async Task UpdatePodcast(PodcastDTO podcastDTO, CancellationToken cancellationToken)
        {
            var podcast = _mapper.Map<Podcast>(podcastDTO);

            await _podcastRepository.UpdatePodcast(podcast, cancellationToken);
        }

        public async Task DeletePodcast(int id, CancellationToken cancellationToken)
        {
            await _podcastRepository.DeletePodcast(id, cancellationToken);
        }
    }
}
