using MessageHub.Core.ValueObjects;

namespace MessageHub.Core.Tests.Entities;

public class PhoneNumberTests
{
  [Theory]
  [InlineData("7", "9991234567", "+79991234567")]
  [InlineData("+7", "9991234567", "+79991234567")]
  [InlineData("992", "909998324", "+992909998324")]
  [InlineData("1", "23456789012345", "+123456789012345")]
  public void From_ValidParts_ReturnsNormalizedE164(string cc, string number, string expected)
  {
    var phone = PhoneNumber.From(cc, number);

    Assert.Equal(expected, phone.ToString());
    Assert.Equal(cc.TrimStart('+'), phone.CountryCode);
  }

  [Fact]
  public void From_SameNumberDifferentFormatting_AreEqual()
  {
    Assert.Equal(PhoneNumber.From("+7", "9991234567"), PhoneNumber.From("7", "9991234567"));
  }

  [Theory]
  [InlineData(null, "123")]
  [InlineData("", "123")]
  [InlineData("0", "123")]
  [InlineData("1234", "123")]
  [InlineData("7a", "123")]
  [InlineData(" 7", "123")]
  public void From_InvalidCountryCode_ThrowsArgumentException(string? cc, string number)
  {
    Assert.Throws<ArgumentException>(() => PhoneNumber.From(cc!, number));
  }

  [Theory]
  [InlineData("7", null)]
  [InlineData("7", "")]
  [InlineData("7", "   ")]
  [InlineData("7", "99a")]
  [InlineData("7", "+999123456")]
  [InlineData("7", "999 123-45-67")]
  [InlineData("7", "999123456789012")]
  [InlineData("992", "1234567890123")]
  public void From_InvalidNumber_ThrowsArgumentException(string cc, string? number)
  {
    Assert.Throws<ArgumentException>(() => PhoneNumber.From(cc, number!));
  }
}
