using Godot;
using Godot.Collections;
using System;

namespace Scribe.Scripts.Data.ComponentData;

public partial class HealthComponent : RefCounted, IEntityComponent
{
    public int CurrentHP { get; set; }
    public int MaxHP { get; set; }
    public int ArmorClass { get; set; }
    public Array<DamageType> Resistances { get; set; }
    public Array<DamageType> Vulnerabilities { get; set; }
    public Array<DamageType> Immunities { get; set; }
    public bool IsAlive => CurrentHP > 0;

    [Signal]
    public delegate void DamagedEventHandler(int amount, int damageType);

    [Signal]
    public delegate void DeathEventHandler();

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
        if (Immunities.Contains(damageType))
            return 0;

        if (Resistances.Contains(damageType))
            return baseDamage / 2;

        if (Vulnerabilities.Contains(damageType))
            return baseDamage * 2;

        return baseDamage;
    }
}
