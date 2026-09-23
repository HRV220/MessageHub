namespace MessageHub.Core.Entities.Conversation.Enums;

/// <summary>
/// Kind of a conversation, as reported by the channel (8.3 ТЗ, таблица conversations).
/// </summary>
public enum ConversationType
{
  Personal,
  Group,
  Broadcast,
  Other
}
