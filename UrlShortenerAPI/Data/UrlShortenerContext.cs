using Microsoft.EntityFrameworkCore;
using UrlShortenerAPI.Models;

namespace UrlShortenerAPI.Data
{
    /// <summary>
    /// EF Core database context for the URL Shortener application.
    /// </summary>
    public class UrlShortenerContext : DbContext
    {
        public UrlShortenerContext(DbContextOptions<UrlShortenerContext> options)
            : base(options)
        {
        }

        public DbSet<UrlEntity> Urls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<UrlEntity>().ToTable("urls");

    modelBuilder.Entity<UrlEntity>()
        .Property(u => u.ShortUrl)
        .HasColumnName("short_url"); 

    modelBuilder.Entity<UrlEntity>()
        .Property(u => u.OriginalUrl)
        .HasColumnName("original_url");

    modelBuilder.Entity<UrlEntity>()
        .HasIndex(u => u.ShortUrl)
        .IsUnique();
}

    }
}
