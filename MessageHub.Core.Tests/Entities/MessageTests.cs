using MessageHub.Core.Entities.Message;
using MessageHub.Core.Entities.Message.Enums;

namespace MessageHub.Core.Tests.Entities;

public class MessageTests
{
  private const int ConversationId = 1;

  [Fact]
  public void CreateIncoming_WithValidArguments_ReturnsReceivedUnreadMessage()
  {
    var message = Message.CreateIncoming(ConversationId, "ext-1", "telegram", "Hello", DateTime.UtcNow);

    Assert.Equal(MessageStatus.Received, message.Status);
    Assert.False(message.IsRead);
    Assert.Equal("Hello", message.Text);
  }

  [Fact]
  public void CreateIncoming_WithEmptyExternalId_Throws()
  {
    Assert.Throws<ArgumentException>(() =>
      Message.CreateIncoming(ConversationId, "", "telegram", "Hello", DateTime.UtcNow));
  }

  [Fact]
  public void CreateOutgoing_WithValidArguments_ReturnsPendingReadMessage()
  {
    var message = Message.CreateOutgoing(ConversationId, "telegram", "Hello");

    Assert.Equal(MessageStatus.Pending, message.Status);
    Assert.True(message.IsRead);
    Assert.Null(message.ExternalId);
  }

  [Fact]
  public void MarkAsSent_FromPending_SetsStatusToSentAndSetsExternalId()
  {
    var message = Message.CreateOutgoing(ConversationId, "telegram", "Hello");

    message.MarkAsSent("ext-1", DateTime.UtcNow);

    Assert.Equal(MessageStatus.Sent, message.Status);
    Assert.Equal("ext-1", message.ExternalId);
  }

  [Fact]
  public void MarkAsSent_WhenNotPending_Throws()
  {
    var message = Message.CreateOutgoing(ConversationId, "telegram", "Hello");
    message.MarkAsSent("ext-1", DateTime.UtcNow);

    Assert.Throws<InvalidOperationException>(() => message.MarkAsSent("ext-2", DateTime.UtcNow));
  }

  [Fact]
  public void MarkAsSent_OnIncomingMessage_Throws()
  {
    var message = Message.CreateIncoming(ConversationId, "ext-1", "telegram", "Hello", DateTime.UtcNow);

    Assert.Throws<InvalidOperationException>(() => message.MarkAsSent("ext-2", DateTime.UtcNow));
  }

  [Fact]
  public void ScheduleRetry_FromPending_IncrementsRetryCountAndStaysPending()
  {
    var message = Message.CreateOutgoing(ConversationId, "telegram", "Hello");

    message.ScheduleRetry("timeout", DateTime.UtcNow.AddMinutes(1));

    Assert.Equal(MessageStatus.Pending, message.Status);
    Assert.Equal(1, message.RetryCount);
  }

  [Fact]
  public void MarkAsFailed_FromPending_SetsStatusToFailed()
  {
    var message = Message.CreateOutgoing(ConversationId, "telegram", "Hello");

    message.MarkAsFailed("permanent error");

    Assert.Equal(MessageStatus.Failed, message.Status);
  }

  [Fact]
  public void Retry_FromFailed_ResetsToPending()
  {
    var message = Message.CreateOutgoing(ConversationId, "telegram", "Hello");
    message.MarkAsFailed("permanent error");

    message.Retry();

    Assert.Equal(MessageStatus.Pending, message.Status);
    Assert.Equal(0, message.RetryCount);
  }

  [Fact]
  public void Retry_WhenNotFailed_Throws()
  {
    var message = Message.CreateOutgoing(ConversationId, "telegram", "Hello");

    Assert.Throws<InvalidOperationException>(() => message.Retry());
  }
}
