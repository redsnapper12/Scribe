using Godot;
using System;
using System.Collections.Generic;
using Scribe.Scripts.Core.Components;
using Scribe.Scripts.Core.Interfaces;
using Scribe.Scripts.AI;

namespace Scribe.Scripts.Core.Entities;

public partial class Entity : RefCounted, IDamageable, IMeleeAttacker
{
    private readonly List<Component> _components = new();
    private readonly Dictionary<Type, Component> _componentsByType = new();
    
    public string Name { get; set; }
    public Vector2I GridPosition { get; set; }
    
    public bool IsAI => GetComponent<AIComponent>() != null;
    
    public void AddComponent(Component component)
    {
        component.SetEntity(this);
        _components.Add(component);
        _componentsByType[component.GetType()] = component;
    }
    
    public T GetComponent<T>() where T : Component
    {
        if (_componentsByType.TryGetValue(typeof(T), out var component))
        {
            return component as T;
        }
        return null;
    }
    
    public void InitializeComponents()
    {
        foreach (var component in _components)
        {
            component.Initialize();
        }
        
        foreach (var component in _components)
        {
            component.OnReady();
        }
    }
    
    public void ProcessComponents(double delta)
    {
        foreach (var component in _components)
        {
            component.Process(delta);
        }
    }
    
    public void CleanupComponents()
    {
        foreach (var component in _components)
        {
            component.Cleanup();
        }
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
    
    #region AI Methods
    
    public AIBehaviorType BehaviorType => GetComponent<AIComponent>()?.BehaviorType ?? AIBehaviorType.SimpleAggressive;
    
    public void Act(BattleContext context)
    {
        GetComponent<AIComponent>()?.Act(context);
    }
    
    #endregion
}