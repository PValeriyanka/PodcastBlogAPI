using AutoMapper;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Application.ModelsDto;
using PodcastBlog.Domain.Interfaces;
using PodcastBlog.Domain.Models;

namespace PodcastBlog.Application.Services
{
    public class PodcastService : IPodcastService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediaService _audioService;
        private readonly IPodcastCleanupStrategy _cleanup;
        private readonly IMapper _mapper;

        public PodcastService(IUnitOfWork unitOfWork, IMediaService audioService, IPodcastCleanupStrategy cleanup, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _audioService = audioService;
            _cleanup = cleanup;
            _mapper = mapper;
        }

        public async Task<PodcastDto> GetPodcastByIdAsync(int id, CancellationToken cancellationToken)
        {
            var podcast = await _unitOfWork.Podcasts.GetByIdAsync(id, cancellationToken);

            var podcastDto = _mapper.Map<PodcastDto>(podcast);

            return podcastDto;
        }

        public async Task CreatePodcastAsync(PodcastDto podcastDto, CancellationToken cancellationToken)
        {
            var podcast = _mapper.Map<Podcast>(podcastDto);

            if (podcastDto.CoverImageUpload is not null)
            {
                podcast.CoverImage = await _audioService.GetCoverImageAsync(podcastDto.CoverImageUpload, cancellationToken);
            }

            if (podcastDto.AudioUpload is not null)
            {
                var (path, duration, bitrate, transcript) = await _audioService.GetAudioMetadataAsync(podcastDto.AudioUpload, cancellationToken);

                podcast.AudioFile = path;
                podcast.Duration = duration;
                podcast.ListenCount = 0;
                podcast.Transcript = transcript;
            }

            await _unitOfWork.Podcasts.CreateAsync(podcast, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdatePodcastAsync(PodcastDto podcastDto, CancellationToken cancellationToken)
        {
            var podcast = await _unitOfWork.Podcasts.GetByIdAsync(podcastDto.PodcastId, cancellationToken);

            _mapper.Map(podcastDto, podcast);

            if (podcastDto.CoverImageUpload is not null)
            {
                podcast.CoverImage = await _audioService.GetCoverImageAsync(podcastDto.CoverImageUpload, cancellationToken);
            }

            if (podcastDto.AudioUpload is not null)
            {
                var (path, duration, bitrate, transcript) = await _audioService.GetAudioMetadataAsync(podcastDto.AudioUpload, cancellationToken);
                
                podcast.AudioFile = path;
                podcast.Duration = duration;
                podcast.Transcript = transcript;
            }

            await _unitOfWork.Podcasts.UpdateAsync(podcast, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeletePodcastAsync(int id, CancellationToken cancellationToken)
        {
            var podcast = await _unitOfWork.Podcasts.GetByIdAsync(id, cancellationToken);

            await _cleanup.CleanupAsync(podcast, cancellationToken);

            await _unitOfWork.Podcasts.DeleteAsync(podcast, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task ListeningAsync(int podcastId, CancellationToken cancellationToken)
        {
            var podcast = await _unitOfWork.Podcasts.GetByIdAsync(podcastId, cancellationToken);

            podcast.ListenCount += 1;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}