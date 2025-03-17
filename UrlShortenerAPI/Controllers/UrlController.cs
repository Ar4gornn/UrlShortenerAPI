using Microsoft.AspNetCore.Mvc;
using UrlShortenerAPI.Dtos;
using UrlShortenerAPI.Services;

namespace UrlShortenerAPI.Controllers
{
    /// <summary>
    /// Controller for handling URL shortener endpoints.
    /// </summary>
    [ApiController]
    [Route("api/urls")]  // Ensure it matches what the frontend calls
    public class UrlsController : ControllerBase
    {
        private readonly IUrlShortenerService _urlShortenerService;

        /// <summary>
        /// Constructs the UrlController with the required service.
        /// </summary>
        public UrlsController(IUrlShortenerService urlShortenerService)
        {
            _urlShortenerService = urlShortenerService;
        }

        /// <summary>
        /// Creates a new short URL token for the specified original URL.
        /// </summary>
        /// <param name="dto">The DTO containing the original URL.</param>
        /// <returns>A 200 response with a ShortUrlResponseDto, or 400/500 on error.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateShortUrl([FromBody] CreateShortUrlDto dto)
        {
            try
            {
                // Validate input
                if (dto == null || string.IsNullOrWhiteSpace(dto.OriginalUrl))
                {
                    return BadRequest(new { error = "Original URL cannot be empty." });
                }

                ShortUrlResponseDto result = await _urlShortenerService.CreateShortUrlAsync(dto);
                return Ok(result);
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new { error = argEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while processing your request.", details = ex.Message });
            }
        }

        /// <summary>
        /// Redirects the user to the original URL for the given short token, if it exists and is not expired.
        /// </summary>
        /// <param name="shortUrl">The short token to look up.</param>
        /// <returns>A redirect (302) to the original URL, or 404 if not found/expired.</returns>
        [HttpGet("{shortUrl}")]
        public async Task<IActionResult> RedirectToOriginal(string shortUrl)
        {
            var entity = await _urlShortenerService.GetByShortUrlTokenAsync(shortUrl);

            if (entity == null)
            {
                return NotFound(new { error = "URL not found or expired." });
            }

            return Redirect(entity.OriginalUrl);
        }
    }
}
