using Godot;
using System;
using Scribe.Scripts.Core.Interfaces;
using Scribe.Scripts.Core.Interfaces.Entities;

namespace Scribe.Scripts.Data.ComponentData;

[GlobalClass]
public partial class MeleeAttackComponentData : EntityComponentData
{
    [Export] public int AttackBonus { get; set; } = 0;

    [ExportGroup("Damage")]
    [Export] public int NumDice { get; set; } = 1;
    [Export] public DieType DieType { get; set; } = DieType.D6;
    [Export] public int DamageBonus { get; set; } = 0;
    [Export] public DamageType DamageType { get; set; } = DamageType.Bludgeoning;

    [Export] public int Range { get; set; } = 5;
    [Export] public string AttackName { get; set; } = "Melee Attack";

    public override IEntityComponent CreateComponent()
    {
        return new MeleeAttackComponent
        {
            AttackBonus = this.AttackBonus,
            NumDice = this.NumDice,
            DieType = this.DieType,
            DamageBonus = this.DamageBonus,
            DamageType = this.DamageType,
            MeleeRange = this.Range,
            AttackName = this.AttackName
        };
    }
}

public partial class MeleeAttackComponent : RefCounted, IEntityComponent, IMeleeAttacker
{
    private readonly Random _random = new();

    public int AttackBonus { get; set; }
    public int NumDice { get; set; }
    public DieType DieType { get; set; }
    public int DamageBonus { get; set; }
    public DamageType DamageType { get; set; }
    public int MeleeRange { get; set; }
    public string AttackName { get; set; }

    public AttackResult MeleeAttack(IDamageable target)
    {
        int d20Roll = RollD20();
        int attackRoll = d20Roll + AttackBonus;
        bool isCritical = d20Roll == 20;
        bool isCriticalMiss = d20Roll == 1;

        if (isCriticalMiss)
        {
            return new AttackResult(false, attackRoll, 0, false);
        }

        bool hit = isCritical || attackRoll >= target.ArmorClass;

        if (!hit)
        {
            return new AttackResult(false, attackRoll, 0, false);
        }

        int damage = RollDamage(isCritical);
        target.TakeDamage(damage, DamageType);

        return new AttackResult(true, attackRoll, damage, isCritical);
    }

    private int RollD20()
    {
        return _random.Next(1, 21);
    }

    private int RollDamage(bool critical)
    {
        int totalDamage = 0;
        int rolls = critical ? 2 : 1;
        int dieSize = (int)DieType;

        for (int r = 0; r < rolls; r++)
        {
            for (int i = 0; i < NumDice; i++)
            {
                totalDamage += _random.Next(1, dieSize + 1);
            }
        }

        totalDamage += DamageBonus;

        return totalDamage;
    }
}
