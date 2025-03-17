using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using UrlShortenerAPI.Data;
using UrlShortenerAPI.Models;
using UrlShortenerAPI.Services;
using Xunit;

namespace UrlShortenerAPI.Tests.Services
{
    public class UrlShortenerServiceTests
    {
        // We'll use an in-memory EF Core context for simplicity
        private UrlShortenerContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<UrlShortenerContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new UrlShortenerContext(options);
        }

        [Fact]
        public async Task CreateShortUrlAsync_WithValidUrl_ReturnsShortUrlResponseDto()
        {
            // Arrange
            var context = GetInMemoryContext();
            var loggerMock = new Mock<ILogger<UrlShortenerService>>();
            var service = new UrlShortenerService(context, loggerMock.Object);

            var dto = new CreateShortUrlDto
            {
                OriginalUrl = "https://example.com"
            };

            // Act
            var result = await service.CreateShortUrlAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ShortUrlToken);
            Assert.NotNull(result.FullShortUrl);

            // Verify that the entity was actually saved to the DB
            var savedEntity = await context.Urls.FirstOrDefaultAsync(u => u.ShortUrl == result.ShortUrlToken);
            Assert.NotNull(savedEntity);
            Assert.Equal(dto.OriginalUrl, savedEntity.OriginalUrl);
        }

        [Fact]
        public async Task CreateShortUrlAsync_WithInvalidUrl_ThrowsArgumentException()
        {
            // Arrange
            var context = GetInMemoryContext();
            var loggerMock = new Mock<ILogger<UrlShortenerService>>();
            var service = new UrlShortenerService(context, loggerMock.Object);

            var dto = new CreateShortUrlDto
            {
                OriginalUrl = "not-a-valid-url"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateShortUrlAsync(dto));
        }

        [Fact]
        public async Task CreateShortUrlAsync_WithExistingUrl_ReturnsExistingShortUrl()
        {
            // Arrange
            var context = GetInMemoryContext();
            var loggerMock = new Mock<ILogger<UrlShortenerService>>();
            var service = new UrlShortenerService(context, loggerMock.Object);

            var existingEntity = new UrlEntity
            {
                OriginalUrl = "https://example.com",
                ShortUrl = "abcdef12",
                ExpirationDate = DateTime.UtcNow.AddDays(30)
            };

            await context.Urls.AddAsync(existingEntity);
            await context.SaveChangesAsync();

            var dto = new CreateShortUrlDto { OriginalUrl = "https://example.com" };

            // Act
            var result = await service.CreateShortUrlAsync(dto);

            // Assert
            Assert.Equal("abcdef12", result.ShortUrlToken);
            Assert.Contains("abcdef12", result.FullShortUrl);
            // The DB shouldn’t have created a second record
            var count = await context.Urls.CountAsync();
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task GetByShortUrlTokenAsync_WithValidNonExpiredRecord_ReturnsEntity()
        {
            // Arrange
            var context = GetInMemoryContext();
            var loggerMock = new Mock<ILogger<UrlShortenerService>>();
            var service = new UrlShortenerService(context, loggerMock.Object);

            var shortToken = "abcd1234";
            var entity = new UrlEntity
            {
                OriginalUrl = "https://example.com",
                ShortUrl = shortToken,
                ExpirationDate = DateTime.UtcNow.AddDays(1)
            };

            await context.Urls.AddAsync(entity);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetByShortUrlTokenAsync(shortToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity.OriginalUrl, result.OriginalUrl);
        }

        [Fact]
        public async Task GetByShortUrlTokenAsync_WithExpiredRecord_ReturnsNull()
        {
            // Arrange
            var context = GetInMemoryContext();
            var loggerMock = new Mock<ILogger<UrlShortenerService>>();
            var service = new UrlShortenerService(context, loggerMock.Object);

            var shortToken = "expired12";
            var entity = new UrlEntity
            {
                OriginalUrl = "https://oldlink.com",
                ShortUrl = shortToken,
                ExpirationDate = DateTime.UtcNow.AddDays(-1) // already expired
            };

            await context.Urls.AddAsync(entity);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetByShortUrlTokenAsync(shortToken);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByShortUrlTokenAsync_WithNonExistentToken_ReturnsNull()
        {
            // Arrange
            var context = GetInMemoryContext();
            var loggerMock = new Mock<ILogger<UrlShortenerService>>();
            var service = new UrlShortenerService(context, loggerMock.Object);

            // Act
            var result = await service.GetByShortUrlTokenAsync("doesnotexist");

            // Assert
            Assert.Null(result);
        }
    }
}
