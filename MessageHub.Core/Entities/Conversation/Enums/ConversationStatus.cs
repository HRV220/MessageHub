namespace MessageHub.Core.Entities.Conversation.Enums;

/// <summary>
/// Lifecycle status of a persisted <see cref="Entities.Conversation"/> (7.4 ТЗ).
/// <c>Candidate</c> exists only in memory during the selection screen (US-08) and is never persisted.
/// </summary>
public enum ConversationStatus
{
  /// <summary>Appeared after the account was connected or during sync; only metadata is kept.</summary>
  Pending,

  /// <summary>Added by the user; messages are synchronized.</summary>
  Added,

  /// <summary>Ignored or removed from the app.</summary>
  Ignored
}
