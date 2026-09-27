using System.Text;
using MessageHub.Core.Entities;

namespace MessageHub.Core.Tests.Entities;

public class UserProfileTests
{
  private static readonly byte[] Hash = Encoding.UTF8.GetBytes("hash");
  private static readonly byte[] Salt = Encoding.UTF8.GetBytes("salt");
  private static readonly byte[] NewHash = Encoding.UTF8.GetBytes("new-hash");
  private static readonly byte[] NewSalt = Encoding.UTF8.GetBytes("new-salt");

  [Fact]
  public void Create_WithValidArguments_ReturnsProfile()
  {
    var profile = UserProfile.Create("Roman", Hash, Salt, "{\"algo\":\"pbkdf2\"}");

    Assert.Equal("Roman", profile.Username);
    Assert.Equal(Hash, profile.PasswordHash);
  }

  [Fact]
  public void Create_WithEmptyUsername_Throws()
  {
    Assert.Throws<ArgumentException>(() => UserProfile.Create("", Hash, Salt, "{}"));
  }

  [Fact]
  public void Create_WithEmptyPasswordHash_Throws()
  {
    Assert.Throws<ArgumentException>(() => UserProfile.Create("Roman", [], Salt, "{}"));
  }

  [Fact]
  public void Create_WithEmptyPasswordSalt_Throws()
  {
    Assert.Throws<ArgumentException>(() => UserProfile.Create("Roman", Hash, [], "{}"));
  }

  [Fact]
  public void ChangePassword_UpdatesHashSaltAndKdfParams()
  {
    var profile = UserProfile.Create("Roman", Hash, Salt, "{}");

    profile.ChangePassword(NewHash, NewSalt, "{\"algo\":\"argon2\"}");

    Assert.Equal(NewHash, profile.PasswordHash);
    Assert.Equal(NewSalt, profile.PasswordSalt);
    Assert.Equal("{\"algo\":\"argon2\"}", profile.KdfParams);
  }
}
