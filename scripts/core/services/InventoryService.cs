using System.Collections.Generic;
using System.Linq;
using Godot;
using Scribe.Scripts.AI;
using Scribe.Scripts.Core;
using Scribe.Scripts.Core.Entities;
using Scribe.Scripts.Items.Components;

namespace Scribe.Scripts.Items;

/// <summary>
/// Service layer for inventory operations.
/// Provides a clean interaction layer for inventory management, similar to MovementService.
/// </summary>
public partial class InventoryService : Node
{
    private GridManager _gridManager;

    public override void _Ready()
    {
        _gridManager = GetNode<GridManager>("/root/Main/GridManager");
        if (_gridManager == null)
        {
            GD.PrintErr("InventoryService: GridManager not found!");
        }
    }

    #region Inventory Operations

    /// <summary>
    /// Transfers an item from one entity to another.
    /// </summary>
    public bool TransferItem(Entity from, Entity to, string itemId, int quantity = 1)
    {
        var fromInventory = from.GetComponent<InventoryComponent>();
        var toInventory = to.GetComponent<InventoryComponent>();

        if (fromInventory == null || toInventory == null)
        {
            GD.PrintErr("TransferItem: One or both entities lack InventoryComponent");
            return false;
        }

        var item = fromInventory.GetItem(itemId);
        if (item == null)
        {
            GD.Print($"TransferItem: {from.Name} does not have {itemId}");
            return false;
        }

        if (!fromInventory.HasItem(itemId, quantity))
        {
            GD.Print($"TransferItem: {from.Name} does not have enough {item.DisplayName}");
            return false;
        }

        if (!CanCarryItem(to, item, quantity))
        {
            GD.Print($"TransferItem: {to.Name} cannot carry {quantity}x {item.DisplayName}");
            return false;
        }

        // Remove from source
        if (!fromInventory.RemoveItem(itemId, quantity))
            return false;

        // Add to destination
        if (!toInventory.AddItem(item, quantity))
        {
            // Rollback if add fails
            fromInventory.AddItem(item, quantity);
            return false;
        }

        GD.Print($"{from.Name} transferred {quantity}x {item.DisplayName} to {to.Name}");
        return true;
    }

    /// <summary>
    /// Picks up an item from the ground.
    /// Note: ItemNode will be implemented next, so this is a placeholder for now.
    /// </summary>
    public bool PickupItem(Entity entity, ItemNode itemNode)
    {
        if (entity == null || itemNode == null)
            return false;

        var inventory = entity.GetComponent<InventoryComponent>();
        if (inventory == null)
        {
            GD.PrintErr($"PickupItem: {entity.Name} has no InventoryComponent");
            return false;
        }

        var item = itemNode.Item;
        if (!CanCarryItem(entity, item, 1))
        {
            GD.Print($"{entity.Name} cannot carry {item.DisplayName}");
            return false;
        }

        // Trigger interaction event
        item.OnPickup(entity);

        // Add to inventory
        if (!inventory.AddItem(item, 1))
            return false;

        // Remove from grid
        if (_gridManager != null)
        {
            _gridManager.RemoveItem(itemNode);
        }

        GD.Print($"{entity.Name} picked up {item.DisplayName}");
        return true;
    }

    /// <summary>
    /// Drops an item from inventory onto the ground.
    /// Note: ItemNode spawning will be implemented when GridManager is updated.
    /// </summary>
    public bool DropItem(Entity entity, string itemId, Vector2I targetPosition, int quantity = 1)
    {
        if (entity == null || string.IsNullOrEmpty(itemId))
            return false;

        var inventory = entity.GetComponent<InventoryComponent>();
        if (inventory == null)
        {
            GD.PrintErr($"DropItem: {entity.Name} has no InventoryComponent");
            return false;
        }

        var item = inventory.GetItem(itemId);
        if (item == null)
        {
            GD.Print($"DropItem: {entity.Name} does not have {itemId}");
            return false;
        }

        if (!inventory.HasItem(itemId, quantity))
        {
            GD.Print($"DropItem: {entity.Name} does not have enough {item.DisplayName}");
            return false;
        }

        // Check if target position is valid
        if (_gridManager != null && !_gridManager.IsValidGridPosition(targetPosition))
        {
            GD.Print($"DropItem: Invalid target position {targetPosition}");
            return false;
        }

        // Remove from inventory
        if (!inventory.RemoveItem(itemId, quantity))
            return false;

        // Trigger interaction event
        item.OnDrop(entity);

        // Spawn on grid
        if (_gridManager != null)
        {
            _gridManager.PlaceItem(item, targetPosition);
        }

        GD.Print($"{entity.Name} dropped {quantity}x {item.DisplayName} at {targetPosition}");
        return true;
    }

    #endregion

    #region Equipment Operations

    /// <summary>
    /// Equips an item from the entity's inventory.
    /// </summary>
    public bool EquipItem(Entity entity, string itemId)
    {
        if (entity == null || string.IsNullOrEmpty(itemId))
            return false;

        var inventory = entity.GetComponent<InventoryComponent>();
        if (inventory == null)
        {
            GD.PrintErr($"EquipItem: {entity.Name} has no InventoryComponent");
            return false;
        }

        var item = inventory.GetItem(itemId);
        if (item == null)
        {
            GD.Print($"EquipItem: {entity.Name} does not have {itemId}");
            return false;
        }

        return inventory.EquipItem(item);
    }

    /// <summary>
    /// Unequips an item from the specified slot.
    /// </summary>
    public bool UnequipItem(Entity entity, EquipSlot slot)
    {
        if (entity == null)
            return false;

        var inventory = entity.GetComponent<InventoryComponent>();
        if (inventory == null)
        {
            GD.PrintErr($"UnequipItem: {entity.Name} has no InventoryComponent");
            return false;
        }

        var item = inventory.UnequipItem(slot);
        return item != null;
    }

    /// <summary>
    /// Swaps equipment - unequips old item and equips new item.
    /// </summary>
    public bool SwapEquipment(Entity entity, Item newItem)
    {
        if (entity == null || newItem == null)
            return false;

        var inventory = entity.GetComponent<InventoryComponent>();
        if (inventory == null)
        {
            GD.PrintErr($"SwapEquipment: {entity.Name} has no InventoryComponent");
            return false;
        }

        var equippable = newItem.GetComponent<IEquippable>();
        if (equippable == null)
        {
            GD.Print($"SwapEquipment: {newItem.DisplayName} is not equippable");
            return false;
        }

        // Unequip existing item if any
        var existingItem = inventory.GetEquippedItem(equippable.Slot);
        if (existingItem != null)
        {
            inventory.UnequipItem(equippable.Slot);
        }

        // Equip new item
        return inventory.EquipItem(newItem);
    }

    #endregion

    #region Item Usage

    /// <summary>
    /// Uses an item from the entity's inventory.
    /// </summary>
    public bool UseItem(Entity user, string itemId, BattleContext context)
    {
        if (user == null || string.IsNullOrEmpty(itemId))
            return false;

        var inventory = user.GetComponent<InventoryComponent>();
        if (inventory == null)
        {
            GD.PrintErr($"UseItem: {user.Name} has no InventoryComponent");
            return false;
        }

        var item = inventory.GetItem(itemId);
        if (item == null)
        {
            GD.Print($"UseItem: {user.Name} does not have {itemId}");
            return false;
        }

        var usable = item.GetComponent<IUsable>();
        if (usable == null)
        {
            GD.Print($"UseItem: {item.DisplayName} is not usable");
            return false;
        }

        if (!usable.CanUse(user))
        {
            GD.Print($"UseItem: {user.Name} cannot use {item.DisplayName} right now");
            return false;
        }

        bool used = usable.OnUse(user, context);
        if (!used)
            return false;

        // If consumed on use, remove from inventory
        if (usable.ConsumedOnUse)
        {
            inventory.RemoveItem(itemId, 1);
            GD.Print($"{user.Name} consumed {item.DisplayName}");
        }

        return true;
    }

    #endregion

    #region Query Operations

    /// <summary>
    /// Gets all equippable items in the entity's inventory.
    /// </summary>
    public List<Item> GetEquippableItems(Entity entity)
    {
        var inventory = entity?.GetComponent<InventoryComponent>();
        if (inventory == null)
            return new List<Item>();

        return inventory.GetAllItems()
            .Where(stack => stack.Item.HasComponent<IEquippable>())
            .Select(stack => stack.Item)
            .ToList();
    }

    /// <summary>
    /// Gets all usable items in the entity's inventory.
    /// </summary>
    public List<Item> GetUsableItems(Entity entity)
    {
        var inventory = entity?.GetComponent<InventoryComponent>();
        if (inventory == null)
            return new List<Item>();

        return inventory.GetAllItems()
            .Where(stack => stack.Item.HasComponent<IUsable>())
            .Select(stack => stack.Item)
            .ToList();
    }

    /// <summary>
    /// Checks if an entity can carry an item (weight limit check).
    /// </summary>
    public bool CanCarryItem(Entity entity, Item item, int quantity = 1)
    {
        var inventory = entity?.GetComponent<InventoryComponent>();
        if (inventory == null)
            return false;

        float totalWeight = item.Weight * quantity;
        return (inventory.CurrentWeight + totalWeight) <= inventory.MaxWeight;
    }

    #endregion
}
