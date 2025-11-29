using Godot;
using System;
using System.Linq;
using Scribe.Scripts.Core.Interfaces;
using Scribe.Scripts.Data.ComponentData;

namespace Scribe.Scripts.Core.Components;

public partial class HealthComponent : Component, IDamageable
{
    private HealthComponentData _data;
    
    public int CurrentHP { get; private set; }
    public int MaxHP { get; private set; }
    public int ArmorClass { get; private set; }
    public bool IsAlive => CurrentHP > 0;
    
    [Signal]
    public delegate void DamagedEventHandler(int amount, int damageType);
    
    [Signal]
    public delegate void DeathEventHandler();
    
    public void Initialize(HealthComponentData data)
    {
        _data = data;
        MaxHP = data.MaxHP;
        CurrentHP = MaxHP;
        ArmorClass = data.ArmorClass;
    }
    
    public override void Initialize()
    {
    }
    
    public void TakeDamage(int amount, DamageType damageType = DamageType.Bludgeoning)
    {
        if (!IsAlive) return;
        
        int finalDamage = CalculateDamage(amount, damageType);
        
        if (finalDamage <= 0) return;
        
        CurrentHP = Math.Max(0, CurrentHP - finalDamage);
        EmitSignal(SignalName.Damaged, finalDamage, (int)damageType);
        
        if (!IsAlive)
        {
            EmitSignal(SignalName.Death);
        }
    }
    
    public void Heal(int amount)
    {
        if (!IsAlive) return;
        
        CurrentHP = Math.Min(MaxHP, CurrentHP + amount);
    }
    
    private int CalculateDamage(int baseDamage, DamageType damageType)
    {
        if (_data.Immunities.Contains(damageType))
            return 0;
        
        if (_data.Resistances.Contains(damageType))
            return baseDamage / 2;
        
        if (_data.Vulnerabilities.Contains(damageType))
            return baseDamage * 2;
        
        return baseDamage;
    }
}