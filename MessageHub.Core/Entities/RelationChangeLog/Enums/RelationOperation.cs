namespace MessageHub.Core.Entities.RelationChangeLog.Enums;

/// <summary>
/// Kind of relation change recorded by a <see cref="Entities.RelationChangeLog"/> entry
/// (8.3 ТЗ, таблица relation_change_logs; 9.6 ТЗ — merge/split).
/// </summary>
public enum RelationOperation
{
  Merge,
  AutoMerge,
  Split,
  ContactAdded,
  ContactRemoved,
  ProposalAccepted,
  ProposalRejected,
  PersonCreated,
  PersonDeleted
}
