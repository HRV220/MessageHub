using MessageHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MessageHub.Infrastructure.Tests;

/// <summary>
/// Файловая SQLite-БД во временной папке: WAL на :memory: не работает, а
/// <c>DatabasePath</c> (LOCALAPPDATA) тестам трогать нельзя.
/// </summary>
internal sealed class TempDatabase : IDisposable
{
  private readonly string _directory = Path.Combine(Path.GetTempPath(), "MessageHubTests", Guid.NewGuid().ToString("N"));

  public TempDatabase()
  {
    Directory.CreateDirectory(_directory);
  }

  // Pooling=False — каждое открытие реально новое соединение и файл освобождается при Dispose.
  public string ConnectionString => $"Data Source={Path.Combine(_directory, "test.db")};Pooling=False";

  public MessageHubDbContext CreateContext()
  {
    var options = new DbContextOptionsBuilder<MessageHubDbContext>()
      .UseMessageHubSqlite(ConnectionString)
      .Options;

    return new MessageHubDbContext(options);
  }

  public void Dispose()
  {
    try
    {
      Directory.Delete(_directory, recursive: true);
    }
    catch (IOException)
    {
      // Временная папка — best effort, не валим тест из-за очистки.
    }
  }
}
