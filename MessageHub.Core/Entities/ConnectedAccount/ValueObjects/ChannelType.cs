namespace MessageHub.Core.Entities.ConnectedAccount.ValueObjects;

/// <summary>
/// Code of an external messaging service, e.g. <c>telegram</c>, <c>vk</c>, <c>whatsapp</c>, <c>email</c>
/// (7.2 ТЗ). Value object without identity, equality by code. The list of valid codes is intentionally
/// not baked into <c>Core</c> — this VO only checks the format; whether an adapter actually exists for
/// the code is <c>ChannelRegistry</c>'s job in <c>Application</c>.
/// </summary>
public sealed record ChannelType
{
  /// <summary>
  /// Normalized (lower-case, trimmed) code of the service.
  /// </summary>
  public string Code { get; }

  private ChannelType(string code) => Code = code;

  /// <summary>
  /// Parses and normalizes a channel code.
  /// </summary>
  /// <exception cref="ArgumentException"><paramref name="code"/> is null/whitespace, or contains characters other than <c>[a-z0-9-]</c>.</exception>
  public static ChannelType From(string code)
  {
    if (string.IsNullOrWhiteSpace(code))
      throw new ArgumentException("Channel code is required.", nameof(code));

    var normalized = code.Trim().ToLowerInvariant();
    if (!normalized.All(c => char.IsAsciiLetterLower(c) || char.IsAsciiDigit(c) || c == '-'))
      throw new ArgumentException($"Invalid channel code: {code}", nameof(code));

    return new ChannelType(normalized);
  }

  /// <inheritdoc/>
  public override string ToString() => Code;
}
