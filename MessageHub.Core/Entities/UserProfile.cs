namespace MessageHub.Core.Entities;

/// <summary>
/// The single local user profile (8.3 ТЗ, таблица user_profiles — always id = 1 in v1). A password is
/// always set, even when "disabled": disabling it means hashing an empty string (9.7 ТЗ).
/// </summary>
public class UserProfile
{
  /// <summary>
  /// Unique identifier of the local profile.
  /// </summary>
  public int Id { get; private set; }

  /// <summary>
  /// The user's own display name for the profile.
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
  /// Hash of the current password. Already hashed by the caller — this entity never sees a raw password.
  /// Empty password (disabled login, 9.7 ТЗ) is still a hash: the hash of the empty string.
  /// </summary>
  public byte[] PasswordHash
  {
    get;
    private set
    {
      if (value is null || value.Length == 0)
        throw new ArgumentException("PasswordHash must not be empty.", nameof(value));
      field = value;
    }
  } = null!;

  /// <summary>
  /// Salt used to derive <see cref="PasswordHash"/> (8.3 ТЗ).
  /// </summary>
  public byte[] PasswordSalt
  {
    get;
    private set
    {
      if (value is null || value.Length == 0)
        throw new ArgumentException("PasswordSalt must not be empty.", nameof(value));
      field = value;
    }
  } = null!;

  /// <summary>
  /// JSON describing the KDF algorithm and its parameters used to derive <see cref="PasswordHash"/> (БЗ-04).
  /// </summary>
  public string KdfParams
  {
    get;
    private set
    {
      if (string.IsNullOrWhiteSpace(value))
        throw new ArgumentException("KdfParams must not be empty.", nameof(value));
      field = value;
    }
  } = null!;

  /// <summary>
  /// When this profile was created, in UTC.
  /// </summary>
  public DateTime CreatedAt { get; private set; }

  /// <summary>
  /// When this profile was last updated, in UTC.
  /// </summary>
  public DateTime UpdatedAt { get; private set; }

  private UserProfile(int id, string username, byte[] passwordHash, byte[] passwordSalt, string kdfParams)
  {
    Id = id;
    Username = username;
    PasswordHash = passwordHash;
    PasswordSalt = passwordSalt;
    KdfParams = kdfParams;
    CreatedAt = DateTime.UtcNow;
    UpdatedAt = CreatedAt;
  }

  /// <summary>
  /// Creates the local profile on first run (ФТ-101). <paramref name="passwordHash"/>/<paramref name="passwordSalt"/>
  /// must already be produced by the caller's KDF — hashing is an infrastructure concern, not a domain one (БЗ-04).
  /// </summary>
  /// <exception cref="ArgumentException">Any required field is null or whitespace.</exception>
  public static UserProfile Create(string username, byte[] passwordHash, byte[] passwordSalt, string kdfParams)
  {
    if (string.IsNullOrWhiteSpace(username))
      throw new ArgumentException("Username must not be empty.", nameof(username));

    return new UserProfile(1, username, passwordHash, passwordSalt, kdfParams);
  }

  /// <summary>
  /// Sets a new password after the caller has already verified the current one — or is disabling it with
  /// the hash of an empty string (ФТ-102). All three arguments must already be produced by the caller's KDF.
  /// </summary>
  /// <exception cref="ArgumentException">Any required field is null or whitespace.</exception>
  public void ChangePassword(byte[] newPasswordHash, byte[] newPasswordSalt, string newKdfParams)
  {
    PasswordHash = newPasswordHash;
    PasswordSalt = newPasswordSalt;
    KdfParams = newKdfParams;
    UpdatedAt = DateTime.UtcNow;
  }
}
