using MessageHub.Core.Entities.ConnectedAccount;
using MessageHub.Core.Entities.ConnectedAccount.Enums;
using MessageHub.Core.Entities.ConnectedAccount.ValueObjects;

namespace MessageHub.Core.Tests.Entities;

public class ConnectedAccountTests
{
  private static readonly ChannelType Telegram = ChannelType.From("telegram");

  [Fact]
  public void Create_WithAccountId_ReturnsConnectedAccount()
  {
    var account = ConnectedAccount.Create(Telegram, "12345", null, "My Telegram", HistoryDepth.All);

    Assert.Equal(AccountStatus.Connected, account.Status);
    Assert.Equal("12345", account.AccountId);
  }

  [Fact]
  public void Create_WithBothAccountIdAndEmail_Throws()
  {
    Assert.Throws<ArgumentException>(() =>
      ConnectedAccount.Create(Telegram, "12345", "a@b.com", "My Account", HistoryDepth.All));
  }

  [Fact]
  public void Create_WithNeitherAccountIdNorEmail_Throws()
  {
    Assert.Throws<ArgumentException>(() =>
      ConnectedAccount.Create(Telegram, null, null, "My Account", HistoryDepth.All));
  }

  [Fact]
  public void Disconnect_SetsStatusToDisconnected()
  {
    var account = ConnectedAccount.Create(Telegram, "12345", null, "My Telegram", HistoryDepth.All);

    account.Disconnect();

    Assert.Equal(AccountStatus.Disconnected, account.Status);
  }

  [Fact]
  public void RecordSuccessfulSync_FromError_RecoversToConnected()
  {
    var account = ConnectedAccount.Create(Telegram, "12345", null, "My Telegram", HistoryDepth.All);
    account.MarkError();

    account.RecordSuccessfulSync();

    Assert.Equal(AccountStatus.Connected, account.Status);
  }
}
