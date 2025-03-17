namespace UrlShortenerAPI.Models
{
    /// <summary>
    /// DTO representing the data needed to create a shortened URL.
    /// </summary>
    public class CreateShortUrlDto
    {
        public string OriginalUrl { get; set; } = string.Empty;
    }
}