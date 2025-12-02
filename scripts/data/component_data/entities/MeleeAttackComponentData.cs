using Godot;
using System;
using Scribe.Scripts.Core.Interfaces;
using Scribe.Scripts.Core.Interfaces.Entities;
using Scribe.Scripts.Core.Interfaces.Items;

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
    public int AttackBonus { get; set; }
    public int NumDice { get; set; }
    public DieType DieType { get; set; }
    public int DamageBonus { get; set; }
    public DamageType DamageType { get; set; }
    public int MeleeRange { get; set; }
    public string AttackName { get; set; }

    public AttackResult MeleeAttack(IDamageable target)
    {
        DiceRollResult diceRollResult = DiceRollingService.RequestRoll(new(DieType.D20, 1, AttackBonus));

        int attackRoll = diceRollResult.Result;

        // If rolled crit failure, return missed attack immediately.
        if(diceRollResult.DiceRollCategory == DiceRollCategory.CriticalFailure) return new AttackResult(false, attackRoll, 0, false);

        bool isCritical = diceRollResult.DiceRollCategory == DiceRollCategory.CriticalSuccess;
        bool hit = isCritical || attackRoll >= target.ArmorClass;

        if (!hit)
        {
            return new AttackResult(false, attackRoll, 0, false);
        }

        int damage = RollDamage(isCritical);
        target.TakeDamage(damage, DamageType);

        return new AttackResult(true, attackRoll, damage, isCritical);
    }

    private int RollDamage(bool critical)
    {
        int totalDamage = 0;
        int rolls = critical ? 2 : 1;
  
        // Number of attacks
        for (int r = 0; r < rolls; r++)
        {   
            // Dice rolls per attack
            for (int i = 0; i < NumDice; i++)
            {
                totalDamage +=  DiceRollingService.RequestRoll(new(DieType, 1, DamageBonus)).Result;
            }
        }

        totalDamage += DamageBonus;

        return totalDamage;
    }
}
