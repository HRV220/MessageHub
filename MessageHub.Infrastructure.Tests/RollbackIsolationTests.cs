using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace MessageHub.Infrastructure.Tests;

/// <summary>
/// Тесты поведения на мигрированной БД: миграция один раз на класс (fixture),
/// каждый тест в своей транзакции, которая откатывается в <see cref="DisposeAsync"/>.
/// </summary>
public class RollbackIsolationTests(MigratedDatabaseFixture fixture) : IClassFixture<MigratedDatabaseFixture>, IAsyncLifetime
{
  private DbTransaction _transaction = null!;

  public async Task InitializeAsync() => _transaction = await fixture.Connection.BeginTransactionAsync();

  public async Task DisposeAsync()
  {
    await _transaction.RollbackAsync();
    await _transaction.DisposeAsync();
  }

  [Fact]
  public async Task Insert_MessageWithMissingConversation_RejectedByForeignKey()
  {
    var exception = await Assert.ThrowsAsync<SqliteException>(() => Execute("""
      INSERT INTO messages (conversation_id, channel_type, direction, text, sent_at, received_at, status)
      VALUES (999, 'telegram', 'incoming', 'hi', 0, 0, 'received')
      """));

    Assert.Equal(787, exception.SqliteExtendedErrorCode); // SQLITE_CONSTRAINT_FOREIGNKEY
  }

  [Fact]
  public async Task Insert_UserProfileFirstTest_IsRolledBackAfterTest() => await InsertUserProfileAndAssertSingleRow();

  [Fact]
  public async Task Insert_UserProfileSecondTest_IsRolledBackAfterTest() => await InsertUserProfileAndAssertSingleRow();

  // Оба теста вставляют строку с id=1: если откат между тестами сломан, второй упадёт на PK.
  private async Task InsertUserProfileAndAssertSingleRow()
  {
    await Execute("""
      INSERT INTO user_profiles (id, username, password_hash, password_salt, kdf_params, created_at, updated_at)
      VALUES (1, 'user', x'00', x'00', '{}', 0, 0)
      """);

    await using var command = fixture.Connection.CreateCommand();
    command.Transaction = _transaction;
    command.CommandText = "SELECT COUNT(*) FROM user_profiles";

    Assert.Equal(1L, await command.ExecuteScalarAsync());
  }

  private async Task Execute(string sql)
  {
    await using var command = fixture.Connection.CreateCommand();
    command.Transaction = _transaction;
    command.CommandText = sql;
    await command.ExecuteNonQueryAsync();
  }
}
