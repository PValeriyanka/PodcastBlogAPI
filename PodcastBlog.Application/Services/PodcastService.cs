﻿using AutoMapper;
using Microsoft.Extensions.Logging;
using PodcastBlog.Application.Interfaces.Services;
using PodcastBlog.Application.Interfaces.Strategies;
using PodcastBlog.Application.ModelsDto.Podcast;
using PodcastBlog.Domain.Interfaces;
using PodcastBlog.Domain.Models;
using System.Security.Claims;

namespace PodcastBlog.Application.Services
{
    public class PodcastService : IPodcastService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediaService _audioService;
        private readonly IPodcastCleanupStrategy _cleanup;
        private readonly IMapper _mapper;
        private readonly ILogger<PodcastService> _logger;

        public PodcastService(IUnitOfWork unitOfWork, IMediaService audioService, IPodcastCleanupStrategy cleanup, IMapper mapper, ILogger<PodcastService> logger)
        {
            _unitOfWork = unitOfWork;
            _audioService = audioService;
            _cleanup = cleanup;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PodcastDto> GetPodcastByIdAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                var podcast = await _unitOfWork.Podcasts.GetByIdAsync(id, cancellationToken);

                var podcastDto = _mapper.Map<PodcastDto>(podcast);

                _logger.LogInformation("Подкаст успешно загружен");
                return podcastDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении подкаста");
                throw;
            }
        }

        public async Task CreatePodcastAsync(CreatePodcastDto createPodcastDto, CancellationToken cancellationToken)
        {
            try
            {
                var podcast = _mapper.Map<Podcast>(createPodcastDto);

                if (createPodcastDto.CoverImageUpload is not null)
                {
                    podcast.CoverImage = await _audioService.GetCoverImageAsync(createPodcastDto.CoverImageUpload, cancellationToken);
                }

                if (createPodcastDto.AudioUpload is not null)
                {
                    var (path, duration, bitrate, transcript) = await _audioService.GetAudioMetadataAsync(createPodcastDto.AudioUpload, cancellationToken);

                    podcast.AudioFile = path;
                    podcast.Duration = duration;
                    podcast.Bitrate = bitrate;
                    podcast.Transcript = transcript;
                }

                await _unitOfWork.Podcasts.CreateAsync(podcast, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Подкаст успешно создан");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании подкаста");
                throw;
            }
        }

        public async Task UpdatePodcastAsync(UpdatePodcastDto updatePodcastDto, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken)
        {
            try
            {
                var podcast = await _unitOfWork.Podcasts.GetByIdAsync(updatePodcastDto.PodcastId, cancellationToken);

                if (podcast is null)
                {
                    _logger.LogWarning("Обновление. Подкаст не найден");
                    return;
                }

                int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);

                var post = await _unitOfWork.Posts.GetByPodcastIdAsync(podcast.PodcastId, cancellationToken);

                var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

                if (userId != post.AuthorId && user.Role != UserRole.Administrator)
                {
                    _logger.LogWarning("Подкаст изменить может только автор поста или администратор");
                    return;
                }

                _mapper.Map(updatePodcastDto, podcast);

                if (updatePodcastDto.CoverImageUpload is not null)
                {
                    podcast.CoverImage = await _audioService.GetCoverImageAsync(updatePodcastDto.CoverImageUpload, cancellationToken);
                }

                if (updatePodcastDto.AudioUpload is not null)
                {
                    var (path, duration, bitrate, transcript) = await _audioService.GetAudioMetadataAsync(updatePodcastDto.AudioUpload, cancellationToken);

                    podcast.AudioFile = path;
                    podcast.Duration = duration;
                    podcast.Bitrate = bitrate;
                    podcast.Transcript = transcript;
                }

                await _unitOfWork.Podcasts.UpdateAsync(podcast, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Подкаст успешно обновлен");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении подкаста");
                throw;
            }
        }

        public async Task DeletePodcastAsync(int id, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken)
        {
            try
            {
                var podcast = await _unitOfWork.Podcasts.GetByIdAsync(id, cancellationToken);

                if (podcast is null)
                {
                    _logger.LogWarning("Удаление. Подкаст не найден");
                    return;
                }

                int.TryParse(userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);

                var post = await _unitOfWork.Posts.GetByPodcastIdAsync(podcast.PodcastId, cancellationToken);

                var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

                if (userId != post.AuthorId && user.Role != UserRole.Administrator)
                {
                    _logger.LogWarning("Подкаст удалить может только автор поста или администратор");
                    return;
                }

                await _cleanup.CleanupAsync(podcast, cancellationToken);

                await _unitOfWork.Podcasts.DeleteAsync(podcast, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Подкаст успешно удален");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении подкаста");
                throw;
            }
        }

        public async Task ListeningAsync(int podcastId, CancellationToken cancellationToken)
        {
            try
            {
                var podcast = await _unitOfWork.Podcasts.GetByIdAsync(podcastId, cancellationToken);

                if (podcast is null)
                {
                    _logger.LogWarning("Прослушивание. Подкаст не найден");
                    return;
                }

                podcast.ListenCount += 1;

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Прослушивание успешно засчитано");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении счётчика прослушиваний");
                throw;
            }
        }
    }
}