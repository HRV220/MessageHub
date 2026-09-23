namespace MessageHub.Core.Entities;

public class Person
{
  /// <summary>
  /// Unique identifier of this person.
  /// </summary>
  public Guid Id { get; private set; }

  /// <summary>
  /// Short name used to identify this person in lists and search.
  /// </summary>
  public string Username
  {
    get;
    private set
    {
      if (string.IsNullOrWhiteSpace(value))
        throw new ArgumentException("Username must not be empty.", nameof(value));
      field = value;
    }
  } = null!;

  /// <summary>
  /// First name, if known.
  /// </summary>
  public string? FirstName { get; private set; }

  /// <summary>
  /// Last name, if known.
  /// </summary>
  public string? LastName { get; private set; }

  /// <summary>
  /// Phone number, if known.
  /// </summary>
  public string? Phone { get; private set; }

  /// <summary>
  /// Email address, if known.
  /// </summary>
  public string? Email { get; private set; }

  private readonly List<ChannelContact> _channelContacts = [];

  /// <summary>
  /// This person's identities across all channels (channel_contacts.person_id → person.id).
  /// </summary>
  public IReadOnlyList<ChannelContact> ChannelContacts => _channelContacts;

  private Person(Guid id, string username, string? firstName, string? lastName, string? phone, string? email)
  {
    Id = id;
    Username = username;
    FirstName = firstName;
    LastName = lastName;
    Phone = phone;
    Email = email;
  }

  /// <summary>
  /// Creates a new person. Only <paramref name="username"/> is required — the rest may be filled in later.
  /// </summary>
  /// <exception cref="ArgumentException"><paramref name="username"/> is null or whitespace.</exception>
  public static Person Create(string username, string? firstName = null, string? lastName = null, string? phone = null, string? email = null)
  {
    return new Person(Guid.NewGuid(), username, firstName, lastName, phone, email);
  }

  /// <summary>
  /// Updates the person's first and last name. Pass null to clear either.
  /// </summary>
  public void Rename(string? firstName, string? lastName)
  {
    FirstName = firstName;
    LastName = lastName;
  }

  /// <summary>
  /// Changes the person's phone number. Pass null to clear it.
  /// </summary>
  public void ChangePhone(string? phone)
  {
    Phone = phone;
  }

  /// <summary>
  /// Changes the person's email address. Pass null to clear it.
  /// </summary>
  public void ChangeEmail(string? email)
  {
    Email = email;
  }

  /// <summary>
  /// Adds a new channel identity for this person, as seen through one of the user's connected
  /// accounts (US-08). <see cref="ChannelContact"/> is created here, not by the caller, so it is
  /// always linked to this person's <see cref="Id"/> from the start.
  /// </summary>
  /// <exception cref="ArgumentException"><paramref name="connectedAccountId"/> is empty, or <paramref name="externalId"/> is null or whitespace.</exception>
  /// <exception cref="InvalidOperationException">This person already has an identity with the same <paramref name="connectedAccountId"/> and <paramref name="externalId"/>.</exception>
  public ChannelContact AddChannelContact(Guid connectedAccountId, string externalId, string? username = null, string? displayName = null, string? phone = null, string? email = null)
  {
    if (_channelContacts.Any(c => c.ConnectedAccountId == connectedAccountId && c.ExternalId == externalId))
      throw new InvalidOperationException("This channel identity is already linked to this person.");

    var contact = ChannelContact.Create(Id, connectedAccountId, externalId, username, displayName, phone, email);
    _channelContacts.Add(contact);
    return contact;
  }

  /// <summary>
  /// Unlinks a channel identity from this person (ФТ-504). The <see cref="ChannelContact"/> itself,
  /// and the conversations and messages linked through it, are not affected here — the caller decides
  /// their fate (reassign to a new person, or ignore).
  /// </summary>
  public void RemoveChannelContact(Guid channelContactId)
  {
    _channelContacts.RemoveAll(c => c.Id == channelContactId);
  }
}
