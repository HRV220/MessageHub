using MessageHub.Core.Enums;

namespace MessageHub.Core.Entities;

public class Message
{
  public Guid Id { get; private set; }
  public string ExternalId { get; private set; }

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

  public string Sender { get; private set; }
  public DateTime SendedAt { get; private set; }
  public string ChannelName { get; private set; }
  public MessageStatus Status { get; private set; }
  public MessageDirection Direction { get; private set; }

  private Message(Guid id, string externalId, string text, string sender, DateTime sendedAt, string channelName, MessageStatus status, MessageDirection direction)
  {
    Id = id;
    ExternalId = externalId;
    Sender = sender;
    SendedAt = sendedAt;
    ChannelName = channelName;
    Status = status;
    Direction = direction;
    Text = text;
  }

  /// <summary>
  /// Creates a new message with <see cref="MessageStatus.Pending"/> status.
  /// </summary>
  /// <exception cref="ArgumentException">Any required field is null or whitespace.</exception>
  public static Message Create(string externalId, string text, string sender, string channelName, MessageDirection direction)
  {
    if (string.IsNullOrWhiteSpace(externalId))
      throw new ArgumentException("ExternalId must not be empty.", nameof(externalId));
    if (string.IsNullOrWhiteSpace(sender))
      throw new ArgumentException("Sender must not be empty.", nameof(sender));
    if (string.IsNullOrWhiteSpace(channelName))
      throw new ArgumentException("ChannelName must not be empty.", nameof(channelName));

    return new Message(Guid.NewGuid(), externalId, text, sender, DateTime.UtcNow, channelName, MessageStatus.Pending, direction);
  }

  /// <summary>
  /// Transitions the message from <see cref="MessageStatus.Pending"/> to <see cref="MessageStatus.Sent"/>.
  /// </summary>
  /// <exception cref="InvalidOperationException">The message is not currently pending.</exception>
  public void MarkAsSent()
  {
    if (Status != MessageStatus.Pending)
      throw new InvalidOperationException($"Cannot mark message as sent from status {Status}.");
    Status = MessageStatus.Sent;
  }

  /// <summary>
  /// Transitions the message from <see cref="MessageStatus.Sent"/> to <see cref="MessageStatus.Delivered"/>.
  /// </summary>
  /// <exception cref="InvalidOperationException">The message has not been sent yet.</exception>
  public void MarkAsDelivered()
  {
    if (Status != MessageStatus.Sent)
      throw new InvalidOperationException($"Cannot mark message as delivered from status {Status}.");
    Status = MessageStatus.Delivered;
  }

  /// <summary>
  /// Transitions the message from <see cref="MessageStatus.Delivered"/> to <see cref="MessageStatus.Read"/>.
  /// </summary>
  /// <exception cref="InvalidOperationException">The message has not been delivered yet.</exception>
  public void MarkAsRead()
  {
    if (Status != MessageStatus.Delivered)
      throw new InvalidOperationException($"Cannot mark message as read from status {Status}.");
    Status = MessageStatus.Read;
  }

  /// <summary>
  /// Marks the message as <see cref="MessageStatus.Failed"/>.
  /// </summary>
  /// <exception cref="InvalidOperationException">The message has already been read.</exception>
  public void MarkAsFailed()
  {
    if (Status == MessageStatus.Read)
      throw new InvalidOperationException("Cannot mark an already read message as failed.");
    Status = MessageStatus.Failed;
  }

  /// <summary>
  /// Replaces the message text.
  /// </summary>
  /// <exception cref="ArgumentException"><paramref name="newText"/> is null or whitespace.</exception>
  public void EditText(string newText)
  {
    Text = newText;
  }
}