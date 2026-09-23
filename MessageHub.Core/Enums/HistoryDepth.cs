namespace MessageHub.Core.Enums;

/// <summary>
/// How much history to load when a channel is first connected (ФТ-205).
/// </summary>
public enum HistoryDepth
{
  None,
  SevenDays,
  ThirtyDays,
  NinetyDays,
  All
}
