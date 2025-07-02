using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PodcastBlog.Application.Exceptions;
using PodcastBlog.Application.Interfaces.Services;

namespace PodcastBlog.Application.Services
{
    public class MediaService : IMediaService
    {
        private readonly ILogger<MediaService> _logger;

        public MediaService(ILogger<MediaService> logger)
        {
            _logger = logger;
        }

        public async Task<string?> GetCoverImageAsync(IFormFile? cover, CancellationToken cancellationToken)
        {
            if (cover is null)
            {
                return null;
            }

            try
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(cover.FileName);
                var path = Path.Combine("wwwroot/images", fileName);
                Directory.CreateDirectory("wwwroot/images");

                await using var stream = new FileStream(path, FileMode.Create);

                await cover.CopyToAsync(stream, cancellationToken);

                _logger.LogInformation("Изображение успешно загружено");

                return $"/images/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке изображения");

                return null;
            }
        }

        public async Task<(string path, int duration, int bitrate, string transcription)> GetAudioMetadataAsync(IFormFile? audio, CancellationToken cancellationToken)
        {
            if (audio is null)
            {
                throw new MediaException("Аудиофайл обязателен для подкаста");
            }

            try
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(audio.FileName);
                var fullPath = Path.Combine("wwwroot/audio", fileName);
                Directory.CreateDirectory("wwwroot/audio");

                await using var stream = new FileStream(fullPath, FileMode.Create);

                await audio.CopyToAsync(stream, cancellationToken);

                var tagFile = TagLib.File.Create(fullPath);
                int duration = (int)tagFile.Properties.Duration.TotalSeconds;
                int bitrate = tagFile.Properties.AudioBitrate;
                string transcription = await TranscribeAudioMockAsync(fullPath, cancellationToken);

                _logger.LogInformation("Аудиофайл успешно загружен");
                return ($"/audio/{fileName}", duration, bitrate, transcription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке аудиофайла");

                throw new MediaException("Не удалось обработать аудиофайл");
            }
        }

        public async Task<string> TranscribeAudioMockAsync(string audioPath, CancellationToken cancellationToken)
        {
            await Task.Delay(1000, cancellationToken);

            return $"Распознанная речь для {Path.GetFileName(audioPath)}";
        }
    }
}
