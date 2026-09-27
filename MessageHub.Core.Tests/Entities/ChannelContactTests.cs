using MessageHub.Core.Entities;

namespace MessageHub.Core.Tests.Entities;

public class ChannelContactTests
{
  [Fact]
  public void Create_WithValidArguments_SetsCreatedAt()
  {
    var contact = ChannelContact.Create(1, 2, "ext-1");

    Assert.NotEqual(default, contact.CreatedAt);
  }

  [Fact]
  public void Create_WithEmptyPersonId_Throws()
  {
    Assert.Throws<ArgumentException>(() => ChannelContact.Create(-1, 2, "ext-1"));
  }

  [Fact]
  public void ReassignTo_UpdatesPersonId()
  {
    var contact = ChannelContact.Create(1, 2, "ext-1");
    var newPersonId = 5;

    contact.ReassignTo(newPersonId);

    Assert.Equal(newPersonId, contact.PersonId);
  }
}
