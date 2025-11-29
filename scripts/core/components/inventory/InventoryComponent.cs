using System.Collections.Generic;
using System.Linq;
using Godot;
using Scribe.Scripts.Core.Components;
using Scribe.Scripts.Items.Components;

namespace Scribe.Scripts.Items;

/// <summary>
/// Entity component that manages an entity's inventory and equipped items.
/// Similar to HealthComponent, MovementComponent pattern.
/// </summary>
public partial class InventoryComponent : Component
{
    private readonly List<ItemStack> _items = new();
    private readonly Dictionary<EquipSlot, Item> _equippedItems = new();

    [Export] public float MaxWeight { get; set; } = 100.0f;
    public float CurrentWeight { get; private set; }

    public int ItemCount => _items.Sum(stack => stack.Quantity);

    /// <summary>
    /// Attempts to add an item to the inventory.
    /// Returns true if successful, false if inventory is full or item is too heavy.
    /// </summary>
    public bool AddItem(Item item, int quantity = 1)
    {
        if (item == null || quantity <= 0)
            return false;

        // Check weight limit
        float totalWeight = item.Weight * quantity;
        if (CurrentWeight + totalWeight > MaxWeight)
        {
            GD.Print($"Cannot add {item.DisplayName}: would exceed weight limit");
            return false;
        }

        // If stackable, try to find existing stack
        if (item.IsStackable)
        {
            var existingStack = _items.FirstOrDefault(s => s.Item.ItemId == item.ItemId);
            if (existingStack != null)
            {
                int remainingSpace = item.MaxStackSize - existingStack.Quantity;
                int toAdd = Mathf.Min(quantity, remainingSpace);

                existingStack.Quantity += toAdd;
                CurrentWeight += item.Weight * toAdd;

                // If we couldn't add all, create a new stack
                if (toAdd < quantity)
                {
                    int remaining = quantity - toAdd;
                    _items.Add(new ItemStack { Item = item, Quantity = remaining });
                    CurrentWeight += item.Weight * remaining;
                }

                return true;
            }
        }

        // Create new stack
        _items.Add(new ItemStack { Item = item, Quantity = quantity });
        CurrentWeight += totalWeight;
        return true;
    }

    /// <summary>
    /// Attempts to remove an item from the inventory.
    /// Returns true if successful, false if item not found or insufficient quantity.
    /// </summary>
    public bool RemoveItem(string itemId, int quantity = 1)
    {
        if (string.IsNullOrEmpty(itemId) || quantity <= 0)
            return false;

        var stack = _items.FirstOrDefault(s => s.Item.ItemId == itemId);
        if (stack == null)
            return false;

        if (stack.Quantity < quantity)
            return false;

        CurrentWeight -= stack.Item.Weight * quantity;
        stack.Quantity -= quantity;

        if (stack.Quantity <= 0)
        {
            _items.Remove(stack);
        }

        return true;
    }

    /// <summary>
    /// Gets the first item with the specified ID.
    /// </summary>
    public Item GetItem(string itemId)
    {
        return _items.FirstOrDefault(s => s.Item.ItemId == itemId)?.Item;
    }

    /// <summary>
    /// Gets the item stack with the specified ID.
    /// </summary>
    public ItemStack GetItemStack(string itemId)
    {
        return _items.FirstOrDefault(s => s.Item.ItemId == itemId);
    }

    /// <summary>
    /// Gets all items in the inventory.
    /// </summary>
    public IReadOnlyList<ItemStack> GetAllItems()
    {
        return _items.AsReadOnly();
    }

    /// <summary>
    /// Checks if the inventory contains at least the specified quantity of an item.
    /// </summary>
    public bool HasItem(string itemId, int quantity = 1)
    {
        var stack = _items.FirstOrDefault(s => s.Item.ItemId == itemId);
        return stack != null && stack.Quantity >= quantity;
    }

    #region Equipment Management

    /// <summary>
    /// Attempts to equip an item from the inventory.
    /// Returns true if successful.
    /// </summary>
    public bool EquipItem(Item item)
    {
        if (item == null)
            return false;

        var equippable = item.GetComponent<IEquippable>();
        if (equippable == null)
        {
            GD.Print($"{item.DisplayName} is not equippable");
            return false;
        }

        if (!equippable.CanEquip(_entity))
        {
            GD.Print($"{_entity.Name} cannot equip {item.DisplayName}");
            return false;
        }

        var slot = equippable.Slot;

        // Unequip existing item in slot
        if (_equippedItems.ContainsKey(slot))
        {
            UnequipItem(slot);
        }

        // Equip new item
        _equippedItems[slot] = item;
        equippable.OnEquip(_entity);

        GD.Print($"{_entity.Name} equipped {item.DisplayName} in {slot} slot");
        return true;
    }

    /// <summary>
    /// Unequips an item from the specified slot.
    /// Returns the unequipped item, or null if slot was empty.
    /// </summary>
    public Item UnequipItem(EquipSlot slot)
    {
        if (!_equippedItems.TryGetValue(slot, out var item))
            return null;

        var equippable = item.GetComponent<IEquippable>();
        equippable?.OnUnequip(_entity);

        _equippedItems.Remove(slot);

        GD.Print($"{_entity.Name} unequipped {item.DisplayName} from {slot} slot");
        return item;
    }

    /// <summary>
    /// Gets the item equipped in the specified slot.
    /// </summary>
    public Item GetEquippedItem(EquipSlot slot)
    {
        _equippedItems.TryGetValue(slot, out var item);
        return item;
    }

    /// <summary>
    /// Gets all equipped items.
    /// </summary>
    public IReadOnlyDictionary<EquipSlot, Item> GetAllEquippedItems()
    {
        return _equippedItems;
    }

    /// <summary>
    /// Checks if an item is currently equipped.
    /// </summary>
    public bool IsEquipped(string itemId)
    {
        return _equippedItems.Values.Any(item => item.ItemId == itemId);
    }

    #endregion

    public override void Initialize()
    {
        base.Initialize();
        CurrentWeight = 0;
    }

    public override void Cleanup()
    {
        base.Cleanup();
        _items.Clear();
        _equippedItems.Clear();
    }
}

/// <summary>
/// Represents a stack of items in an inventory.
/// </summary>
public class ItemStack
{
    public Item Item { get; set; }
    public int Quantity { get; set; }

    public float TotalWeight => Item.Weight * Quantity;
}
