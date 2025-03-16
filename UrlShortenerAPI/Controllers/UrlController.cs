using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortenerAPI.Data;
using UrlShortenerAPI.Models;

namespace UrlShortenerAPI.Controllers

{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlController : ControllerBase
    {
        private readonly UrlShortenerContext _context;

        public UrlController(UrlShortenerContext context)
        {
            _context = context;
        }

        // POST: api/Url
        // Expects a JSON body with the original URL as a string
        [HttpPost]
        public async Task<IActionResult> CreateShortUrl([FromBody] string originalUrl)
        {
            if (!Uri.IsWellFormedUriString(originalUrl, UriKind.Absolute))
            {
                return BadRequest("Invalid URL");
            }

            // Generate a short token
            var shortToken = GenerateShortToken();

            // Create and save entity
            var entity = new UrlEntity
            {
                ShortUrl = shortToken,
                OriginalUrl = originalUrl
            };
            _context.Urls.Add(entity);
            await _context.SaveChangesAsync();

            // Return the short URL token (or full path if you want)
            return Ok(new { ShortUrl = shortToken });
        }

        // GET: api/Url/{shortUrl}
        // Redirect user to the original URL
        [HttpGet("{shortUrl}")]
        public async Task<IActionResult> RedirectToOriginal(string shortUrl)
        {
            var entity = await _context.Urls
                .SingleOrDefaultAsync(u => u.ShortUrl == shortUrl);

            if (entity == null)
            {
                return NotFound("URL not found");
            }

            // Redirect user to the original URL
            return Redirect(entity.OriginalUrl);
        }

        private string GenerateShortToken()
        {
            // Simple random generator for a 6-character token
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new char[6];
            var rng = new Random();

            for (int i = 0; i < random.Length; i++)
            {
                random[i] = chars[rng.Next(chars.Length)];
            }

            return new string(random);
        }
    }
}