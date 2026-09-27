using MessageHub.Core.Entities.Message.Enums;

namespace MessageHub.Core.Entities.Message;

/// <summary>
/// A message within one conversation (8.3 ТЗ, таблица messages; 7.4 ТЗ — диаграмма статусов). Messages
/// are only persisted for conversations in status <c>Added</c> (7.3 ТЗ, инвариант 4).
/// </summary>
public class Message
{
  /// <summary>
  /// Unique identifier of this message.
  /// </summary>
  public int Id { get; private set; }

  /// <summary>
  /// Id of the conversation this message belongs to.
  /// </summary>
  public int ConversationId { get; private set; }

  /// <summary>
  /// Identifier of this message in the external service. Null for an outgoing message until the
  /// service confirms it (8.3 ТЗ: CHECK Sent → external_id IS NOT NULL).
  /// </summary>
  public string? ExternalId { get; private set; }

  /// <summary>
  /// Client-generated identifier of an outgoing message, used to make a repeated send idempotent
  /// (9.5 ТЗ, ФТ-907).
  /// </summary>
  public string? ClientMessageId { get; private set; }

  /// <summary>
  /// Code of the channel this message came from or was sent through. Denormalized on purpose, to
  /// filter by channel without a join (НФТ-16) — do not remove in a future refactor.
  /// </summary>
  public string ChannelType
  {
    get;
    private set
    {
      if (string.IsNullOrWhiteSpace(value))
        throw new ArgumentException("ChannelType must not be empty.", nameof(value));
      field = value;
    }
  } = null!;

  /// <summary>
  /// Whether the message was received from, or sent to, the other side.
  /// </summary>
  public MessageDirection Direction { get; private set; }

  /// <summary>
  /// The group/broadcast participant who authored this message, if it came from a group (8.3 ТЗ).
  /// </summary>
  public int? SenderParticipantId { get; private set; }

  /// <summary>
  /// The message's text content.
  /// </summary>
  public string Text
  {
    get;
    private set
    {
      if (string.IsNullOrWhiteSpace(value))
        throw new ArgumentException("Text must not be empty.", nameof(value));
      field = value;
    }
  } = null!;

  /// <summary>
  /// JSON describing attachment types only — never their content (8.3 ТЗ, НФТ-15).
  /// </summary>
  public string? AttachmentsInfo { get; private set; }

  /// <summary>
  /// Time reported by the service, in UTC — the sort key for the timeline (НФТ-14, 9.4 ТЗ).
  /// </summary>
  public DateTime SentAt { get; private set; }

  /// <summary>
  /// When this message was saved locally, in UTC.
  /// </summary>
  public DateTime ReceivedAt { get; private set; }

  /// <summary>
  /// When the message was last edited, in UTC, if the service reported an edit.
  /// </summary>
  public DateTime? EditedAt { get; private set; }

  /// <summary>
  /// When the service reported this message as deleted, in UTC, if it did.
  /// </summary>
  public DateTime? RemoteDeletedAt { get; private set; }

  /// <summary>
  /// Current delivery status of the message.
  /// </summary>
  public MessageStatus Status { get; private set; }

  /// <summary>
  /// Whether the message has been read. Always true for outgoing messages (8.3 ТЗ, CHECK).
  /// </summary>
  public bool IsRead { get; private set; }

  /// <summary>
  /// Description of the last send error, without secrets (БЗ-02).
  /// </summary>
  public string? LastError { get; private set; }

  /// <summary>
  /// Number of failed send attempts since the message was created or last retried.
  /// </summary>
  public int RetryCount { get; private set; }

  /// <summary>
  /// When to retry sending next, in UTC. Null when no retry is scheduled (ФТ-923, НФТ-31).
  /// </summary>
  public DateTime? NextRetryAt { get; private set; }

  private Message(int id, int conversationId, string? externalId, string? clientMessageId, string channelType,
    MessageDirection direction, int? senderParticipantId, string text, string? attachmentsInfo, DateTime sentAt,
    MessageStatus status, bool isRead)
  {
    Id = id;
    ConversationId = conversationId;
    ExternalId = externalId;
    ClientMessageId = clientMessageId;
    ChannelType = channelType;
    Direction = direction;
    SenderParticipantId = senderParticipantId;
    Text = text;
    AttachmentsInfo = attachmentsInfo;
    SentAt = sentAt;
    ReceivedAt = DateTime.UtcNow;
    Status = status;
    IsRead = isRead;
  }

  /// <summary>
  /// Records an incoming message, as reported by the channel. Status is always <c>Received</c> (7.4 ТЗ).
  /// </summary>
  /// <exception cref="ArgumentException">
  /// <paramref name="conversationId"/> is empty, or <paramref name="externalId"/>/<paramref name="channelType"/>/<paramref name="text"/>
  /// is null or whitespace.
  /// </exception>
  public static Message CreateIncoming(int conversationId, string externalId, string channelType, string text,
    DateTime sentAt, int? senderParticipantId = null, string? attachmentsInfo = null)
  {
    if (conversationId < 0)
      throw new ArgumentException("ConversationId must not be empty.", nameof(conversationId));
    if (string.IsNullOrWhiteSpace(externalId))
      throw new ArgumentException("ExternalId must not be empty for an incoming message.", nameof(externalId));

    return new Message(default, conversationId, externalId, null, channelType, MessageDirection.Incoming,
      senderParticipantId, text, attachmentsInfo, sentAt, MessageStatus.Received, isRead: false);
  }

  /// <summary>
  /// Creates an outgoing message the user is sending (ФТ-704). Starts as <c>Pending</c> with no
  /// <see cref="ExternalId"/> until <see cref="MarkAsSent"/> confirms it. Outgoing messages are always
  /// read by definition (8.3 ТЗ, CHECK).
  /// </summary>
  /// <exception cref="ArgumentException">
  /// <paramref name="conversationId"/> is empty, or <paramref name="channelType"/>/<paramref name="text"/> is null or whitespace.
  /// </exception>
  public static Message CreateOutgoing(int conversationId, string channelType, string text, string? clientMessageId = null,
    string? attachmentsInfo = null)
  {
    if (conversationId < 0)
      throw new ArgumentException("ConversationId must not be empty.", nameof(conversationId));

    var now = DateTime.UtcNow;
    return new Message(default, conversationId, null, clientMessageId, channelType, MessageDirection.Outgoing,
      null, text, attachmentsInfo, now, MessageStatus.Pending, isRead: true);
  }

  /// <summary>
  /// Confirms the outgoing message was sent (ФТ-705, 7.4 ТЗ: Pending → Sent).
  /// </summary>
  /// <exception cref="InvalidOperationException">The message is not an outgoing, pending message.</exception>
  /// <exception cref="ArgumentException"><paramref name="externalId"/> is null or whitespace.</exception>
  public void MarkAsSent(string externalId, DateTime sentAt)
  {
    EnsureOutgoingPending(nameof(MarkAsSent));
    if (string.IsNullOrWhiteSpace(externalId))
      throw new ArgumentException("ExternalId must not be empty.", nameof(externalId));

    ExternalId = externalId;
    SentAt = sentAt;
    Status = MessageStatus.Sent;
    LastError = null;
    RetryCount = 0;
    NextRetryAt = null;
  }

  /// <summary>
  /// Records a failed send attempt that will be retried automatically after a temporary error (ФТ-923).
  /// The message stays <c>Pending</c> (7.4 ТЗ: Pending → Pending).
  /// </summary>
  /// <exception cref="InvalidOperationException">The message is not an outgoing, pending message.</exception>
  public void ScheduleRetry(string reason, DateTime nextRetryAt)
  {
    EnsureOutgoingPending(nameof(ScheduleRetry));

    LastError = reason;
    RetryCount++;
    NextRetryAt = nextRetryAt;
  }

  /// <summary>
  /// Marks the send as failed after retries are exhausted or a permanent error (7.4 ТЗ: Pending → Failed).
  /// </summary>
  /// <exception cref="InvalidOperationException">The message is not an outgoing, pending message.</exception>
  public void MarkAsFailed(string reason)
  {
    EnsureOutgoingPending(nameof(MarkAsFailed));

    Status = MessageStatus.Failed;
    LastError = reason;
    NextRetryAt = null;
  }

  /// <summary>
  /// Re-queues a failed message for sending, after the user retried it (ФТ-707, 7.4 ТЗ: Failed → Pending).
  /// </summary>
  /// <exception cref="InvalidOperationException">The message is not <c>Failed</c>.</exception>
  public void Retry()
  {
    if (Direction != MessageDirection.Outgoing || Status != MessageStatus.Failed)
      throw new InvalidOperationException($"Cannot retry an outgoing message from status {Status}.");

    Status = MessageStatus.Pending;
    LastError = null;
    RetryCount = 0;
    NextRetryAt = null;
  }

  /// <summary>
  /// Marks an incoming message as read (ФТ-608).
  /// </summary>
  public void MarkRead()
  {
    IsRead = true;
  }

  /// <summary>
  /// Replaces the message text after the service reports an edit.
  /// </summary>
  /// <exception cref="ArgumentException"><paramref name="newText"/> is null or whitespace.</exception>
  public void EditText(string newText)
  {
    Text = newText;
    EditedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Marks the message as deleted on the remote service.
  /// </summary>
  public void MarkRemoteDeleted()
  {
    RemoteDeletedAt = DateTime.UtcNow;
  }

  private void EnsureOutgoingPending(string operation)
  {
    if (Direction != MessageDirection.Outgoing || Status != MessageStatus.Pending)
      throw new InvalidOperationException($"Cannot {operation} a message that is not an outgoing, pending message (current status: {Status}).");
  }
}
