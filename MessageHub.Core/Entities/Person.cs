using MessageHub.Core.ValueObjects;

namespace MessageHub.Core.Entities;

/// <summary>
/// A collocutor and aggregate root over their channel identities (7.1 ТЗ; US-04, US-05).
/// </summary>
public class Person
{
  /// <summary>
  /// Unique identifier of this person.
  /// </summary>
  public int Id { get; private set; }

  /// <summary>
  /// First name, if known.
  /// </summary>
  public string? FirstName { get; private set; }

  /// <summary>
  /// Last name, if known.
  /// </summary>
  public string? LastName { get; private set; }

  /// <summary>
  /// Name shown for this person in lists and the timeline.
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
  /// Normalized form of <see cref="DisplayName"/> used for search: lower-cased, ё → е. Recomputed
  /// automatically whenever <see cref="DisplayName"/> changes — SQLite's <c>NOCASE</c> does not fold
  /// Cyrillic case, so normalization happens in the application (8.3 ТЗ, таблица person).
  /// </summary>
  public string SearchName { get; private set; } = null!;

  /// <summary>
  /// Phone number (E.164), if known.
  /// </summary>
  public PhoneNumber? Phone { get; private set; }

  /// <summary>
  /// Email address, lower-cased, if known.
  /// </summary>
  public string? Email { get; private set; }

  /// <summary>
  /// Free-text note about this person.
  /// </summary>
  public string? Note { get; private set; }

  /// <summary>
  /// The channel identity used by default when sending to this person (US-07). Always one of
  /// <see cref="ChannelContacts"/> — enforced by <see cref="SetPreferredChannel"/>.
  /// </summary>
  public int? PreferredChannelContactId { get; private set; }

  /// <summary>
  /// When this person was created, in UTC.
  /// </summary>
  public DateTime CreatedAt { get; private set; }

  /// <summary>
  /// When this person was last updated, in UTC.
  /// </summary>
  public DateTime UpdatedAt { get; private set; }

  private readonly List<ChannelContact> _channelContacts = [];

  /// <summary>
  /// This person's identities across all channels (channel_contacts.person_id → person.id).
  /// </summary>
  public IReadOnlyList<ChannelContact> ChannelContacts => _channelContacts;

  private Person(int id, string displayName, string? firstName, string? lastName, PhoneNumber? phone, string? email, string? note)
  {
    Id = id;
    DisplayName = displayName;
    SearchName = Normalize(displayName);
    FirstName = firstName;
    LastName = lastName;
    Phone = phone;
    Email = email;
    Note = note;
    CreatedAt = DateTime.UtcNow;
    UpdatedAt = CreatedAt;
  }

  /// <summary>
  /// Creates a new person. Only <paramref name="displayName"/> is required — the rest may be filled in later.
  /// </summary>
  /// <exception cref="ArgumentException"><paramref name="displayName"/> is null or whitespace.</exception>
  public static Person Create(string displayName, string? firstName = null, string? lastName = null, PhoneNumber? phone = null, string? email = null, string? note = null)
  {
    return new Person(default, displayName, firstName, lastName, phone, email, note);
  }

  /// <summary>
  /// Updates the person's name fields. Recomputes <see cref="SearchName"/> from the new <paramref name="displayName"/>.
  /// </summary>
  /// <exception cref="ArgumentException"><paramref name="displayName"/> is null or whitespace.</exception>
  public void Rename(string displayName, string? firstName, string? lastName)
  {
    DisplayName = displayName;
    SearchName = Normalize(displayName);
    FirstName = firstName;
    LastName = lastName;
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Changes the person's phone number. Pass null to clear it.
  /// </summary>
  public void ChangePhone(PhoneNumber? phone)
  {
    Phone = phone;
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Changes the person's email address. Pass null to clear it.
  /// </summary>
  public void ChangeEmail(string? email)
  {
    Email = email;
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Replaces the free-text note. Pass null to clear it.
  /// </summary>
  public void SetNote(string? note)
  {
    Note = note;
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Sets the default channel to send to this person (US-07, ФТ-708).
  /// </summary>
  /// <exception cref="InvalidOperationException"><paramref name="channelContactId"/> does not belong to this person.</exception>
  public void SetPreferredChannel(int channelContactId)
  {
    if (!_channelContacts.Any(c => c.Id == channelContactId))
      throw new InvalidOperationException("PreferredChannelContactId must be one of this person's own channel contacts.");

    PreferredChannelContactId = channelContactId;
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Adds a new channel identity for this person, as seen through one of the user's connected
  /// accounts (US-08). <see cref="ChannelContact"/> is created here, not by the caller, so it is
  /// always linked to this person's <see cref="Id"/> from the start.
  /// </summary>
  /// <exception cref="ArgumentException"><paramref name="connectedAccountId"/> is empty, or <paramref name="externalId"/> is null or whitespace.</exception>
  /// <exception cref="InvalidOperationException">This person already has an identity with the same <paramref name="connectedAccountId"/> and <paramref name="externalId"/>.</exception>
  public ChannelContact AddChannelContact(int connectedAccountId, string externalId, string? username = null, string? displayName = null, PhoneNumber? phone = null, string? email = null)
  {
    if (_channelContacts.Any(c => c.ConnectedAccountId == connectedAccountId && c.ExternalId == externalId))
      throw new InvalidOperationException("This channel identity is already linked to this person.");

    var contact = ChannelContact.Create(Id, connectedAccountId, externalId, username, displayName, phone, email);
    _channelContacts.Add(contact);
    UpdatedAt = DateTime.UtcNow;
    return contact;
  }

  /// <summary>
  /// Unlinks a channel identity from this person (ФТ-504). The <see cref="ChannelContact"/> itself,
  /// and the conversations and messages linked through it, are not affected here — the caller decides
  /// their fate (reassign to a new person, or ignore). Clears <see cref="PreferredChannelContactId"/>
  /// if it pointed at the removed identity.
  /// </summary>
  public void RemoveChannelContact(int channelContactId)
  {
    _channelContacts.RemoveAll(c => c.Id == channelContactId);
    if (PreferredChannelContactId == channelContactId)
      PreferredChannelContactId = null;
    UpdatedAt = DateTime.UtcNow;
  }

  private static string Normalize(string displayName) => displayName.Trim().ToLowerInvariant().Replace('ё', 'е');
}
