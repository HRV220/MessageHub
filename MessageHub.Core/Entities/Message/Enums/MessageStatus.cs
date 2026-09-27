namespace MessageHub.Core.Entities.Message.Enums;

/// <summary>
/// Delivery status of a message (8.3 ТЗ, таблица messages; 7.4 ТЗ — диаграмма статусов).
/// </summary>
public enum MessageStatus
{
  /// <summary>Incoming message (7.4 ТЗ: "Входящие сообщения имеют статус Received").</summary>
  Received,

  /// <summary>Outgoing, not yet confirmed by the service.</summary>
  Pending,

  /// <summary>Outgoing, confirmed by the service.</summary>
  Sent,

  /// <summary>Outgoing, sending failed (retries exhausted, or a permanent error).</summary>
  Failed
}
