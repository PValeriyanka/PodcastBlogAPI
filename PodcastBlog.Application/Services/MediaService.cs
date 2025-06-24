using Microsoft.AspNetCore.Http;
using PodcastBlog.Application.Interfaces.Services;

namespace PodcastBlog.Application.Services
{
    public class MediaService : IMediaService
    {
        public async Task<string> GetCoverImageAsync(IFormFile cover, CancellationToken cancellationToken)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(cover.FileName);
            var path = Path.Combine("wwwroot/images", fileName);

            Directory.CreateDirectory("wwwroot/images");
            await using (var stream = new FileStream(path, FileMode.Create))
            {
                await cover.CopyToAsync(stream, cancellationToken);
            }

            return $"/images/{fileName}"; 
        }

        public async Task<(string path, int duration, int bitrate, string transcription)> GetAudioMetadataAsync(IFormFile audio, CancellationToken cancellationToken)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(audio.FileName);
            var fullPath = Path.Combine("wwwroot/audio", fileName);

            Directory.CreateDirectory("wwwroot/audio");
            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await audio.CopyToAsync(stream, cancellationToken);
            }

            var tagFile = TagLib.File.Create(fullPath);
            int duration = (int)tagFile.Properties.Duration.TotalSeconds;
            int bitrate = tagFile.Properties.AudioBitrate;
            string transcription = await TranscribeAudioMockAsync(fullPath, cancellationToken);

            return ($"/audio/{fileName}", duration, bitrate, transcription);
        }

        public async Task<string> TranscribeAudioMockAsync(string audioPath, CancellationToken cancellationToken)
        {
            await Task.Delay(1000, cancellationToken);

            return $"Распознанная речь для {Path.GetFileName(audioPath)}";
        }
    }
}
