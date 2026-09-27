using MessageHub.Core.Entities;

namespace MessageHub.Core.Tests.Entities;

public class PersonTests
{
  [Fact]
  public void Create_WithValidDisplayName_ReturnsPersonWithNormalizedSearchName()
  {
    var person = Person.Create("Иван Ёлкин");

    Assert.Equal("иван елкин", person.SearchName);
  }

  [Fact]
  public void Create_WithEmptyDisplayName_Throws()
  {
    Assert.Throws<ArgumentException>(() => Person.Create(""));
  }

  [Fact]
  public void SetPreferredChannel_WithOwnChannelContact_Succeeds()
  {
    var person = Person.Create("Иван");
    var contact = person.AddChannelContact(1, "ext-1");

    person.SetPreferredChannel(contact.Id);

    Assert.Equal(contact.Id, person.PreferredChannelContactId);
  }

  [Fact]
  public void SetPreferredChannel_WithForeignChannelContact_Throws()
  {
    var person = Person.Create("Иван");

    Assert.Throws<InvalidOperationException>(() => person.SetPreferredChannel(999));
  }

  [Fact]
  public void RemoveChannelContact_ClearsPreferredChannelIfRemoved()
  {
    var person = Person.Create("Иван");
    var contact = person.AddChannelContact(1, "ext-1");
    person.SetPreferredChannel(contact.Id);

    person.RemoveChannelContact(contact.Id);

    Assert.Null(person.PreferredChannelContactId);
  }
}
