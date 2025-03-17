using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UrlShortenerAPI.Controllers;
using UrlShortenerAPI.Models;
using UrlShortenerAPI.Services;
using Xunit;

namespace UrlShortenerAPI.Tests.Controllers
{
    public class UrlsControllerTests
    {
        private readonly Mock<IUrlShortenerService> _mockService;
        private readonly UrlsController _controller;

        public UrlsControllerTests()
        {
            _mockService = new Mock<IUrlShortenerService>();
            _controller = new UrlsController(_mockService.Object);
        }

        [Fact]
        public async Task CreateShortUrl_WithValidDto_ReturnsOkResult()
        {
            // Arrange
            var createDto = new CreateShortUrlDto { OriginalUrl = "https://example.com" };
            var expectedResponse = new ShortUrlResponseDto
            {
                ShortUrlToken = "abcd1234",
                FullShortUrl = "http://localhost:5251/api/urls/abcd1234"
            };

            _mockService
                .Setup(s => s.CreateShortUrlAsync(createDto))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateShortUrl(createDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualResponse = Assert.IsType<ShortUrlResponseDto>(okResult.Value);
            Assert.Equal(expectedResponse.ShortUrlToken, actualResponse.ShortUrlToken);
            Assert.Equal(expectedResponse.FullShortUrl, actualResponse.FullShortUrl);
        }

        [Fact]
        public async Task CreateShortUrl_WithNullDto_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.CreateShortUrl(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Original URL cannot be empty", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task CreateShortUrl_WithEmptyOriginalUrl_ReturnsBadRequest()
        {
            // Arrange
            var createDto = new CreateShortUrlDto { OriginalUrl = "" };

            // Act
            var result = await _controller.CreateShortUrl(createDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Original URL cannot be empty", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task CreateShortUrl_WhenServiceThrowsArgumentException_ReturnsBadRequest()
        {
            // Arrange
            var createDto = new CreateShortUrlDto { OriginalUrl = "invalid-url" };

            _mockService
                .Setup(s => s.CreateShortUrlAsync(createDto))
                .ThrowsAsync(new ArgumentException("Invalid URL format."));

            // Act
            var result = await _controller.CreateShortUrl(createDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid URL format", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task CreateShortUrl_WhenServiceThrowsGeneralException_ReturnsInternalServerError()
        {
            // Arrange
            var createDto = new CreateShortUrlDto { OriginalUrl = "https://example.com" };

            _mockService
                .Setup(s => s.CreateShortUrlAsync(createDto))
                .ThrowsAsync(new Exception("Something went wrong."));

            // Act
            var result = await _controller.CreateShortUrl(createDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Contains("An error occurred while processing your request", objectResult.Value.ToString());
        }

        [Fact]
        public async Task RedirectToOriginal_WithValidShortUrl_ReturnsRedirectResult()
        {
            // Arrange
            var shortUrlToken = "abcd1234";
            var originalUrl = "https://example.com";

            var urlEntity = new UrlEntity
            {
                ShortUrl = shortUrlToken,
                OriginalUrl = originalUrl,
                ExpirationDate = DateTime.UtcNow.AddDays(1)
            };

            _mockService
                .Setup(s => s.GetByShortUrlTokenAsync(shortUrlToken))
                .ReturnsAsync(urlEntity);

            // Act
            var result = await _controller.RedirectToOriginal(shortUrlToken);

            // Assert
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal(originalUrl, redirectResult.Url);
        }

        [Fact]
        public async Task RedirectToOriginal_WithNonExistentShortUrl_ReturnsNotFoundResult()
        {
            // Arrange
            var shortUrlToken = "notfound";

            _mockService
                .Setup(s => s.GetByShortUrlTokenAsync(shortUrlToken))
                .ReturnsAsync((UrlEntity)null);

            // Act
            var result = await _controller.RedirectToOriginal(shortUrlToken);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("URL not found or expired", notFoundResult.Value.ToString());
        }
    }
}
