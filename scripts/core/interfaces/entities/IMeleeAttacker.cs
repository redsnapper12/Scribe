namespace Scribe.Scripts.Core.Interfaces;

public interface IMeleeAttacker
{
    int AttackBonus { get; }
    int NumDice { get; }
    DieType DieType { get; }
    int DamageBonus { get; }
    DamageType DamageType { get; }
    int MeleeRange { get; }
    string AttackName { get; }

    AttackResult MeleeAttack(IDamageable target);
}

public struct AttackResult
{
    public bool Hit { get; set; }
    public int AttackRoll { get; set; }
    public int Damage { get; set; }
    public bool CriticalHit { get; set; }
    
    public AttackResult(bool hit, int attackRoll, int damage, bool criticalHit = false)
    {
        Hit = hit;
        AttackRoll = attackRoll;
        Damage = damage;
        CriticalHit = criticalHit;
    }
}

