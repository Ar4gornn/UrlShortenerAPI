using Microsoft.EntityFrameworkCore;
using UrlShortenerAPI.Data;
using UrlShortenerAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure EF Core with PostgreSQL
builder.Services.AddDbContext<UrlShortenerContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("UrlShortenerDB")));

// Build the app
var app = builder.Build();

// Map the POST endpoint to shorten a URL
// Expects a JSON body of the form { "originalUrl": "https://...." }
app.MapPost("/api/urls", async (UrlRequest request, UrlShortenerContext db) =>
{
    if (!Uri.IsWellFormedUriString(request.OriginalUrl, UriKind.Absolute))
    {
        return Results.BadRequest("Invalid URL");
    }

    var shortToken = GenerateShortToken();
    var entity = new UrlEntity
    {
        ShortUrl = shortToken,
        OriginalUrl = request.OriginalUrl
    };

    db.Urls.Add(entity);
    await db.SaveChangesAsync();

    return Results.Ok(new { ShortUrl = shortToken });
});

// Map the GET endpoint to retrieve the original URL and perform a redirect
app.MapGet("/api/urls/{shortUrl}", async (string shortUrl, UrlShortenerContext db) =>
{
    var entity = await db.Urls.SingleOrDefaultAsync(u => u.ShortUrl == shortUrl);

    if (entity == null)
    {
        return Results.NotFound("URL not found");
    }

    return Results.Redirect(entity.OriginalUrl);
});

app.Run();

static string GenerateShortToken()
{
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    var random = new char[6];
    var rng = new Random();

    for (int i = 0; i < random.Length; i++)
    {
        random[i] = chars[rng.Next(chars.Length)];
    }

    return new string(random);
}

public record UrlRequest(string OriginalUrl);
