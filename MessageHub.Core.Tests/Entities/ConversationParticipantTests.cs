using MessageHub.Core.Entities;

namespace MessageHub.Core.Tests.Entities;

public class ConversationParticipantTests
{
  [Fact]
  public void Create_WithValidArguments_ReturnsUnlinkedParticipant()
  {
    var participant = ConversationParticipant.Create(1, "ext-1", "Ivan");

    Assert.Equal("Ivan", participant.DisplayName);
    Assert.Null(participant.ChannelContactId);
  }

  [Fact]
  public void Create_WithEmptyConversationId_Throws()
  {
    Assert.Throws<ArgumentException>(() => ConversationParticipant.Create(-1, "ext-1"));
  }

  [Fact]
  public void Create_WithEmptyExternalId_Throws()
  {
    Assert.Throws<ArgumentException>(() => ConversationParticipant.Create(1, ""));
  }

  [Fact]
  public void LinkToChannelContact_SetsChannelContactId()
  {
    var participant = ConversationParticipant.Create(1, "ext-1");
    var channelContactId = 2;

    participant.LinkToChannelContact(channelContactId);

    Assert.Equal(channelContactId, participant.ChannelContactId);
  }

  [Fact]
  public void LinkToChannelContact_WithEmptyId_Throws()
  {
    var participant = ConversationParticipant.Create(1, "ext-1");

    Assert.Throws<ArgumentException>(() => participant.LinkToChannelContact(-1));
  }
}
