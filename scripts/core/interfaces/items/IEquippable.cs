using Scribe.Scripts.Core.Entities;

namespace Scribe.Scripts.Items.Components;

/// <summary>
/// Optional component for items that can be equipped by entities.
/// </summary>
public interface IEquippable : IItemComponent
{
    /// <summary>
    /// The equipment slot this item occupies.
    /// </summary>
    EquipSlot Slot { get; }

    /// <summary>
    /// Called when this item is equipped by an entity.
    /// </summary>
    void OnEquip(Entity wearer);

    /// <summary>
    /// Called when this item is unequipped by an entity.
    /// </summary>
    void OnUnequip(Entity wearer);

    /// <summary>
    /// Whether this item can be equipped by the given entity.
    /// </summary>
    bool CanEquip(Entity entity);
}

public enum EquipSlot
{
    MainHand,
    OffHand,
    Head,
    Body,
    Hands,
    Feet,
    Accessory1,
    Accessory2
}
