namespace Scribe.Scripts.Items.Components;

/// <summary>
/// Required interface for all items. Defines basic item identity.
/// </summary>
public interface IIdentifiable : IItemComponent
{
    /// <summary>
    /// Unique identifier for this item type.
    /// </summary>
    string ItemId { get; }

    /// <summary>
    /// Display name shown to players.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Description text shown when examining the item.
    /// </summary>
    string Description { get; }
}
