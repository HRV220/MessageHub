namespace MessageHub.Core.ValueObjects;

/// <summary>
/// Phone number in E.164 form (ITU-T E.164, 7.1 ТЗ): <c>+</c>, country code (1–3 digits, the first one
/// non-zero) and national (significant) number; the whole number is at most 15 digits, so the national
/// part is at most <c>15 − cc</c> digits. Value object without identity, equality by country code and number.
/// Dialing prefixes of a particular country (<c>00</c>, <c>810</c>, <c>011</c>, trunk <c>8</c>) are not
/// part of E.164 and are not interpreted here.
/// </summary>
public sealed record PhoneNumber
{
  private const int MaxDigits = 15;
  private const int MaxCountryCodeDigits = 3;

  /// <summary>
  /// Country code digits (a leading <c>+</c> is dropped), e.g. <c>7</c>.
  /// </summary>
  public string CountryCode
  {
    get;
    private set
    {
      if (string.IsNullOrWhiteSpace(value))
        throw new ArgumentException("Country code is required.", nameof(CountryCode));

      var digits = value.TrimStart('+');

      if (!IsDigitsOnly(digits))
        throw new ArgumentException($"Country code must contain only digits: {value}", nameof(CountryCode));
      if (digits.StartsWith('0'))
        throw new ArgumentException($"Country code cannot start with 0: {value}", nameof(CountryCode));
      if (digits.Length > MaxCountryCodeDigits)
        throw new ArgumentException(
          $"Country code must be at most {MaxCountryCodeDigits} digits: {value}", nameof(CountryCode));

      field = digits;
    }
  } = null!;

  /// <summary>
  /// National (significant) number, digits only, e.g. <c>9991234567</c>.
  /// </summary>
  public string Number
  {
    get;
    private set
    {
      if (string.IsNullOrWhiteSpace(value))
        throw new ArgumentException("National number is required.", nameof(Number));

      var maxLength = MaxDigits - CountryCode.Length;

      if (!IsDigitsOnly(value))
        throw new ArgumentException("National number must contain only digits.", nameof(Number));
      if (value.Length > maxLength)
        throw new ArgumentException(
          $"National number must be at most {maxLength} digits for country code +{CountryCode}.", nameof(Number));

      field = value;
    }
  } = null!;

  private PhoneNumber() { }

  /// <summary>
  /// Builds a number from a country code (with or without leading <c>+</c>) and a national number.
  /// Formatting characters (spaces, dashes, parentheses) are not accepted — the caller passes digits only.
  /// </summary>
  /// <exception cref="ArgumentException">Country code is not 1–3 digits or starts with 0; national number is empty,
  /// not numeric, or together with the country code exceeds 15 digits.</exception>
  public static PhoneNumber From(string countryCode, string number)
  {
    var phone = new PhoneNumber();
    phone.CountryCode = countryCode;
    phone.Number = number;
    return phone;
  }

  private static bool IsDigitsOnly(string text) => text.All(char.IsAsciiDigit);

  /// <summary>
  /// Normalized E.164 form, e.g. <c>+79991234567</c>.
  /// </summary>
  public override string ToString() => $"+{CountryCode}{Number}";
}
