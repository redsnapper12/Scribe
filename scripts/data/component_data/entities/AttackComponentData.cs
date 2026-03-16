using Godot;
using System;
using Scribe.Scripts.Entities;

namespace Scribe.Scripts.Data.ComponentData;

public struct AttackResult
{
    public bool Hit;
    public int AttackRoll;
    public int Damage;
    public bool IsCritical;

    public AttackResult(bool hit, int attackRoll, int damage, bool isCritical)
    {
        Hit = hit;
        AttackRoll = attackRoll;
        Damage = damage;
        IsCritical = isCritical;
    }

    public static AttackResult CriticalHit(int attackRoll, int damage)
    {
        return new AttackResult
        {
            Hit = true,
            AttackRoll = attackRoll,
            Damage = damage,
            IsCritical = true
        };
    }

    public static AttackResult NormalHit(int attackRoll, int damage)
    {
        return new AttackResult
        {
            Hit = true,
            AttackRoll = attackRoll,
            Damage = damage,
            IsCritical = false
        };
    }

    public static AttackResult Miss(int attackRoll)
    {
        return new AttackResult
        {
            Hit = false,
            AttackRoll = attackRoll,
            Damage = 0,
            IsCritical = false
        };
    }
}

[GlobalClass]
public partial class AttackComponentData : EntityComponentData
{
    /// <summary>
    /// Natural attacks for creatures (bite, claws, etc.).
    /// Empty for characters and creatures with only equipped weapons.
    /// Contains ONLY innate biological attacks, NOT equipped weapons.
    /// </summary>
    [ExportGroup("Natural Attacks")]
    [Export] public Godot.Collections.Array<MeleeAttackData> NaturalAttacks { get; set; } = new();

    public override IEntityComponent CreateComponent()
    {
        return new AttackComponent
        {
            NaturalAttacks = this.NaturalAttacks
        };
    }
}
