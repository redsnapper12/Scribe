using Godot;
using System;
using System.Collections.Generic;
using Scribe.Scripts.Core.Services;
using Scribe.Scripts.Data;
using Scribe.Scripts.Data.ComponentData;
using Scribe.Scripts.Core.Attributes;

namespace Scribe.Scripts.Entities;

[RequiresComponent(typeof(HealthComponent))]
[RequiresComponent(typeof(MovementComponent))]
[RequiresComponent(typeof(AttackComponent))]
[RequiresComponent(typeof(ActionEconomyComponent))]
public partial class Entity : RefCounted
{
    private readonly List<IEntityComponent> _components = new();
    private readonly Dictionary<Type, IEntityComponent> _componentsByType = new();

    public string EntityName { get; set; }
    public ControllerType Controller { get; set; }
    public Texture2D Icon {get; set; }
    public EntityType Type { get; set; }
    public EntitySize Size { get; set; }
    public EntityAlignment Alignment { get; set; }

    public Vector2I GridPosition { get; set; }
    public string OwnerPlayerId { get; set; } 

    /// <summary>
    /// Adds a component to this entity.
    /// </summary>
    public void AddComponent(IEntityComponent component)
    {
        _components.Add(component);
        _componentsByType[component.GetType()] = component;

        // Also register by all interface types the component implements
        foreach (var interfaceType in component.GetType().GetInterfaces())
        {
            if (typeof(IEntityComponent).IsAssignableFrom(interfaceType) && interfaceType != typeof(IEntityComponent))
            {
                _componentsByType[interfaceType] = component;
            }
        }
    }

    /// <summary>
    /// Attempts to get a component of the specified type.
    /// Returns true if the component exists, false otherwise.
    /// </summary>
    /// <typeparam name="T">The component type to retrieve</typeparam>
    /// <param name="component">The retrieved component, or default if not found</param>
    /// <returns>True if component was found, false otherwise</returns>
    public bool TryGetComponent<T>(out T component) where T : IEntityComponent
    {
        if (_componentsByType.TryGetValue(typeof(T), out var componentObj))
        {
            component = (T)componentObj;
            return true;
        }
        component = default;
        return false;
    }

    /// <summary>
    /// Checks if this entity has a component of the specified type.
    /// </summary>
    public bool HasComponent<T>() where T : IEntityComponent
    {
        return _componentsByType.ContainsKey(typeof(T));
    }

    /// <summary>
    /// Gets all components on this entity.
    /// </summary>
    public IReadOnlyList<IEntityComponent> GetAllComponents()
    {
        return _components.AsReadOnly();
    }

    internal bool TryGetComponent<T>()
    {
        throw new NotImplementedException();
    }
}