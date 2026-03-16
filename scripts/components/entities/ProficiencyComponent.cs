using Godot;
using Godot.Collections;
using Scribe.Scripts.Data;
using Scribe.Scripts.Data.ComponentData;

/// <summary>
/// Implementation of ProficiencyComponent.
/// Tracks proficiency bonus and weapon proficiencies based on D&D 5e rules.
/// </summary>
public partial class ProficiencyComponent : RefCounted, IEntityComponent
{
    public int ProficiencyBonus { get; set; }

    /// <summary>
    /// Weapon categories this entity is proficient with.
    /// </summary>
    public Array<WeaponCategory> WeaponCategories { get; set; } = new();

    /// <summary>
    /// Specific weapons this entity is proficient with by name.
    /// </summary>
    public Array<string> SpecificWeapons { get; set; } = new();

    /// <summary>
    /// Check if this entity is proficient with a given attack.
    /// Natural attacks are always proficient.
    /// </summary>
    /// <param name="attackData">The attack data to check proficiency for</param>
    /// <param name="isNaturalAttack">Whether this is a natural attack (bite, claws, etc.)</param>
    /// <returns>True if proficient, false otherwise</returns>
    public bool IsProficientWith(MeleeAttackData attackData, bool isNaturalAttack = false)
    {
        // Natural attacks are always proficient
        if (isNaturalAttack || attackData.IsNaturalWeapon)
            return true;

        // Check specific weapon by name
        if (SpecificWeapons.Contains(attackData.AttackName))
            return true;

        // Check by weapon category
        if (attackData.Category != WeaponCategory.None &&
            WeaponCategories.Contains(attackData.Category))
            return true;

        return false;
    }
}
