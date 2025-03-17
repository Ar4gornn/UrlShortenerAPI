using Microsoft.EntityFrameworkCore;
using UrlShortenerAPI.Data;
using UrlShortenerAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add EF Core
builder.Services.AddDbContext<UrlShortenerContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("UrlShortenerDB")));

// Add controllers
builder.Services.AddControllers();

// Register the service for DI
builder.Services.AddScoped<IUrlShortenerService, UrlShortenerService>();

var app = builder.Build();
app.UseCors("AllowAll");

app.MapControllers();
app.Run();
