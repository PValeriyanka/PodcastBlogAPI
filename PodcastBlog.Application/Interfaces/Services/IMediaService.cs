using Microsoft.AspNetCore.Http;

namespace PodcastBlog.Application.Interfaces.Services
{
    public interface IMediaService
    {
        Task<string?> GetCoverImageAsync(IFormFile? cover, CancellationToken cancellationToken);
        Task<(string path, int duration, int bitrate, string transcription)> GetAudioMetadataAsync(IFormFile file, CancellationToken cancellationToken);
        Task<string> TranscribeAudioMockAsync(string? audioPath, CancellationToken cancellationToken);
    }
}
