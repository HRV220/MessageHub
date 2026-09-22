namespace MessageHub.Core.Entities;

public class Person
{
  public Guid Id { get; private set; }

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

  public string? FirstName { get; private set; }
  public string? LastName { get; private set; }
  public string? Phone { get; private set; }
  public string? Email { get; private set; }
  //TODO: Поменять ValueObject Channels (vk, tg, email and etc.)
  private readonly List<string> _connectedChannels = [];
  public IReadOnlyList<string> ConnectedChannels => _connectedChannels;

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
  /// Connects a channel (e.g. Telegram, WhatsApp) to this person, if not already connected.
  /// </summary>
  /// <exception cref="ArgumentException"><paramref name="channelName"/> is null or whitespace.</exception>
  public void ConnectChannel(string channelName)
  {
    if (string.IsNullOrWhiteSpace(channelName))
      throw new ArgumentException("ChannelName must not be empty.", nameof(channelName));

    if (!_connectedChannels.Contains(channelName))
      _connectedChannels.Add(channelName);
  }

  /// <summary>
  /// Disconnects a channel from this person, if connected.
  /// </summary>
  public void DisconnectChannel(string channelName)
  {
    _connectedChannels.Remove(channelName);
  }
}
