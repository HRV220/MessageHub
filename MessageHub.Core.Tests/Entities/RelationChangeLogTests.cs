using MessageHub.Core.Entities.RelationChangeLog;
using MessageHub.Core.Entities.RelationChangeLog.Enums;

namespace MessageHub.Core.Tests.Entities;

public class RelationChangeLogTests
{
  [Fact]
  public void Create_WithOnlyOperation_ReturnsEntryWithNullOptionalFields()
  {
    var entry = RelationChangeLog.Create(RelationOperation.PersonCreated);

    Assert.Equal(RelationOperation.PersonCreated, entry.Operation);
    Assert.Null(entry.PersonId);
    Assert.Null(entry.RelatedPersonId);
    Assert.Null(entry.ChannelContactId);
    Assert.Null(entry.ProposalId);
    Assert.Null(entry.Snapshot);
  }

  [Fact]
  public void Create_ForMerge_RecordsBothPeopleAndSnapshot()
  {
    var personId = Guid.NewGuid();
    var relatedPersonId = Guid.NewGuid();

    var entry = RelationChangeLog.Create(RelationOperation.Merge, personId, relatedPersonId, snapshot: "{}");

    Assert.Equal(personId, entry.PersonId);
    Assert.Equal(relatedPersonId, entry.RelatedPersonId);
    Assert.Equal("{}", entry.Snapshot);
  }
}
