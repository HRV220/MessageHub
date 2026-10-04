using System.Data.Common;
using MessageHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MessageHub.Infrastructure.Tests;

/// <summary>
/// БД с применёнными миграциями — один раз на тестовый класс.
/// Соединение открывает EF (поэтому PRAGMA-интерцептор отработал) и держит открытым.
/// </summary>
public sealed class MigratedDatabaseFixture : IAsyncLifetime
{
  private readonly TempDatabase _database = new();
  private MessageHubDbContext _context = null!;

  public DbConnection Connection => _context.Database.GetDbConnection();

  public async Task InitializeAsync()
  {
    _context = _database.CreateContext();
    await _context.Database.OpenConnectionAsync();
    await _context.Database.MigrateAsync();
  }

  public async Task DisposeAsync()
  {
    await _context.DisposeAsync();
    _database.Dispose();
  }
}
