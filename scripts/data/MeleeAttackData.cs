using Godot;
using Godot.Collections;
using Scribe.Scripts.Data.ComponentData;

namespace Scribe.Scripts.Data;

/// <summary>
/// Represents a single melee attack option, used by both weapons and natural attacks.
/// Contains all the parameters needed to execute a melee attack with D&D 5e rules.
/// </summary>
[GlobalClass]
public partial class MeleeAttackData : Resource
{
    /// <summary>
    /// Display name for this attack (e.g., "Slash", "Bite", "Claw")
    /// </summary>
    [Export] public string AttackName { get; set; } = "Attack";

    /// <summary>
    /// Number of damage dice (e.g., 1 for 1d8, 2 for 2d6)
    /// </summary>
    [Export] public int NumDice { get; set; } = 1;

    /// <summary>
    /// Type of damage die (D4, D6, D8, D10, D12, D20)
    /// </summary>
    [Export] public DieType DieType { get; set; } = DieType.D6;

    /// <summary>
    /// Type of damage dealt (Slashing, Piercing, Bludgeoning, etc.)
    /// </summary>
    [Export] public DamageType DamageType { get; set; } = DamageType.Slashing;

    /// <summary>
    /// Melee range in feet (usually 5ft, 10ft for Reach weapons)
    /// </summary>
    [Export] public int MeleeRange { get; set; } = 5;

    /// <summary>
    /// Static attack bonus from magic weapons (e.g., +1 for a +1 sword)
    /// </summary>
    [Export] public int AttackBonus { get; set; } = 0;

    /// <summary>
    /// Static damage bonus from magic weapons (e.g., +1 for a +1 sword)
    /// </summary>
    [Export] public int DamageBonus { get; set; } = 0;

    /// <summary>
    /// Weapon properties (Finesse, Reach, TwoHanded, etc.)
    /// </summary>
    [Export] public Array<WeaponProperties> Properties { get; set; } = new();

    /// <summary>
    /// Weapon proficiency category (Simple, Martial, or None for natural weapons)
    /// </summary>
    [Export] public WeaponCategory Category { get; set; } = WeaponCategory.Simple;

    /// <summary>
    /// Flag indicating if this is a natural weapon (bite, claws, etc.).
    /// Natural weapons are always proficient regardless of character proficiencies.
    /// </summary>
    [Export] public bool IsNaturalWeapon { get; set; } = false;

    /// <summary>
    /// The action economy cost to use this attack.
    /// Most attacks cost an Action, but some (like bonus action attacks) may differ.
    /// </summary>
    [Export] public ActionCost ActionCost { get; set; } = ActionCost.Action;

    public MeleeAttackData()
    {
    }

    public MeleeAttackData(string attackName, int numDice, DieType dieType, DamageType damageType, int meleeRange = 5)
    {
        AttackName = attackName;
        NumDice = numDice;
        DieType = dieType;
        DamageType = damageType;
        MeleeRange = meleeRange;
    }
}
