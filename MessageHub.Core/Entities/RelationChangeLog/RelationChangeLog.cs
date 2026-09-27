using MessageHub.Core.Entities.RelationChangeLog.Enums;

namespace MessageHub.Core.Entities.RelationChangeLog;

/// <summary>
/// Append-only audit entry for a change to the relations between people and their channel identities
/// (7.3 ТЗ, инвариант 7: "Каждая операция со связями создаёт запись в RelationChangeLog"; 9.6 ТЗ — merge/split).
/// Has no state transitions and no foreign keys by design — it must outlive the rows it references
/// (8.3 ТЗ, таблица relation_change_logs).
/// </summary>
public class RelationChangeLog
{
  /// <summary>
  /// Unique identifier of this log entry.
  /// </summary>
  public int Id { get; private set; }

  /// <summary>
  /// Kind of change this entry records.
  /// </summary>
  public RelationOperation Operation { get; private set; }

  /// <summary>
  /// The person primarily affected by this operation, if any. No FK on purpose — the entry survives deletion.
  /// </summary>
  public int? PersonId { get; private set; }

  /// <summary>
  /// The second person involved, for operations with two sides (e.g. <see cref="RelationOperation.Merge"/>).
  /// </summary>
  public int? RelatedPersonId { get; private set; }

  /// <summary>
  /// The channel identity involved, if any.
  /// </summary>
  public int? ChannelContactId { get; private set; }

  /// <summary>
  /// The <see cref="MergeProposal"/> this operation resolved, if any.
  /// </summary>
  public int? ProposalId { get; private set; }

  /// <summary>
  /// JSON snapshot of the relations before this operation. Used by split to suggest the original grouping (9.6 ТЗ).
  /// </summary>
  public string? Snapshot { get; private set; }

  /// <summary>
  /// When this entry was recorded, in UTC.
  /// </summary>
  public DateTime CreatedAt { get; private set; }

  private RelationChangeLog(int id, RelationOperation operation, int? personId, int? relatedPersonId, int? channelContactId, int? proposalId, string? snapshot)
  {
    Id = id;
    Operation = operation;
    PersonId = personId;
    RelatedPersonId = relatedPersonId;
    ChannelContactId = channelContactId;
    ProposalId = proposalId;
    Snapshot = snapshot;
    CreatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Records a relation change. Append-only — there is no way to modify or remove an entry afterwards.
  /// </summary>
  public static RelationChangeLog Create(RelationOperation operation, int? personId = null, int? relatedPersonId = null, int? channelContactId = null, int? proposalId = null, string? snapshot = null)
  {
    return new RelationChangeLog(default, operation, personId, relatedPersonId, channelContactId, proposalId, snapshot);
  }
}
