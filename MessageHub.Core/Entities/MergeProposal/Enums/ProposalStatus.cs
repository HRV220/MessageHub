namespace MessageHub.Core.Entities.MergeProposal.Enums;

/// <summary>
/// Resolution status of a <see cref="Entities.MergeProposal"/> (8.3 ТЗ, таблица merge_proposals).
/// </summary>
public enum ProposalStatus
{
  Pending,
  Accepted,
  Rejected
}
