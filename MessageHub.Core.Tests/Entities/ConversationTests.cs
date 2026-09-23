using MessageHub.Core.Entities;
using MessageHub.Core.Entities.Conversation;
using MessageHub.Core.Entities.Conversation.Enums;

namespace MessageHub.Core.Tests.Entities;

public class ConversationTests
{
  [Fact]
  public void Create_WithoutStatus_ReturnsPendingConversation()
  {
    var conversation = Conversation.Create(Guid.NewGuid(), "ext-1", ConversationType.Group);

    Assert.Equal(ConversationStatus.Pending, conversation.Status);
    Assert.Null(conversation.AddedAt);
  }

  [Fact]
  public void Create_WithEmptyConnectedAccountId_Throws()
  {
    Assert.Throws<ArgumentException>(() => Conversation.Create(Guid.Empty, "ext-1", ConversationType.Group));
  }

  [Fact]
  public void Create_PersonalAdded_WithoutChannelContactId_Throws()
  {
    Assert.Throws<ArgumentException>(() =>
      Conversation.Create(Guid.NewGuid(), "ext-1", ConversationType.Personal, ConversationStatus.Added));
  }

  [Fact]
  public void Create_PersonalAdded_WithChannelContactId_ReturnsAddedConversation()
  {
    var conversation = Conversation.Create(Guid.NewGuid(), "ext-1", ConversationType.Personal, ConversationStatus.Added, channelContactId: Guid.NewGuid());

    Assert.Equal(ConversationStatus.Added, conversation.Status);
    Assert.NotNull(conversation.AddedAt);
  }

  [Fact]
  public void Create_NonPersonal_WithChannelContactId_Throws()
  {
    Assert.Throws<ArgumentException>(() =>
      Conversation.Create(Guid.NewGuid(), "ext-1", ConversationType.Group, channelContactId: Guid.NewGuid()));
  }

  [Fact]
  public void Add_FromPending_SetsStatusToAdded()
  {
    var conversation = Conversation.Create(Guid.NewGuid(), "ext-1", ConversationType.Group);

    conversation.Add();

    Assert.Equal(ConversationStatus.Added, conversation.Status);
    Assert.NotNull(conversation.AddedAt);
  }

  [Fact]
  public void Add_WhenAlreadyAdded_Throws()
  {
    var conversation = Conversation.Create(Guid.NewGuid(), "ext-1", ConversationType.Group);
    conversation.Add();

    Assert.Throws<InvalidOperationException>(() => conversation.Add());
  }

  [Fact]
  public void Add_PersonalWithoutChannelContactId_Throws()
  {
    var conversation = Conversation.Create(Guid.NewGuid(), "ext-1", ConversationType.Personal);

    Assert.Throws<ArgumentException>(() => conversation.Add());
  }

  [Fact]
  public void Add_PersonalWithChannelContactId_SetsChannelContactId()
  {
    var conversation = Conversation.Create(Guid.NewGuid(), "ext-1", ConversationType.Personal);
    var channelContactId = Guid.NewGuid();

    conversation.Add(channelContactId);

    Assert.Equal(channelContactId, conversation.ChannelContactId);
  }

  [Fact]
  public void Ignore_FromPending_SetsStatusToIgnored()
  {
    var conversation = Conversation.Create(Guid.NewGuid(), "ext-1", ConversationType.Group);

    conversation.Ignore();

    Assert.Equal(ConversationStatus.Ignored, conversation.Status);
  }

  [Fact]
  public void Ignore_WhenAlreadyIgnored_Throws()
  {
    var conversation = Conversation.Create(Guid.NewGuid(), "ext-1", ConversationType.Group);
    conversation.Ignore();

    Assert.Throws<InvalidOperationException>(() => conversation.Ignore());
  }

  [Fact]
  public void Add_AfterIgnore_SetsStatusToAdded()
  {
    var conversation = Conversation.Create(Guid.NewGuid(), "ext-1", ConversationType.Group);
    conversation.Ignore();

    conversation.Add();

    Assert.Equal(ConversationStatus.Added, conversation.Status);
  }

  [Fact]
  public void RecordMessage_Incoming_IncrementsUnreadCount()
  {
    var conversation = Conversation.Create(Guid.NewGuid(), "ext-1", ConversationType.Group);
    var sentAt = DateTime.UtcNow;

    conversation.RecordMessage(sentAt, "Hello", isIncoming: true);

    Assert.Equal(1, conversation.UnreadCount);
    Assert.Equal(sentAt, conversation.LastMessageAt);
    Assert.Equal(sentAt, conversation.LastIncomingAt);
    Assert.Equal("Hello", conversation.LastMessagePreview);
  }

  [Fact]
  public void RecordMessage_Outgoing_DoesNotIncrementUnreadCount()
  {
    var conversation = Conversation.Create(Guid.NewGuid(), "ext-1", ConversationType.Group);

    conversation.RecordMessage(DateTime.UtcNow, "Hello", isIncoming: false);

    Assert.Equal(0, conversation.UnreadCount);
  }

  [Fact]
  public void MarkRead_ResetsUnreadCount()
  {
    var conversation = Conversation.Create(Guid.NewGuid(), "ext-1", ConversationType.Group);
    conversation.RecordMessage(DateTime.UtcNow, "Hello", isIncoming: true);

    conversation.MarkRead();

    Assert.Equal(0, conversation.UnreadCount);
  }

  [Fact]
  public void AddParticipant_WithNewExternalId_AddsParticipant()
  {
    var conversation = Conversation.Create(Guid.NewGuid(), "ext-1", ConversationType.Group);

    var participant = conversation.AddParticipant("p-1", "Ivan");

    Assert.Single(conversation.Participants);
    Assert.Equal(participant.Id, conversation.Participants[0].Id);
  }

  [Fact]
  public void AddParticipant_WithDuplicateExternalId_Throws()
  {
    var conversation = Conversation.Create(Guid.NewGuid(), "ext-1", ConversationType.Group);
    conversation.AddParticipant("p-1");

    Assert.Throws<InvalidOperationException>(() => conversation.AddParticipant("p-1"));
  }

  [Fact]
  public void RemoveParticipant_RemovesMatchingParticipant()
  {
    var conversation = Conversation.Create(Guid.NewGuid(), "ext-1", ConversationType.Group);
    var participant = conversation.AddParticipant("p-1");

    conversation.RemoveParticipant(participant.Id);

    Assert.Empty(conversation.Participants);
  }
}
