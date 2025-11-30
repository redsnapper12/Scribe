using Scribe.Scripts.Entities;

namespace Scribe.Scripts.Core.Interfaces.Items;

/// <summary>
/// Required interface for all items. Defines how items respond to interactions.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Called when an entity picks up this item.
    /// </summary>
    void OnPickup(Entity picker);

    /// <summary>
    /// Called when an entity examines this item.
    /// </summary>
    void OnExamine(Entity examiner);

    /// <summary>
    /// Called when an entity drops this item.
    /// </summary>
    void OnDrop(Entity dropper);
}
