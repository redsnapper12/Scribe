using Godot;
using System;
using System.Collections.Generic;
using Scribe.Scripts.AI;
using Scribe.Scripts.Core;
using Scribe.Scripts.Core.Interfaces;
using Scribe.Scripts.Core.Services;
using Scribe.Scripts.Data;
using Scribe.Scripts.Data.ComponentData;
using Scribe.Scripts.Core.Interfaces.Entities;

namespace Scribe.Scripts.Entities;

public partial class Entity : RefCounted, IDamageable, IMeleeAttacker, IMovable
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
    public bool IsAI => Controller == ControllerType.AI && GetComponent<AIComponent>() != null;

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
    /// Gets a component of the specified type.
    /// </summary>
    public T GetComponent<T>() where T : IEntityComponent
    {
        if (_componentsByType.TryGetValue(typeof(T), out var component))
        {
            return (T)component;
        }
        return default;
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
    
    #region IDamageable Implementation
    
    public int CurrentHP => GetComponent<HealthComponent>()?.CurrentHP ?? 0;
    public int MaxHP => GetComponent<HealthComponent>()?.MaxHP ?? 0;
    public int ArmorClass => GetComponent<HealthComponent>()?.ArmorClass ?? 0;
    public bool IsAlive => GetComponent<HealthComponent>()?.IsAlive ?? false;
    
    public void TakeDamage(int amount, DamageType damageType = DamageType.Bludgeoning)
    {
        GetComponent<HealthComponent>()?.TakeDamage(amount, damageType);
    }
    
    public void Heal(int amount)
    {
        GetComponent<HealthComponent>()?.Heal(amount);
    }
    
    #endregion
    
    #region IMeleeAttacker Implementation
    
    public int AttackBonus => GetComponent<MeleeAttackComponent>()?.AttackBonus ?? 0;
    public int NumDice => GetComponent<MeleeAttackComponent>()?.NumDice ?? 1;
    public DieType DieType => GetComponent<MeleeAttackComponent>()?.DieType ?? DieType.D4;
    public int DamageBonus => GetComponent<MeleeAttackComponent>()?.DamageBonus ?? 0;
    public DamageType DamageType => GetComponent<MeleeAttackComponent>()?.DamageType ?? DamageType.Bludgeoning;
    public int MeleeRange => GetComponent<MeleeAttackComponent>()?.MeleeRange ?? 5;
    public string AttackName => GetComponent<MeleeAttackComponent>()?.AttackName ?? "Attack";
    
    public AttackResult MeleeAttack(IDamageable target)
    {
        return GetComponent<MeleeAttackComponent>()?.MeleeAttack(target) 
            ?? new AttackResult(false, 0, 0);
    }
    
    #endregion
    
    #region IMovable Implementation
    
    public int WalkSpeed => GetComponent<MovementComponent>()?.WalkSpeed ?? 30;
    public int FlySpeed => GetComponent<MovementComponent>()?.FlySpeed ?? 0;
    public int SwimSpeed => GetComponent<MovementComponent>()?.SwimSpeed ?? 0;
    public int ClimbSpeed => GetComponent<MovementComponent>()?.ClimbSpeed ?? 0;
    public int MovementRemaining => GetComponent<MovementComponent>()?.MovementRemaining ?? 0;
    
    public void ResetMovement()
    {
        GetComponent<MovementComponent>()?.ResetMovement();
    }
    
    public bool CanAffordMove(int cost)
    {
        return GetComponent<MovementComponent>()?.CanAffordMove(cost) ?? false;
    }
    
    public void SpendMovement(int cost)
    {
        GetComponent<MovementComponent>()?.SpendMovement(cost);
    }
    
    #endregion
    
    #region AI Methods

    public AIBehaviorType BehaviorType => GetComponent<AIComponent>()?.BehaviorType ?? AIBehaviorType.SimpleAggressive;

    public void Act(BattleContext context)
    {
        GetComponent<AIComponent>()?.Act(this, context);
    }

    #endregion
}