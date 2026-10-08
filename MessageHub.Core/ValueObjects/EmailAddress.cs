namespace MessageHub.Core.ValueObjects;

/// <summary>
/// Email address in normalized form: trimmed and lower-cased (7.1 ТЗ). Value object without identity,
/// equality by normalized value. Validation is intentionally basic (RFC 5321 limits and the
/// <c>local@domain</c> shape) — whether the mailbox really exists is the channel's concern.
/// </summary>
public sealed record EmailAddress
{
  private const int MaxLength = 254;
  private const int MaxLocalPartLength = 64;

  /// <summary>
  /// Normalized address, e.g. <c>ivan.petrov@example.com</c>.
  /// </summary>
  public string Value
  {
    get;
    private set
    {
      if (string.IsNullOrWhiteSpace(value))
        throw new ArgumentException("Email address is required.", nameof(Value));

      var email = value.Trim().ToLowerInvariant();

      if (email.Length > MaxLength)
        throw new ArgumentException($"Email address must be at most {MaxLength} characters.", nameof(Value));
      if (email.Any(c => char.IsWhiteSpace(c) || char.IsControl(c)))
        throw new ArgumentException($"Email address must not contain whitespace: {value}", nameof(Value));

      var parts = email.Split('@');
      if (parts.Length != 2)
        throw new ArgumentException($"Email address must contain exactly one '@': {value}", nameof(Value));

      var (localPart, domain) = (parts[0], parts[1]);

      if (localPart.Length is 0 or > MaxLocalPartLength)
        throw new ArgumentException(
          $"Local part must be 1–{MaxLocalPartLength} characters: {value}", nameof(Value));
      if (!IsValidDomain(domain))
        throw new ArgumentException($"Invalid email domain: {value}", nameof(Value));

      field = email;
    }
  } = null!;

  private EmailAddress() { }

  /// <summary>
  /// Parses and normalizes an email address.
  /// </summary>
  /// <exception cref="ArgumentException"><paramref name="address"/> is null/whitespace, longer than 254 characters,
  /// contains whitespace, does not have the <c>local@domain</c> shape, or has an invalid domain.</exception>
  public static EmailAddress From(string address)
  {
    var email = new EmailAddress();
    email.Value = address;
    return email;
  }

  // Domain: at least two dot-separated labels, none empty (no leading/trailing/double dots).
  private static bool IsValidDomain(string domain)
  {
    var labels = domain.Split('.');
    return labels.Length >= 2 && labels.All(label => label.Length > 0);
  }

  /// <inheritdoc/>
  public override string ToString() => Value;
}
