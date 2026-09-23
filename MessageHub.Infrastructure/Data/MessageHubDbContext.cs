using MessageHub.Infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace MessageHub.Infrastructure.Data;

public class MessageHubDbContext : DbContext
{
  public MessageHubDbContext(DbContextOptions<MessageHubDbContext> options) : base(options)
  {
  }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    if (!optionsBuilder.IsConfigured)
    {
      optionsBuilder
        .UseSqlite($"Data Source={DatabasePath.Get()}")
        .AddInterceptors(new SqliteConnectionInterceptor());
    }
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
  }
}
