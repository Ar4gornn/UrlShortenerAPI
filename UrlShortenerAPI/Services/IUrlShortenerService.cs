using UrlShortenerAPI.Models;

namespace UrlShortenerAPI.Services
{
    /// <summary>
    /// Defines service methods for creating and retrieving shortened URLs.
    /// </summary>
    public interface IUrlShortenerService
    {
        Task<ShortUrlResponseDto> CreateShortUrlAsync(CreateShortUrlDto dto);
        Task<UrlEntity?> GetByShortUrlTokenAsync(string shortUrlToken);
    }
}