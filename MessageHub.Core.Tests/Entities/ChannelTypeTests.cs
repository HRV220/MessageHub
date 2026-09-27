using MessageHub.Core.Entities.ConnectedAccount.ValueObjects;

namespace MessageHub.Core.Tests.Entities;

public class ChannelTypeTests
{
  [Theory]
  [InlineData("Telegram", "telegram")]
  [InlineData("  vk  ", "vk")]
  public void From_NormalizesCase(string input, string expected)
  {
    var channelType = ChannelType.From(input);

    Assert.Equal(expected, channelType.Code);
  }

  [Fact]
  public void From_WithEmptyCode_Throws()
  {
    Assert.Throws<ArgumentException>(() => ChannelType.From(" "));
  }

  [Fact]
  public void From_WithInvalidCharacters_Throws()
  {
    Assert.Throws<ArgumentException>(() => ChannelType.From("Tele Gram!"));
  }

  [Fact]
  public void Equals_SameCodeDifferentCase_AreEqual()
  {
    Assert.Equal(ChannelType.From("telegram"), ChannelType.From("TELEGRAM"));
  }
}
