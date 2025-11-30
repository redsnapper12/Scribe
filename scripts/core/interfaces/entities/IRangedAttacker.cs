using Scribe.Scripts.Core.Interfaces;

namespace Scribe.Scripts.Core.Interfaces.Entities;

public interface IRangedAttacker : IEntityComponent
{
    int AttackBonus { get; set; }
    int NumDice { get; set; }
    DieType DieType { get; set; }
    int DamageBonus { get; set; }
    DamageType DamageType { get; set; }
    int NormalRange { get; set; }
    int MaxRange { get; set; }
    string AttackName { get; set; }

    AttackResult RangedAttack(IDamageable target, int distance);
}
