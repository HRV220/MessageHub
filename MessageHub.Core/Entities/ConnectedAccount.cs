using MessageHub.Core.Enums;

namespace MessageHub.Core.Entities;

public class ConnectedAccount
{
  /// <summary>
  /// Unique identifier of this connected account.
  /// </summary>
  public Guid Id { get; private set; }

  /// <summary>
  /// Which service this account belongs to.
  /// </summary>
  public ChannelType ChannelType { get; private set; }

  /// <summary>
  /// Identifier of this account in the service itself (not the local database id). Null for services
  /// that have no separate account id — e.g. Email, where the address in <see cref="DisplayName"/>
  /// (or a future dedicated field) is the only identifier there is.
  /// </summary>
  public string? AccountId
  {
    get;
    private set
    {
      if (value is not null && string.IsNullOrWhiteSpace(value))
        throw new ArgumentException("AccountId must not be empty when provided.", nameof(value));
      field = value;
    }
  }

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
  //TODO: Удалить
  /// <summary>
  /// Description of the last error, without secrets, when <see cref="Status"/> is <see cref="AccountStatus.Error"/>.
  /// </summary>
  public string? LastError { get; private set; }
  //TODO: Удалить будет локальный сервер вообще непонятно зачем это
  /// <summary>
  /// Reference to the secret in the OS secret store. Never the secret itself (БЗ-02, БЗ-03).
  /// Null only while <see cref="Status"/> is <see cref="AccountStatus.Disconnected"/>.
  /// </summary>
  public string? SecretRef { get; private set; }
  //TODO: Rename enum
  /// <summary>
  /// How much history to load when this account was first connected.
  /// </summary>
  public HistoryDepth InitialHistoryDepth { get; private set; }
  //TODO: What is
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

  private ConnectedAccount(Guid id, ChannelType channelType, string? accountId, string displayName, string secretRef, HistoryDepth initialHistoryDepth)
  {
    Id = id;
    ChannelType = channelType;
    AccountId = accountId;
    DisplayName = displayName;
    SecretRef = secretRef;
    InitialHistoryDepth = initialHistoryDepth;
    Status = AccountStatus.Connected;
    SyncEnabled = true;
    ConnectedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Connects a new account. The caller must already have completed the service's own authorization
  /// and stored the resulting secret in the OS secret store — <paramref name="secretRef"/> is only a
  /// reference to it, never the secret itself.
  /// </summary>
  /// <exception cref="ArgumentException">Any required field is null or whitespace.</exception>
  public static ConnectedAccount Create(ChannelType channelType, string? accountId, string displayName, string secretRef, HistoryDepth initialHistoryDepth)
  {
    if (string.IsNullOrWhiteSpace(secretRef))
      throw new ArgumentException("SecretRef must not be empty.", nameof(secretRef));

    return new ConnectedAccount(Guid.NewGuid(), channelType, accountId, displayName, secretRef, initialHistoryDepth);
  }

  /// <summary>
  /// Disconnects the account while keeping its local data (messages, contacts). Clears the secret
  /// reference — removing the actual secret from the OS store is the caller's responsibility (ФТ-207).
  /// </summary>
  public void Disconnect()
  {
    Status = AccountStatus.Disconnected;
    SecretRef = null;
    LastError = null;
  }

  /// <summary>
  /// Re-authorizes a disconnected or errored account with a freshly stored secret (ФТ-208).
  /// </summary>
  /// <exception cref="ArgumentException"><paramref name="secretRef"/> is null or whitespace.</exception>
  public void Reconnect(string secretRef)
  {
    if (string.IsNullOrWhiteSpace(secretRef))
      throw new ArgumentException("SecretRef must not be empty.", nameof(secretRef));

    SecretRef = secretRef;
    Status = AccountStatus.Connected;
    LastError = null;
  }

  /// <summary>
  /// Marks the account as failed after a permanent error from the adapter (ФТ-923).
  /// </summary>
  /// <exception cref="ArgumentException"><paramref name="reason"/> is null or whitespace.</exception>
  public void MarkError(string reason)
  {
    if (string.IsNullOrWhiteSpace(reason))
      throw new ArgumentException("Reason must not be empty.", nameof(reason));

    Status = AccountStatus.Error;
    LastError = reason;
  }

  /// <summary>
  /// Records a successful sync or connection check (ФТ-209) and recovers the account from
  /// <see cref="AccountStatus.Error"/>, if it was in that state.
  /// </summary>
  public void RecordSuccessfulSync()
  {
    LastSyncAt = DateTime.UtcNow;
    if (Status == AccountStatus.Error)
    {
      Status = AccountStatus.Connected;
      LastError = null;
    }
  }

  /// <summary>
  /// Enables or disables background sync for this account without disconnecting it (ФТ-307).
  /// </summary>
  public void SetSyncEnabled(bool enabled)
  {
    SyncEnabled = enabled;
  }
}
