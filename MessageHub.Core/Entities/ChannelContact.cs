namespace MessageHub.Core.Entities;

/// <summary>
/// A contact's account within a single channel (e.g. @ivan_petrov in Telegram), as seen through one
/// of the user's own <see cref="ConnectedAccount"/>s. Belongs to exactly one <see cref="Person"/>;
/// reassigned, never deleted, during merge and split (9.6 ТЗ).
/// </summary>
public class ChannelContact
{
  /// <summary>
  /// Unique identifier of this channel identity.
  /// </summary>
  public Guid Id { get; private set; }

  /// <summary>
  /// Id of the person this identity currently belongs to.
  /// </summary>
  public Guid PersonId { get; private set; }

  /// <summary>
  /// The person this identity belongs to. Not set by the constructor — only <see cref="PersonId"/>
  /// is known when a <see cref="Person"/> creates or reassigns this contact by id; EF Core fixes up
  /// the reference from that id when the relationship is loaded (Include, or query tracking).
  /// </summary>
  public Person Person { get; private set; } = null!;

  /// <summary>
  /// Id of the connected account this identity is seen through.
  /// </summary>
  public Guid ConnectedAccountId { get; private set; }

  /// <summary>
  /// The account this identity is seen through. Same story as <see cref="Person"/>: only
  /// <see cref="ConnectedAccountId"/> is known at creation time, EF Core fixes up the reference.
  /// </summary>
  public ConnectedAccount ConnectedAccount { get; private set; } = null!;

  /// <summary>
  /// Identifier of this contact in the external service.
  /// </summary>
  public string ExternalId
  {
    get;
    private set
    {
      if (string.IsNullOrWhiteSpace(value))
        throw new ArgumentException("ExternalId must not be empty.", nameof(value));
      field = value;
    }
  } = null!;

  /// <summary>
  /// Username in the service, if it has one.
  /// </summary>
  public string? Username { get; private set; }

  /// <summary>
  /// Display name reported by the service, if any.
  /// </summary>
  public string? DisplayName { get; private set; }

  /// <summary>
  /// Phone number reported by the service, if any.
  /// </summary>
  public string? Phone { get; private set; }

  /// <summary>
  /// Email address reported by the service, if any.
  /// </summary>
  public string? Email { get; private set; }

  private ChannelContact(Guid id, Guid personId, Guid connectedAccountId, string externalId, string? username, string? displayName, string? phone, string? email)
  {
    Id = id;
    PersonId = personId;
    ConnectedAccountId = connectedAccountId;
    ExternalId = externalId;
    Username = username;
    DisplayName = displayName;
    Phone = phone;
    Email = email;
  }

  /// <summary>
  /// Creates a channel contact for a person, as seen through one of the user's connected accounts.
  /// </summary>
  /// <exception cref="ArgumentException">
  /// <paramref name="personId"/> or <paramref name="connectedAccountId"/> is empty, or <paramref name="externalId"/> is null or whitespace.
  /// </exception>
  public static ChannelContact Create(Guid personId, Guid connectedAccountId, string externalId, string? username = null, string? displayName = null, string? phone = null, string? email = null)
  {
    if (personId == Guid.Empty)
      throw new ArgumentException("PersonId must not be empty.", nameof(personId));
    if (connectedAccountId == Guid.Empty)
      throw new ArgumentException("ConnectedAccountId must not be empty.", nameof(connectedAccountId));

    return new ChannelContact(Guid.NewGuid(), personId, connectedAccountId, externalId, username, displayName, phone, email);
  }

  /// <summary>
  /// Refreshes the profile fields the service reports for this contact (username, name, phone, email),
  /// typically during background sync.
  /// </summary>
  public void UpdateProfile(string? username, string? displayName, string? phone, string? email)
  {
    Username = username;
    DisplayName = displayName;
    Phone = phone;
    Email = email;
  }

  /// <summary>
  /// Reassigns this identity to another person during merge or split (9.6 ТЗ). The identity itself,
  /// and every conversation and message linked through it, is preserved — only ownership changes.
  /// </summary>
  /// <exception cref="ArgumentException"><paramref name="personId"/> is empty.</exception>
  public void ReassignTo(Guid personId)
  {
    if (personId == Guid.Empty)
      throw new ArgumentException("PersonId must not be empty.", nameof(personId));

    PersonId = personId;
  }
}
