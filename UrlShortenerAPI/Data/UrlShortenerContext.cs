using Microsoft.EntityFrameworkCore;
using UrlShortenerAPI.Models;

namespace UrlShortenerAPI.Data
{
    public class UrlShortenerContext : DbContext
    {
        public UrlShortenerContext(DbContextOptions<UrlShortenerContext> options)
            : base(options)
        {
        }

        public DbSet<UrlEntity> Urls => Set<UrlEntity>();
    }
}