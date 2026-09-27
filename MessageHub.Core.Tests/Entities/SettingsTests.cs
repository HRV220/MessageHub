using MessageHub.Core.Entities.Settings;
using MessageHub.Core.Entities.Settings.Enums;

namespace MessageHub.Core.Tests.Entities;

public class SettingsTests
{
  [Fact]
  public void Create_GlobalScope_WithoutAccountId_Succeeds()
  {
    var settings = Settings.Create(SettingsScope.Global, "ui.theme", "\"dark\"");

    Assert.Equal(SettingsScope.Global, settings.Scope);
  }

  [Fact]
  public void Create_AccountScope_WithoutAccountId_Throws()
  {
    Assert.Throws<ArgumentException>(() => Settings.Create(SettingsScope.Account, "sync.enabled", "true"));
  }

  [Fact]
  public void Create_GlobalScope_WithAccountId_Throws()
  {
    Assert.Throws<ArgumentException>(() =>
      Settings.Create(SettingsScope.Global, "ui.theme", "\"dark\"", 1));
  }

  [Fact]
  public void UpdateValue_ChangesValue()
  {
    var settings = Settings.Create(SettingsScope.Global, "ui.theme", "\"dark\"");

    settings.UpdateValue("\"light\"");

    Assert.Equal("\"light\"", settings.Value);
  }
}
