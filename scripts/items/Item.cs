using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Scribe.Scripts.Items.Components;

namespace Scribe.Scripts.Items;

/// <summary>
/// Base class for all items in the game.
/// Items are built from composable IItemComponent implementations.
/// All items must implement IIdentifiable, IStorable, and IInteractable.
/// </summary>
public partial class Item : RefCounted, IIdentifiable, IStorable, IInteractable
{
    private readonly List<IItemComponent> _components = new();
    private readonly Dictionary<Type, IItemComponent> _componentsByType = new();

    // Required properties from IIdentifiable
    public string ItemId { get; internal set; }
    public string DisplayName { get; internal set; }
    public string Description { get; internal set; }

    // Required properties from IStorable
    public bool IsStackable { get; internal set; }
    public int MaxStackSize { get; internal set; } = 1;
    public float Weight { get; internal set; }
    public Texture2D Icon { get; internal set; }

    /// <summary>
    /// Adds a component to this item.
    /// </summary>
    public void AddComponent(IItemComponent component)
    {
        _components.Add(component);
        _componentsByType[component.GetType()] = component;

        // Also register by all interface types the component implements
        foreach (var interfaceType in component.GetType().GetInterfaces())
        {
            if (typeof(IItemComponent).IsAssignableFrom(interfaceType) && interfaceType != typeof(IItemComponent))
            {
                _componentsByType[interfaceType] = component;
            }
        }
    }

    /// <summary>
    /// Gets a component of the specified type.
    /// </summary>
    public T GetComponent<T>() where T : IItemComponent
    {
        if (_componentsByType.TryGetValue(typeof(T), out var component))
        {
            return (T)component;
        }
        return default;
    }

    /// <summary>
    /// Checks if this item has a component of the specified type.
    /// </summary>
    public bool HasComponent<T>() where T : IItemComponent
    {
        return _componentsByType.ContainsKey(typeof(T));
    }

    /// <summary>
    /// Gets all components on this item.
    /// </summary>
    public IReadOnlyList<IItemComponent> GetAllComponents()
    {
        return _components.AsReadOnly();
    }

    #region IInteractable Implementation

    public virtual void OnPickup(Core.Entities.Entity picker)
    {
        // Default implementation - can be overridden by specific items
        GD.Print($"{picker.Name} picked up {DisplayName}");
    }

    public virtual void OnExamine(Core.Entities.Entity examiner)
    {
        // Default implementation - can be overridden by specific items
        GD.Print($"{examiner.Name} examines {DisplayName}: {Description}");
    }

    public virtual void OnDrop(Core.Entities.Entity dropper)
    {
        // Default implementation - can be overridden by specific items
        GD.Print($"{dropper.Name} dropped {DisplayName}");
    }

    #endregion

    /// <summary>
    /// Creates a deep copy of this item.
    /// Useful for creating instances from templates.
    /// </summary>
    public virtual Item Clone()
    {
        var clone = (Item)Activator.CreateInstance(GetType());
        clone.ItemId = ItemId;
        clone.DisplayName = DisplayName;
        clone.Description = Description;
        clone.IsStackable = IsStackable;
        clone.MaxStackSize = MaxStackSize;
        clone.Weight = Weight;
        clone.Icon = Icon;

        // Clone components (note: this is a shallow copy of component references)
        // For deep cloning of components, we'd need ICloneable on components
        foreach (var component in _components)
        {
            clone.AddComponent(component);
        }

        return clone;
    }
}
