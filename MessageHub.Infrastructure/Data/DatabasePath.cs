namespace MessageHub.Infrastructure.Data;

internal static class DatabasePath
{
  private static readonly Lazy<string> Value = new(Resolve);

  public static string Get() => Value.Value;

  private static string Resolve()
  {
    var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MessageHub");

    Directory.CreateDirectory(folder);

    return Path.Combine(folder, "message_hub.db");
  }
}
