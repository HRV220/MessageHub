using MessageHub.Core.ValueObjects;

namespace MessageHub.Core.Tests.Entities;

public class EmailAddressTests
{
  [Theory]
  [InlineData("ivan@example.com", "ivan@example.com")]
  [InlineData("  Ivan.Petrov@Example.COM ", "ivan.petrov@example.com")]
  [InlineData("a+tag@sub.example.co.uk", "a+tag@sub.example.co.uk")]
  public void From_ValidAddress_ReturnsNormalizedValue(string input, string expected)
  {
    var email = EmailAddress.From(input);

    Assert.Equal(expected, email.Value);
    Assert.Equal(expected, email.ToString());
  }

  [Fact]
  public void From_DifferentCaseOfSameAddress_AreEqual()
  {
    Assert.Equal(EmailAddress.From("Ivan@Example.com"), EmailAddress.From("ivan@example.COM"));
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData("ivan")]
  [InlineData("ivan@")]
  [InlineData("@example.com")]
  [InlineData("a@b@example.com")]
  [InlineData("ivan@example")]
  [InlineData("ivan@.example.com")]
  [InlineData("ivan@example.com.")]
  [InlineData("ivan@exa..mple.com")]
  [InlineData("iv an@example.com")]
  public void From_InvalidAddress_ThrowsArgumentException(string? input)
  {
    Assert.Throws<ArgumentException>(() => EmailAddress.From(input!));
  }

  [Fact]
  public void From_LocalPartLongerThan64_Throws()
  {
    Assert.Throws<ArgumentException>(() => EmailAddress.From(new string('a', 65) + "@example.com"));
  }

  [Fact]
  public void From_AddressLongerThan254_Throws()
  {
    Assert.Throws<ArgumentException>(() => EmailAddress.From("a@" + new string('b', 250) + ".com"));
  }
}
