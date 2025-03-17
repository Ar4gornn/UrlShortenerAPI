namespace UrlShortenerAPI.Models
{
    /// <summary>
    /// Represents a shortened URL record in the database.
    /// </summary>
    public class UrlEntity
    {
        public int Id { get; set; }
        public string ShortUrl { get; set; } = string.Empty;
        public string OriginalUrl { get; set; } = string.Empty;
		public DateTime ExpirationDate { get; set; }
    }
}
