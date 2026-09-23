namespace MessageHub.Core.Entities;

/// <summary>
/// A participant of a group or broadcast <see cref="Conversation"/>, as reported by the channel.
/// Not a collocutor (7.1 ТЗ: "не является собеседником") — it becomes linked to a
/// <see cref="ChannelContact"/> only when the user explicitly adds it as one.
/// </summary>
public class ConversationParticipant
{
  /// <summary>
  /// Unique identifier of this participant.
  /// </summary>
  public Guid Id { get; private set; }

  /// <summary>
  /// Id of the conversation this participant belongs to.
  /// </summary>
  public Guid ConversationId { get; private set; }

  /// <summary>
  /// Identifier of this participant in the external service.
  /// </summary>
  public string ExternalId
  {
    get;
    private set
    {
      if (string.IsNullOrWhiteSpace(value))
        throw new ArgumentException("ExternalId must not be empty.", nameof(value));
      field = value;
    }
  } = null!;

  /// <summary>
  /// Display name reported by the service, if any.
  /// </summary>
  public string? DisplayName { get; private set; }

  /// <summary>
  /// Id of the <see cref="ChannelContact"/> this participant was linked to, filled in only if the user
  /// explicitly added this participant as a collocutor.
  /// </summary>
  public Guid? ChannelContactId { get; private set; }

  private ConversationParticipant(Guid id, Guid conversationId, string externalId, string? displayName)
  {
    Id = id;
    ConversationId = conversationId;
    ExternalId = externalId;
    DisplayName = displayName;
  }

  /// <summary>
  /// Creates a participant for a group or broadcast conversation.
  /// </summary>
  /// <exception cref="ArgumentException"><paramref name="conversationId"/> is empty, or <paramref name="externalId"/> is null or whitespace.</exception>
  public static ConversationParticipant Create(Guid conversationId, string externalId, string? displayName = null)
  {
    if (conversationId == Guid.Empty)
      throw new ArgumentException("ConversationId must not be empty.", nameof(conversationId));

    return new ConversationParticipant(Guid.NewGuid(), conversationId, externalId, displayName);
  }

  /// <summary>
  /// Refreshes the display name the service reports for this participant.
  /// </summary>
  public void UpdateProfile(string? displayName)
  {
    DisplayName = displayName;
  }

  /// <summary>
  /// Links this participant to a collocutor, after the user explicitly added it as one.
  /// </summary>
  /// <exception cref="ArgumentException"><paramref name="channelContactId"/> is empty.</exception>
  public void LinkToChannelContact(Guid channelContactId)
  {
    if (channelContactId == Guid.Empty)
      throw new ArgumentException("ChannelContactId must not be empty.", nameof(channelContactId));

    ChannelContactId = channelContactId;
  }
}
