using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MessageHub.Infrastructure.Data.Interceptors;

internal class SqliteConnectionInterceptor : DbConnectionInterceptor
{
  private const string PragmaStatements = """
        PRAGMA foreign_keys = ON;
        PRAGMA journal_mode = WAL;
        PRAGMA synchronous = NORMAL;
        PRAGMA busy_timeout = 5000;
        """;

  public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
  {
    await using var command = connection.CreateCommand();
    command.CommandText = PragmaStatements;
    await command.ExecuteNonQueryAsync(cancellationToken);
  }

  public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
  {
    using var command = connection.CreateCommand();
    command.CommandText = PragmaStatements;
    command.ExecuteNonQuery();
  }
}
