namespace UrlShortenerAPI.Models
{
    public class UrlEntity
    {
        public int Id { get; set; }
        public string ShortUrl { get; set; } = string.Empty;
        public string OriginalUrl { get; set; } = string.Empty;
    }
}