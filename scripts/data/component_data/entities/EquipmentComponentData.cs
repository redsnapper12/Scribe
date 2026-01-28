using Godot;
using Godot.Collections;
using Scribe.Scripts.Core.Interfaces.Items;
using Scribe.Scripts.Items;

namespace Scribe.Scripts.Data.ComponentData;

/// <summary>
/// Resource data for equipment tracking component.
/// </summary>
[GlobalClass]
public partial class EquipmentComponentData : EntityComponentData
{
    /// <summary>
    /// Pre-equipped items for entity templates.
    /// Maps equipment slots to ItemData resources.
    /// </summary>
    [Export] public Dictionary<EquipSlot, ItemData> StartingEquipment { get; set; } = new();

    public override IEntityComponent CreateComponent()
    {
        var component = new EquipmentComponent();

        // Load starting equipment from ItemData resources
        foreach (var kvp in StartingEquipment)
        {
            if (kvp.Value != null)
            {
                var item = kvp.Value.CreateItem();
                component.Equip(item);
            }
        }

        return component;
    }
}

/// <summary>
/// Implementation of equipment tracking component.
/// Manages equipped items by slot.
/// </summary>
public partial class EquipmentComponent : RefCounted, IEntityComponent
{
    private Dictionary<EquipSlot, Item> _equippedItems = new();

    /// <summary>
    /// Equip an item to the appropriate slot.
    /// </summary>
    public void Equip(Item item)
    {
        if (item == null) return;

        var equippable = item.GetComponent<EquippableComponent>();
        if (equippable == null)
        {
            GD.PrintErr($"Item {item.DisplayName} is not equippable");
            return;
        }

        var slot = equippable.Slot;

        // Unequip existing item in this slot
        if (_equippedItems.ContainsKey(slot))
        {
            Unequip(slot);
        }

        // Equip new item
        _equippedItems[slot] = item;
        // Note: Entity is RefCounted, not Node, so we can't use GetParent()
        // OnEquip will be handled differently or removed if not needed
    }

    /// <summary>
    /// Unequip item from a slot.
    /// </summary>
    public void Unequip(EquipSlot slot)
    {
        if (!_equippedItems.ContainsKey(slot)) return;

        var item = _equippedItems[slot];
        var equippable = item.GetComponent<EquippableComponent>();
        // Note: OnUnequip will be handled differently or removed if not needed

        _equippedItems.Remove(slot);
    }

    /// <summary>
    /// Get the item equipped in a specific slot.
    /// </summary>
    public Item GetEquippedItem(EquipSlot slot)
    {
        return _equippedItems.ContainsKey(slot) ? _equippedItems[slot] : null;
    }

    /// <summary>
    /// Get the weapon equipped in the main hand.
    /// </summary>
    public Item GetMainHandWeapon()
    {
        return GetEquippedItem(EquipSlot.MainHand);
    }

    public MeleeAttackData GetMainHandWeaponMeleeData()
    {
        return GetEquippedItem(EquipSlot.MainHand).GetComponent<WeaponComponent>().Attack;
    }

    /// <summary>
    /// Check if a weapon is equipped in the main hand.
    /// </summary>
    public bool HasWeaponEquipped()
    {
        var mainHand = GetMainHandWeapon();
        if (mainHand == null) return false;

        return mainHand.HasComponent<WeaponComponent>();
    }

    /// <summary>
    /// Get all equipped items.
    /// </summary>
    public Dictionary<EquipSlot, Item> GetAllEquippedItems()
    {
        return new Dictionary<EquipSlot, Item>(_equippedItems);
    }
}
