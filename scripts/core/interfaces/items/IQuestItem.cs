namespace Scribe.Scripts.Core.Interfaces.Items;

/// <summary>
/// Optional component for items related to quests.
/// </summary>
public interface IQuestItem
{
    /// <summary>
    /// Quest ID this item is associated with.
    /// </summary>
    string QuestId { get; }

    /// <summary>
    /// Whether this item can be dropped.
    /// Quest items are often not droppable.
    /// </summary>
    bool IsDroppable { get; }

    /// <summary>
    /// Whether this item can be sold.
    /// Quest items are often not sellable.
    /// </summary>
    bool IsSellable { get; }
}
