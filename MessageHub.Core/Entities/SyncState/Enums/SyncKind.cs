namespace MessageHub.Core.Entities.SyncState.Enums;

/// <summary>
/// What a <see cref="Entities.SyncState"/> tracks the cursor for (8.3 ТЗ, таблица sync_states).
/// </summary>
public enum SyncKind
{
  /// <summary>Account-level cursor for incremental updates. Not tied to a conversation.</summary>
  Updates,

  /// <summary>Initial history load for a single conversation.</summary>
  History
}
