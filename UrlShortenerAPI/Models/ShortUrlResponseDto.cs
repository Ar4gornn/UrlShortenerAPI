namespace UrlShortenerAPI.Dtos
{
    /// <summary>
    /// DTO representing the data returned after successfully creating a short URL.
    /// </summary>
    public class ShortUrlResponseDto
    {
        public string ShortUrlToken { get; set; } = string.Empty;
        public string FullShortUrl { get; set; } = string.Empty;
    }
}