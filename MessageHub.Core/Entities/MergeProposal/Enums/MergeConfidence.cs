namespace MessageHub.Core.Entities.MergeProposal.Enums;

/// <summary>
/// How confident the matching rules are that two <see cref="Entities.Person"/> entries are the same
/// person (8.3 ТЗ, таблица merge_proposals; ФТ-513: only <see cref="High"/> can be auto-merged).
/// </summary>
public enum MergeConfidence
{
  High,
  Medium
}
