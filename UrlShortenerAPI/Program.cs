using Microsoft.EntityFrameworkCore;
using UrlShortenerAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

// Configure EF Core with PostgreSQL
builder.Services.AddDbContext<UrlShortenerContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("UrlShortenerDB")));

// Build the app
var app = builder.Build();

// Map controllers
app.MapControllers();

// Run the web application
app.Run();