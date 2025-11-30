using Scribe.Scripts.AI;
using Scribe.Scripts.Entities;

namespace Scribe.Scripts.Core.Interfaces.Items;

/// <summary>
/// Optional component for items that can be used/consumed.
/// </summary>
public interface IUsable
{
    /// <summary>
    /// Number of charges/uses remaining.
    /// -1 means infinite uses.
    /// </summary>
    int Charges { get; }

    /// <summary>
    /// Maximum charges this item can have.
    /// </summary>
    int MaxCharges { get; }

    /// <summary>
    /// Whether the item is consumed on use.
    /// </summary>
    bool ConsumedOnUse { get; }

    /// <summary>
    /// Called when an entity uses this item.
    /// Returns true if the item was successfully used.
    /// </summary>
    bool OnUse(Entity user, BattleContext context);

    /// <summary>
    /// Whether this item can currently be used.
    /// </summary>
    bool CanUse(Entity user);
}
