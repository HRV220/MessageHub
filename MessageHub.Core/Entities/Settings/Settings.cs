using MessageHub.Core.Entities.Settings.Enums;

namespace MessageHub.Core.Entities.Settings;

/// <summary>
/// A single setting value, either global or scoped to one connected account (8.3 ТЗ, таблица settings;
/// ФТ-301, ФТ-307).
/// </summary>
public class Settings
{
  /// <summary>
  /// Unique identifier of this setting entry.
  /// </summary>
  public int Id { get; private set; }

  /// <summary>
  /// Whether this setting applies globally or to one connected account.
  /// </summary>
  public SettingsScope Scope { get; private set; }

  /// <summary>
  /// The account this setting applies to. Set exactly when <see cref="Scope"/> is <see cref="SettingsScope.Account"/>.
  /// </summary>
  public int? ConnectedAccountId { get; private set; }

  /// <summary>
  /// Setting key, e.g. <c>ui.theme</c>, <c>sync.intervalMinutes</c>.
  /// </summary>
  public string Key
  {
    get;
    private set
    {
      if (string.IsNullOrWhiteSpace(value))
        throw new ArgumentException("Key must not be empty.", nameof(value));
      field = value;
    }
  } = null!;

  /// <summary>
  /// JSON value of the setting.
  /// </summary>
  public string Value
  {
    get;
    private set
    {
      if (string.IsNullOrWhiteSpace(value))
        throw new ArgumentException("Value must not be empty.", nameof(value));
      field = value;
    }
  } = null!;

  /// <summary>
  /// When this setting was last updated, in UTC.
  /// </summary>
  public DateTime UpdatedAt { get; private set; }

  private Settings(int id, SettingsScope scope, int? connectedAccountId, string key, string value)
  {
    Id = id;
    Scope = scope;
    ConnectedAccountId = connectedAccountId;
    Key = key;
    Value = value;
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Creates a setting entry. <paramref name="connectedAccountId"/> is required for
  /// <see cref="SettingsScope.Account"/> and must be null for <see cref="SettingsScope.Global"/>
  /// (8.3 ТЗ, таблица settings, CHECK).
  /// </summary>
  /// <exception cref="ArgumentException">
  /// <paramref name="key"/>/<paramref name="value"/> is null or whitespace, or <paramref name="connectedAccountId"/>
  /// is inconsistent with <paramref name="scope"/>.
  /// </exception>
  public static Settings Create(SettingsScope scope, string key, string value, int? connectedAccountId = null)
  {
    var hasAccount = connectedAccountId is not null;
    if (scope == SettingsScope.Account && !hasAccount)
      throw new ArgumentException("ConnectedAccountId is required for account-scoped settings.", nameof(connectedAccountId));
    if (scope == SettingsScope.Global && hasAccount)
      throw new ArgumentException("ConnectedAccountId must be null for global settings.", nameof(connectedAccountId));

    return new Settings(default, scope, connectedAccountId, key, value);
  }

  /// <summary>
  /// Replaces the JSON value.
  /// </summary>
  /// <exception cref="ArgumentException"><paramref name="value"/> is null or whitespace.</exception>
  public void UpdateValue(string value)
  {
    Value = value;
    UpdatedAt = DateTime.UtcNow;
  }
}
