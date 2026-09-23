using MessageHub.Core.Entities.SyncState.Enums;

namespace MessageHub.Core.Entities.SyncState;

/// <summary>
/// Cursor and retry state of a sync job: either the account-level incremental updates cursor, or the
/// initial history load for one conversation (8.3 ТЗ, таблица sync_states; ФТ-905).
/// </summary>
public class SyncState
{
  /// <summary>
  /// Unique identifier of this sync state.
  /// </summary>
  public Guid Id { get; private set; }

  /// <summary>
  /// The connected account this sync job belongs to.
  /// </summary>
  public Guid ConnectedAccountId { get; private set; }

  /// <summary>
  /// The conversation this job loads history for. Null when <see cref="Kind"/> is <see cref="SyncKind.Updates"/>.
  /// </summary>
  public Guid? ConversationId { get; private set; }

  /// <summary>
  /// Whether this tracks the account-level updates cursor or a conversation's initial history load.
  /// </summary>
  public SyncKind Kind { get; private set; }

  /// <summary>
  /// Current state of the job.
  /// </summary>
  public SyncStatus Status { get; private set; }

  /// <summary>
  /// Opaque cursor/offset/UID understood only by the channel adapter.
  /// </summary>
  public string? Cursor { get; private set; }

  /// <summary>
  /// Lower bound of the initial history load, in UTC. Null means the whole available history.
  /// </summary>
  public DateTime? HistoryFrom { get; private set; }

  /// <summary>
  /// When the job last completed successfully, in UTC.
  /// </summary>
  public DateTime? LastSuccessAt { get; private set; }

  /// <summary>
  /// When the job last failed, in UTC.
  /// </summary>
  public DateTime? LastErrorAt { get; private set; }

  /// <summary>
  /// Number of consecutive failed attempts since the last success.
  /// </summary>
  public int RetryCount { get; private set; }

  /// <summary>
  /// When to retry next, in UTC. Null when no retry is scheduled.
  /// </summary>
  public DateTime? NextRetryAt { get; private set; }

  /// <summary>
  /// When this state was last updated, in UTC.
  /// </summary>
  public DateTime UpdatedAt { get; private set; }

  private SyncState(Guid id, Guid connectedAccountId, Guid? conversationId, SyncKind kind, DateTime? historyFrom)
  {
    Id = id;
    ConnectedAccountId = connectedAccountId;
    ConversationId = conversationId;
    Kind = kind;
    HistoryFrom = historyFrom;
    Status = SyncStatus.Idle;
    RetryCount = 0;
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Creates a new sync state. <paramref name="conversationId"/> is required for
  /// <see cref="SyncKind.History"/> and must be null for <see cref="SyncKind.Updates"/>, per the
  /// schema's CHECK constraint (8.3 ТЗ, таблица sync_states).
  /// </summary>
  /// <exception cref="ArgumentException">
  /// <paramref name="connectedAccountId"/> is empty, or <paramref name="conversationId"/> is inconsistent with <paramref name="kind"/>.
  /// </exception>
  public static SyncState Create(Guid connectedAccountId, SyncKind kind, Guid? conversationId = null, DateTime? historyFrom = null)
  {
    if (connectedAccountId == Guid.Empty)
      throw new ArgumentException("ConnectedAccountId must not be empty.", nameof(connectedAccountId));

    if (kind == SyncKind.History && (conversationId is null || conversationId == Guid.Empty))
      throw new ArgumentException("ConversationId is required for history sync.", nameof(conversationId));

    if (kind == SyncKind.Updates && conversationId is not null)
      throw new ArgumentException("ConversationId must be null for updates sync.", nameof(conversationId));

    return new SyncState(Guid.NewGuid(), connectedAccountId, conversationId, kind, historyFrom);
  }

  /// <summary>
  /// Marks the job as currently running.
  /// </summary>
  /// <exception cref="InvalidOperationException">The job is already <see cref="SyncStatus.Running"/>.</exception>
  public void Start()
  {
    if (Status == SyncStatus.Running)
      throw new InvalidOperationException("Sync job is already running.");

    Status = SyncStatus.Running;
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Records a successful sync at the given cursor, clearing any pending retry.
  /// </summary>
  public void RecordSuccess(string? cursor)
  {
    Cursor = cursor;
    Status = SyncStatus.Done;
    LastSuccessAt = DateTime.UtcNow;
    RetryCount = 0;
    NextRetryAt = null;
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Records a failed attempt and schedules the next retry (ФТ-923).
  /// </summary>
  public void RecordFailure(DateTime nextRetryAt)
  {
    Status = SyncStatus.Error;
    LastErrorAt = DateTime.UtcNow;
    RetryCount++;
    NextRetryAt = nextRetryAt;
    UpdatedAt = DateTime.UtcNow;
  }
}
