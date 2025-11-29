using Scribe.Scripts.Core.Interfaces;

namespace Scribe.Scripts.Items.Components;

/// <summary>
/// Optional component for items that can be used as weapons.
/// </summary>
public interface IWeapon : IItemComponent
{
    /// <summary>
    /// Number of damage dice.
    /// </summary>
    int NumDice { get; }

    /// <summary>
    /// Type of damage die.
    /// </summary>
    DieType DieType { get; }

    /// <summary>
    /// Bonus damage added to rolls.
    /// </summary>
    int DamageBonus { get; }

    /// <summary>
    /// Type of damage dealt.
    /// </summary>
    DamageType DamageType { get; }

    /// <summary>
    /// Attack bonus for hit rolls.
    /// </summary>
    int AttackBonus { get; }

    /// <summary>
    /// Range for melee attacks (in feet).
    /// </summary>
    int MeleeRange { get; }

    /// <summary>
    /// Name of the attack.
    /// </summary>
    string AttackName { get; }

    /// <summary>
    /// Whether this is a two-handed weapon.
    /// </summary>
    bool IsTwoHanded { get; }
}
