using MessageHub.Core.Entities;
using MessageHub.Core.Entities.ConnectedAccount.Enums;
using MessageHub.Core.Entities.ConnectedAccount.ValueObjects;

namespace MessageHub.Core.Entities.ConnectedAccount;

/// <summary>
/// A user's account in an external messaging service (7.1 ТЗ, 8.3 ТЗ — таблица connected_accounts).
/// Secrets (tokens/passwords) are never part of this entity or persisted anywhere in <c>Core</c> — they
/// live only in the process's in-memory secret store, outside this aggregate (БЗ-02, БЗ-03, 8.4 ТЗ).
/// </summary>
public class ConnectedAccount
{
  /// <summary>
  /// Unique identifier of this connected account.
  /// </summary>
  public int Id { get; private set; }

  /// <summary>
  /// Which service this account belongs to.
  /// </summary>
  public ChannelType ChannelType
  {
    get;
    private set
    {
      if (value is null)
        throw new ArgumentException("ChannelType must not be null.", nameof(value));
      field = value;
    }
  } = null!;

  /// <summary>
  /// Identifier of this account in the service itself (not the local database id). Null for services
  /// that have no separate account id — e.g. Email, where <see cref="Email"/> identifies the account instead.
  /// </summary>
  public string? AccountId { get; private set; }

  /// <summary>
  /// Email identifying this account when the service has no separate account id (e.g. Email). Set exactly
  /// when <see cref="AccountId"/> is not (8.3 ТЗ, таблица connected_accounts, CHECK).
  /// </summary>
  public string? Email { get; private set; }

  /// <summary>
  /// Human-readable name shown for this account in the UI.
  /// </summary>
  public string DisplayName
  {
    get;
    private set
    {
      if (string.IsNullOrWhiteSpace(value))
        throw new ArgumentException("DisplayName must not be empty.", nameof(value));
      field = value;
    }
  } = null!;

  /// <summary>
  /// Current connection state of the account.
  /// </summary>
  public AccountStatus Status { get; private set; }

  /// <summary>
  /// How much history to load when this account was first connected.
  /// </summary>
  public HistoryDepth InitialHistoryDepth { get; private set; }

  /// <summary>
  /// Whether background sync is currently enabled for this account.
  /// </summary>
  public bool SyncEnabled { get; private set; }

  /// <summary>
  /// When this account was connected, in UTC.
  /// </summary>
  public DateTime ConnectedAt { get; private set; }

  /// <summary>
  /// When this account last synced successfully, in UTC. Null if it never has.
  /// </summary>
  public DateTime? LastSyncAt { get; private set; }

  private readonly List<ChannelContact> _channelContacts = [];

  /// <summary>
  /// The contacts visible through this account (channel_contacts.connected_account_id → connected_accounts.id).
  /// Read-only here on purpose: <see cref="ChannelContact"/> belongs to the <see cref="Person"/> aggregate,
  /// which is the only place it is created, reassigned or removed (см. 7.1 ТЗ). EF Core populates this
  /// collection straight into the backing field for querying; it is not a second owner.
  /// </summary>
  public IReadOnlyList<ChannelContact> ChannelContacts => _channelContacts;

  private ConnectedAccount(int id, ChannelType channelType, string? accountId, string? email, string displayName, HistoryDepth initialHistoryDepth)
  {
    Id = id;
    ChannelType = channelType;
    AccountId = accountId;
    Email = email;
    DisplayName = displayName;
    InitialHistoryDepth = initialHistoryDepth;
    Status = AccountStatus.Connected;
    SyncEnabled = true;
    ConnectedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Connects a new account. The caller must already have completed the service's own authorization and
  /// stored the resulting secret in the process's in-memory secret store — this entity never sees it
  /// (БЗ-02). Exactly one of <paramref name="accountId"/>/<paramref name="email"/> must be provided.
  /// </summary>
  /// <exception cref="ArgumentException">
  /// <paramref name="channelType"/> is null, <paramref name="displayName"/> is null or whitespace, or
  /// <paramref name="accountId"/>/<paramref name="email"/> are not exactly one set.
  /// </exception>
  public static ConnectedAccount Create(ChannelType channelType, string? accountId, string? email, string displayName, HistoryDepth initialHistoryDepth)
  {
    ValidateIdentity(accountId, email);

    return new ConnectedAccount(default, channelType, accountId, email, displayName, initialHistoryDepth);
  }

  private static void ValidateIdentity(string? accountId, string? email)
  {
    var hasAccountId = !string.IsNullOrWhiteSpace(accountId);
    var hasEmail = !string.IsNullOrWhiteSpace(email);

    if (hasAccountId == hasEmail)
      throw new ArgumentException("Exactly one of AccountId/Email must be provided.");
  }

  /// <summary>
  /// Disconnects the account while keeping its local data (messages, contacts). Removing the secret from
  /// the in-memory store is the caller's responsibility (ФТ-207).
  /// </summary>
  public void Disconnect()
  {
    Status = AccountStatus.Disconnected;
  }

  /// <summary>
  /// Re-authorizes a disconnected or errored account, after the caller has stored a fresh secret (ФТ-208).
  /// </summary>
  public void Reconnect()
  {
    Status = AccountStatus.Connected;
  }

  /// <summary>
  /// Marks the account as failed after a permanent error from the adapter (ФТ-923). The error reason is
  /// not persisted here — connected_accounts has no such column (8.3 ТЗ) — surfacing it (log/UI) is the
  /// caller's responsibility.
  /// </summary>
  public void MarkError()
  {
    Status = AccountStatus.Error;
  }

  /// <summary>
  /// Records a successful sync or connection check (ФТ-209) and recovers the account from
  /// <see cref="AccountStatus.Error"/>, if it was in that state.
  /// </summary>
  public void RecordSuccessfulSync()
  {
    LastSyncAt = DateTime.UtcNow;
    if (Status == AccountStatus.Error)
      Status = AccountStatus.Connected;
  }

  /// <summary>
  /// Enables or disables background sync for this account without disconnecting it (ФТ-307).
  /// </summary>
  public void SetSyncEnabled(bool enabled)
  {
    SyncEnabled = enabled;
  }
}
