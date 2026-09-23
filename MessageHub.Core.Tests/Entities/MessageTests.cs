using MessageHub.Core.Entities;
using MessageHub.Core.Enums;

namespace MessageHub.Core.Tests.Entities;

public class MessageTests
{
  [Fact]
  public void Create_WithValidArguments_ReturnsPendingMessage()
  {
    var message = Message.Create("ext-1", "Hello", "sender-1", "Telegram", MessageDirection.Incoming);

    Assert.Equal(MessageStatus.Pending, message.Status);
    Assert.Equal("Hello", message.Text);
  }

  [Fact]
  public void Create_WithEmptyExternalId_Throws()
  {
    Assert.Throws<ArgumentException>(() =>
      Message.Create("", "Hello", "sender-1", "Telegram", MessageDirection.Incoming));
  }

  [Fact]
  public void MarkAsSent_FromPending_SetsStatusToSent()
  {
    var message = Message.Create("ext-1", "Hello", "sender-1", "Telegram", MessageDirection.Incoming);

    message.MarkAsSent();

    Assert.Equal(MessageStatus.Sent, message.Status);
  }

  [Fact]
  public void MarkAsSent_WhenNotPending_Throws()
  {
    var message = Message.Create("ext-1", "Hello", "sender-1", "Telegram", MessageDirection.Incoming);
    message.MarkAsSent();

    Assert.Throws<InvalidOperationException>(() => message.MarkAsSent());
  }
}
