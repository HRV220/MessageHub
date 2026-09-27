using MessageHub.Core.Entities.MergeProposal;
using MessageHub.Core.Entities.MergeProposal.Enums;

namespace MessageHub.Core.Tests.Entities;

public class MergeProposalTests
{
  [Fact]
  public void Create_WithValidArguments_ReturnsPendingProposal()
  {
    var proposal = MergeProposal.Create(1, 2, MergeConfidence.High, "{}");

    Assert.Equal(ProposalStatus.Pending, proposal.Status);
    Assert.Null(proposal.ResolvedAt);
  }

  [Fact]
  public void Create_OrdersPairTheSameRegardlessOfArgumentOrder()
  {
    var personA = 1;
    var personB = 2;

    var proposal1 = MergeProposal.Create(personA, personB, MergeConfidence.High, "{}");
    var proposal2 = MergeProposal.Create(personB, personA, MergeConfidence.High, "{}");

    Assert.Equal(proposal1.PersonLowId, proposal2.PersonLowId);
    Assert.Equal(proposal1.PersonHighId, proposal2.PersonHighId);
  }

  [Fact]
  public void Create_WithSamePersonTwice_Throws()
  {
    var personId = 1;

    Assert.Throws<ArgumentException>(() => MergeProposal.Create(personId, personId, MergeConfidence.High, "{}"));
  }

  [Fact]
  public void Create_WithEmptyReasons_Throws()
  {
    Assert.Throws<ArgumentException>(() => MergeProposal.Create(1, 2, MergeConfidence.High, ""));
  }

  [Fact]
  public void Accept_FromPending_SetsStatusToAccepted()
  {
    var proposal = MergeProposal.Create(1, 2, MergeConfidence.High, "{}");

    proposal.Accept();

    Assert.Equal(ProposalStatus.Accepted, proposal.Status);
    Assert.NotNull(proposal.ResolvedAt);
  }

  [Fact]
  public void Accept_WhenNotPending_Throws()
  {
    var proposal = MergeProposal.Create(1, 2, MergeConfidence.High, "{}");
    proposal.Accept();

    Assert.Throws<InvalidOperationException>(() => proposal.Accept());
  }

  [Fact]
  public void Reject_FromPending_SetsStatusToRejected()
  {
    var proposal = MergeProposal.Create(1, 2, MergeConfidence.High, "{}");

    proposal.Reject();

    Assert.Equal(ProposalStatus.Rejected, proposal.Status);
    Assert.NotNull(proposal.ResolvedAt);
  }

  [Fact]
  public void Reject_WhenNotPending_Throws()
  {
    var proposal = MergeProposal.Create(1, 2, MergeConfidence.High, "{}");
    proposal.Reject();

    Assert.Throws<InvalidOperationException>(() => proposal.Reject());
  }
}
