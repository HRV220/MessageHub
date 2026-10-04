using MessageHub.Infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace MessageHub.Infrastructure.Data;

public static class DbContextOptionsBuilderExtensions
{
  /// <summary>
  /// Подключает SQLite и применяет PRAGMA при каждом открытии соединения (НФТ-13).
  /// </summary>
  public static DbContextOptionsBuilder UseMessageHubSqlite(this DbContextOptionsBuilder builder, string connectionString)
    => builder
      .UseSqlite(connectionString)
      .AddInterceptors(new SqliteConnectionInterceptor());

  /// <inheritdoc cref="UseMessageHubSqlite(DbContextOptionsBuilder, string)"/>
  public static DbContextOptionsBuilder<TContext> UseMessageHubSqlite<TContext>(this DbContextOptionsBuilder<TContext> builder, string connectionString)
    where TContext : DbContext
    => (DbContextOptionsBuilder<TContext>)UseMessageHubSqlite((DbContextOptionsBuilder)builder, connectionString);
}
