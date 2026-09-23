using MessageHub.Core.Entities;
using MessageHub.Core.Entities.Conversation.Enums;

namespace MessageHub.Core.Entities.Conversation;

/// <summary>
/// A dialog in one channel: a personal chat, group, broadcast, or other kind reported by the adapter
/// (7.1 ТЗ). Owns its <see cref="ConversationParticipant"/>s (group/broadcast members).
/// </summary>
public class Conversation
{
  /// <summary>
  /// Unique identifier of this conversation.
  /// </summary>
  public Guid Id { get; private set; }

  /// <summary>
  /// The connected account this conversation was seen through.
  /// </summary>
  public Guid ConnectedAccountId { get; private set; }

  /// <summary>
  /// Identifier of this conversation in the external service.
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
  /// Kind of conversation.
  /// </summary>
  public ConversationType Type { get; private set; }

  /// <summary>
  /// Lifecycle status (7.4 ТЗ).
  /// </summary>
  public ConversationStatus Status { get; private set; }

  /// <summary>
  /// Title of the conversation, for groups and broadcasts.
  /// </summary>
  public string? Title { get; private set; }

  /// <summary>
  /// The collocutor's channel identity, for a <see cref="ConversationType.Personal"/> conversation.
  /// Required when <see cref="Type"/> is <see cref="ConversationType.Personal"/> and <see cref="Status"/>
  /// is <see cref="ConversationStatus.Added"/>; must be null otherwise (8.3 ТЗ, таблица conversations, CHECK).
  /// </summary>
  public Guid? ChannelContactId { get; private set; }

  /// <summary>
  /// Time of the most recent message in this conversation, in UTC. Denormalized from messages.
  /// </summary>
  public DateTime? LastMessageAt { get; private set; }

  /// <summary>
  /// Time of the most recent incoming message, in UTC. Used to pick the default reply channel.
  /// </summary>
  public DateTime? LastIncomingAt { get; private set; }

  /// <summary>
  /// Short preview of the most recent message, shown in list views.
  /// </summary>
  public string? LastMessagePreview { get; private set; }

  /// <summary>
  /// Number of unread messages in this conversation.
  /// </summary>
  public int UnreadCount { get; private set; }

  /// <summary>
  /// When the conversation last transitioned to <see cref="ConversationStatus.Added"/>, in UTC.
  /// </summary>
  public DateTime? AddedAt { get; private set; }

  /// <summary>
  /// When this conversation was first seen, in UTC.
  /// </summary>
  public DateTime CreatedAt { get; private set; }

  /// <summary>
  /// When this conversation was last updated, in UTC.
  /// </summary>
  public DateTime UpdatedAt { get; private set; }

  private readonly List<ConversationParticipant> _participants = [];

  /// <summary>
  /// Members of this conversation, for group and broadcast conversations (conversation_participants.conversation_id → conversations.id).
  /// </summary>
  public IReadOnlyList<ConversationParticipant> Participants => _participants;

  private Conversation(Guid id, Guid connectedAccountId, string externalId, ConversationType type, ConversationStatus status, string? title, Guid? channelContactId)
  {
    Id = id;
    ConnectedAccountId = connectedAccountId;
    ExternalId = externalId;
    Type = type;
    Status = status;
    Title = title;
    ChannelContactId = channelContactId;
    CreatedAt = DateTime.UtcNow;
    UpdatedAt = CreatedAt;
    AddedAt = status == ConversationStatus.Added ? CreatedAt : null;
  }

  /// <summary>
  /// Creates a conversation. <paramref name="status"/> defaults to <see cref="ConversationStatus.Pending"/>
  /// (появился после подключения/синка, только метаданные, 7.4 ТЗ) — pass
  /// <see cref="ConversationStatus.Added"/> when the user added it directly from the candidate screen
  /// (US-08, 9.2 ТЗ).
  /// </summary>
  /// <exception cref="ArgumentException">
  /// <paramref name="connectedAccountId"/> is empty, <paramref name="externalId"/> is null or whitespace,
  /// or <paramref name="channelContactId"/> is inconsistent with <paramref name="type"/> and <paramref name="status"/>.
  /// </exception>
  public static Conversation Create(Guid connectedAccountId, string externalId, ConversationType type, ConversationStatus status = ConversationStatus.Pending, string? title = null, Guid? channelContactId = null)
  {
    if (connectedAccountId == Guid.Empty)
      throw new ArgumentException("ConnectedAccountId must not be empty.", nameof(connectedAccountId));

    ValidateChannelContact(type, status, channelContactId);

    return new Conversation(Guid.NewGuid(), connectedAccountId, externalId, type, status, title, channelContactId);
  }

  private static void ValidateChannelContact(ConversationType type, ConversationStatus status, Guid? channelContactId)
  {
    if (type == ConversationType.Personal && status == ConversationStatus.Added && channelContactId is null)
      throw new ArgumentException("ChannelContactId is required for an added personal conversation.", nameof(channelContactId));

    if (type != ConversationType.Personal && channelContactId is not null)
      throw new ArgumentException("ChannelContactId must be null for a non-personal conversation.", nameof(channelContactId));
  }

  /// <summary>
  /// Adds the conversation to the app (US-08, ФТ-206). Valid from <see cref="ConversationStatus.Pending"/>
  /// or <see cref="ConversationStatus.Ignored"/> (restore).
  /// </summary>
  /// <exception cref="ArgumentException">
  /// This is a <see cref="ConversationType.Personal"/> conversation and neither <paramref name="channelContactId"/>
  /// nor an already-set <see cref="ChannelContactId"/> is available.
  /// </exception>
  /// <exception cref="InvalidOperationException">The conversation is already <see cref="ConversationStatus.Added"/>.</exception>
  public void Add(Guid? channelContactId = null)
  {
    if (Status == ConversationStatus.Added)
      throw new InvalidOperationException("Conversation is already added.");

    var effectiveContactId = channelContactId ?? ChannelContactId;
    ValidateChannelContact(Type, ConversationStatus.Added, effectiveContactId);

    ChannelContactId = effectiveContactId;
    Status = ConversationStatus.Added;
    AddedAt = DateTime.UtcNow;
    UpdatedAt = AddedAt.Value;
  }

  /// <summary>
  /// Removes the conversation from the app (ФТ-207 аналог для диалогов; "убрать из приложения", 7.4 ТЗ).
  /// Valid from <see cref="ConversationStatus.Pending"/> or <see cref="ConversationStatus.Added"/>.
  /// </summary>
  /// <exception cref="InvalidOperationException">The conversation is already <see cref="ConversationStatus.Ignored"/>.</exception>
  public void Ignore()
  {
    if (Status == ConversationStatus.Ignored)
      throw new InvalidOperationException("Conversation is already ignored.");

    Status = ConversationStatus.Ignored;
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Updates the denormalized preview fields after a new message arrives.
  /// </summary>
  public void RecordMessage(DateTime sentAt, string? preview, bool isIncoming)
  {
    LastMessageAt = sentAt;
    LastMessagePreview = preview;
    if (isIncoming)
    {
      LastIncomingAt = sentAt;
      UnreadCount++;
    }
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Resets the unread counter, e.g. when the user opens the conversation.
  /// </summary>
  public void MarkRead()
  {
    UnreadCount = 0;
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Adds a group/broadcast member, as reported by the channel.
  /// </summary>
  /// <exception cref="ArgumentException"><paramref name="externalId"/> is null or whitespace.</exception>
  /// <exception cref="InvalidOperationException">A participant with the same <paramref name="externalId"/> already exists.</exception>
  public ConversationParticipant AddParticipant(string externalId, string? displayName = null)
  {
    if (_participants.Any(p => p.ExternalId == externalId))
      throw new InvalidOperationException("A participant with this external id already exists in this conversation.");

    var participant = ConversationParticipant.Create(Id, externalId, displayName);
    _participants.Add(participant);
    UpdatedAt = DateTime.UtcNow;
    return participant;
  }

  /// <summary>
  /// Removes a group/broadcast member, e.g. after they leave the group.
  /// </summary>
  public void RemoveParticipant(Guid participantId)
  {
    _participants.RemoveAll(p => p.Id == participantId);
    UpdatedAt = DateTime.UtcNow;
  }
}
