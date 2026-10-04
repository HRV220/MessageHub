using System.Data.Common;
using MessageHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MessageHub.Infrastructure.Tests;

/// <summary>
/// Тесты схемы/миграций/PRAGMA (НФТ-12, НФТ-13) — каждому нужна свежая БД, поэтому без общего fixture.
/// </summary>
public class MigrationAndPragmaTests
{
  private static readonly string[] ExpectedTables =
  [
    "channel_contacts", "connected_accounts", "conversation_participants", "conversations",
    "merge_proposals", "messages", "person", "relation_change_logs", "settings",
    "sync_states", "user_profiles"
  ];

  [Fact]
  public async Task Migrate_CleanDatabase_CreatesAllTables()
  {
    using var database = new TempDatabase();
    await using var context = database.CreateContext();

    await context.Database.MigrateAsync();

    var tables = await QueryStrings(context, "SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' AND name NOT LIKE '!_!_EF%' ESCAPE '!' ORDER BY name");
    Assert.Equal(ExpectedTables, tables);
  }

  [Fact]
  public async Task Migrate_CleanDatabase_AppliesAllMigrationsAndLeavesNonePending()
  {
    using var database = new TempDatabase();
    await using var context = database.CreateContext();

    await context.Database.MigrateAsync();

    Assert.Empty(await context.Database.GetPendingMigrationsAsync());
    Assert.Equal(context.Database.GetMigrations(), await context.Database.GetAppliedMigrationsAsync());
  }

  [Fact]
  public async Task Migrate_CalledTwice_IsIdempotent()
  {
    using var database = new TempDatabase();
    await using var context = database.CreateContext();
    await context.Database.MigrateAsync();

    await context.Database.MigrateAsync();

    Assert.Empty(await context.Database.GetPendingMigrationsAsync());
  }

  [Fact]
  public async Task Migrate_CleanDatabase_ProducesNoForeignKeyViolations()
  {
    using var database = new TempDatabase();
    await using var context = database.CreateContext();
    await context.Database.MigrateAsync();

    var violations = await QueryStrings(context, "SELECT \"table\" FROM pragma_foreign_key_check");

    Assert.Empty(violations);
  }

  [Fact]
  public async Task OpenConnection_AfterMigration_EnablesForeignKeys()
  {
    using var database = new TempDatabase();
    await using var context = database.CreateContext();
    await context.Database.MigrateAsync();

    Assert.Equal(1L, await Pragma(context, "foreign_keys"));
  }

  [Fact]
  public async Task OpenConnection_AfterMigration_UsesWalJournal()
  {
    using var database = new TempDatabase();
    await using var context = database.CreateContext();
    await context.Database.MigrateAsync();

    Assert.Equal("wal", await Pragma(context, "journal_mode"));
  }

  [Fact]
  public async Task OpenConnection_AfterMigration_SetsSynchronousNormal()
  {
    using var database = new TempDatabase();
    await using var context = database.CreateContext();
    await context.Database.MigrateAsync();

    Assert.Equal(1L, await Pragma(context, "synchronous"));
  }

  [Fact]
  public async Task OpenConnection_AfterMigration_SetsBusyTimeout()
  {
    using var database = new TempDatabase();
    await using var context = database.CreateContext();
    await context.Database.MigrateAsync();

    Assert.Equal(5000L, await Pragma(context, "busy_timeout"));
  }

  [Fact]
  public async Task OpenConnection_NewConnectionToExistingDatabase_AppliesPerConnectionPragmas()
  {
    using var database = new TempDatabase();
    await using (var first = database.CreateContext())
    {
      await first.Database.MigrateAsync();
    }

    await using var second = database.CreateContext();

    Assert.Equal(1L, await Pragma(second, "foreign_keys"));
    Assert.Equal(1L, await Pragma(second, "synchronous"));
    Assert.Equal(5000L, await Pragma(second, "busy_timeout"));
    Assert.Equal("wal", await Pragma(second, "journal_mode"));
  }

  private static async Task<object?> Pragma(MessageHubDbContext context, string name)
  {
    await context.Database.OpenConnectionAsync();
    await using var command = context.Database.GetDbConnection().CreateCommand();
    command.CommandText = $"PRAGMA {name};";
    return await command.ExecuteScalarAsync();
  }

  private static async Task<List<string>> QueryStrings(MessageHubDbContext context, string sql)
  {
    await context.Database.OpenConnectionAsync();
    await using DbCommand command = context.Database.GetDbConnection().CreateCommand();
    command.CommandText = sql;
    await using var reader = await command.ExecuteReaderAsync();

    var result = new List<string>();
    while (await reader.ReadAsync())
    {
      result.Add(reader.GetString(0));
    }

    return result;
  }
}
