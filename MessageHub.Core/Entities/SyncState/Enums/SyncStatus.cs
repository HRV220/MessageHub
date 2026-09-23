namespace MessageHub.Core.Entities.SyncState.Enums;

/// <summary>
/// Current state of a sync job tracked by a <see cref="Entities.SyncState"/> (8.3 ТЗ, таблица sync_states).
/// </summary>
public enum SyncStatus
{
  Idle,
  Running,
  Done,
  Error
}
