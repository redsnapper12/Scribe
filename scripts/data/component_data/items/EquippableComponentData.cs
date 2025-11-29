using Godot;
using Scribe.Scripts.Core.Entities;
using Scribe.Scripts.Items.Components;

namespace Scribe.Scripts.Items.Data;

/// <summary>
/// Resource data for equippable item components.
/// </summary>
[GlobalClass]
public partial class EquippableComponentData : ItemComponentData
{
    [Export] public EquipSlot Slot { get; set; } = EquipSlot.MainHand;

    public override IItemComponent CreateComponent()
    {
        return new EquippableComponent
        {
            Slot = this.Slot
        };
    }
}

/// <summary>
/// Implementation of IEquippable component.
/// </summary>
public class EquippableComponent : IEquippable
{
    public EquipSlot Slot { get; set; }

    public virtual void OnEquip(Entity wearer)
    {
        GD.Print($"Equipped in {Slot} slot");
    }

    public virtual void OnUnequip(Entity wearer)
    {
        GD.Print($"Unequipped from {Slot} slot");
    }

    public virtual bool CanEquip(Entity entity)
    {
        // Default: anyone can equip
        return true;
    }
}
