using MessageHub.Core.Entities.MergeProposal.Enums;

namespace MessageHub.Core.Entities.MergeProposal;

/// <summary>
/// A suggestion to merge two <see cref="Person"/> entries that are likely duplicates (US-05, ФТ-513).
/// The pair is unordered and unique (7.3 ТЗ, инвариант 8): <see cref="Create"/> canonically orders the
/// two ids into <see cref="PersonLowId"/>/<see cref="PersonHighId"/> so the same pair, in either order,
/// always maps to the same proposal — the domain has no numeric id to order by, unlike the DB row
/// (8.3 ТЗ, таблица merge_proposals: "меньший/больший id пары").
/// </summary>
public class MergeProposal
{
  /// <summary>
  /// Unique identifier of this proposal.
  /// </summary>
  public Guid Id { get; private set; }

  /// <summary>
  /// The smaller of the two person ids in the pair, by canonical (GUID) ordering.
  /// </summary>
  public Guid PersonLowId { get; private set; }

  /// <summary>
  /// The larger of the two person ids in the pair, by canonical (GUID) ordering.
  /// </summary>
  public Guid PersonHighId { get; private set; }

  /// <summary>
  /// How confident the matching rules are (ФТ-513: only <see cref="MergeConfidence.High"/> can be auto-merged).
  /// </summary>
  public MergeConfidence Confidence { get; private set; }

  /// <summary>
  /// JSON description of the matching rules that fired.
  /// </summary>
  public string Reasons
  {
    get;
    private set
    {
      if (string.IsNullOrWhiteSpace(value))
        throw new ArgumentException("Reasons must not be empty.", nameof(value));
      field = value;
    }
  } = null!;

  /// <summary>
  /// Resolution status of this proposal.
  /// </summary>
  public ProposalStatus Status { get; private set; }

  /// <summary>
  /// When this proposal was created, in UTC.
  /// </summary>
  public DateTime CreatedAt { get; private set; }

  /// <summary>
  /// When this proposal was accepted or rejected, in UTC. Null while <see cref="Status"/> is <see cref="ProposalStatus.Pending"/>.
  /// </summary>
  public DateTime? ResolvedAt { get; private set; }

  private MergeProposal(Guid id, Guid personLowId, Guid personHighId, MergeConfidence confidence, string reasons)
  {
    Id = id;
    PersonLowId = personLowId;
    PersonHighId = personHighId;
    Confidence = confidence;
    Reasons = reasons;
    Status = ProposalStatus.Pending;
    CreatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Proposes merging two people. The pair is canonically ordered, so proposing the same pair in either
  /// order produces an equivalent proposal.
  /// </summary>
  /// <exception cref="ArgumentException">
  /// <paramref name="personAId"/> or <paramref name="personBId"/> is empty, they are equal, or <paramref name="reasons"/> is null or whitespace.
  /// </exception>
  public static MergeProposal Create(Guid personAId, Guid personBId, MergeConfidence confidence, string reasons)
  {
    if (personAId == Guid.Empty)
      throw new ArgumentException("PersonAId must not be empty.", nameof(personAId));
    if (personBId == Guid.Empty)
      throw new ArgumentException("PersonBId must not be empty.", nameof(personBId));
    if (personAId == personBId)
      throw new ArgumentException("Cannot propose merging a person with themselves.", nameof(personBId));

    var (low, high) = personAId.CompareTo(personBId) < 0 ? (personAId, personBId) : (personBId, personAId);
    return new MergeProposal(Guid.NewGuid(), low, high, confidence, reasons);
  }

  /// <summary>
  /// Accepts the proposal (ФТ-514: caller records a <see cref="RelationChangeLog"/> for the merge itself).
  /// </summary>
  /// <exception cref="InvalidOperationException">The proposal is not <see cref="ProposalStatus.Pending"/>.</exception>
  public void Accept()
  {
    if (Status != ProposalStatus.Pending)
      throw new InvalidOperationException($"Cannot accept a proposal with status {Status}.");

    Status = ProposalStatus.Accepted;
    ResolvedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Rejects the proposal. A rejected pair is not proposed again (7.3 ТЗ, инвариант 8).
  /// </summary>
  /// <exception cref="InvalidOperationException">The proposal is not <see cref="ProposalStatus.Pending"/>.</exception>
  public void Reject()
  {
    if (Status != ProposalStatus.Pending)
      throw new InvalidOperationException($"Cannot reject a proposal with status {Status}.");

    Status = ProposalStatus.Rejected;
    ResolvedAt = DateTime.UtcNow;
  }
}
