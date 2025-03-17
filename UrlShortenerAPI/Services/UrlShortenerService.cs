using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using UrlShortenerAPI.Data;
using UrlShortenerAPI.Dtos;
using UrlShortenerAPI.Models;
using UrlShortenerAPI.Utility.Constants;

namespace UrlShortenerAPI.Services
{
    /// <summary>
    /// A service providing business logic for creating and retrieving shortened URLs.
    /// </summary>
    public class UrlShortenerService : IUrlShortenerService
    {
        private readonly UrlShortenerContext _context;
        private readonly ILogger<UrlShortenerService> _logger;

        /// <summary>
        /// Constructor that injects the EF Core context and logger.
        /// </summary>
        public UrlShortenerService(UrlShortenerContext context, ILogger<UrlShortenerService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <inheritdoc />
public async Task<ShortUrlResponseDto> CreateShortUrlAsync(CreateShortUrlDto dto)
{
    try
    {
        if (!Uri.IsWellFormedUriString(dto.OriginalUrl, UriKind.Absolute))
        {
            _logger.LogWarning("Invalid URL attempt: {Url}", dto.OriginalUrl);
            throw new ArgumentException("Invalid URL format.");
        }

        // Check if the URL already exists in the database
        var existingEntity = await _context.Urls
            .FirstOrDefaultAsync(u => u.OriginalUrl == dto.OriginalUrl);

        if (existingEntity != null)
        {
            _logger.LogInformation("Existing URL found for {Url}, returning existing short URL.", dto.OriginalUrl);

            // Ensure the full short URL is returned
            return new ShortUrlResponseDto
            {
                ShortUrlToken = existingEntity.ShortUrl,
                FullShortUrl = $"http://localhost:5251/api/urls/{existingEntity.ShortUrl}"
            };
        }

        // Generate a unique short token
        string newShortToken = Guid.NewGuid().ToString("N")[..8];

        var entity = new UrlEntity
        {
            OriginalUrl = dto.OriginalUrl,
            ShortUrl = newShortToken,
            ExpirationDate = DateTime.UtcNow.AddDays(30)
        };

        _context.Urls.Add(entity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully created new short URL: {ShortUrl} -> {OriginalUrl}", 
            newShortToken, dto.OriginalUrl);

        return new ShortUrlResponseDto
        {
            ShortUrlToken = newShortToken,
            FullShortUrl = $"http://localhost:5251/api/urls/{newShortToken}"
        };
    }
    catch (DbUpdateException dbEx)
    {
        _logger.LogError(dbEx, "Database error: {Message}", dbEx.InnerException?.Message ?? dbEx.Message);
        throw new Exception("Failed to save to the database. Check logs for details.");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "General error in URL creation.");
        throw;
    }
}





        /// <inheritdoc />
        public async Task<UrlEntity?> GetByShortUrlTokenAsync(string shortUrlToken)
        {
            // SingleOrDefaultAsync throws if there's more than one record,which is what we want if shortUrl must be unique
            UrlEntity? entity = await _context.Urls
                .SingleOrDefaultAsync(u => u.ShortUrl == shortUrlToken);

            if (entity == null)
            {
                _logger.LogInformation("No record found for short token {Token}", shortUrlToken);
                return null;
            }

            // Check if it's expired
            if (DateTime.UtcNow > entity.ExpirationDate)
            {
                _logger.LogInformation("Short token {Token} has expired.", shortUrlToken);
                return null;
            }

            return entity;
        }

        /// <summary>
        /// Private method that generates a GUID-based token.
        /// Input: none
        /// Output: A string representing the newly generated GUID (lowercased, 8 chars).
        /// </summary>
        private string GenerateUniqueGuidToken()
        {
            string rawGuid = Guid.NewGuid().ToString("N");
            string shortToken = rawGuid[..8]; // 8 chars
            return shortToken.ToLowerInvariant();
        }
    }
}
