using Godot;
using System;
using Scribe.Scripts.Entities;

namespace Scribe.Scripts.Data.ComponentData;

public partial class AttackComponent : RefCounted, IEntityComponent
{
    /// <summary>
    /// Natural attacks for this entity (bite, claws, talons, etc.).
    /// Empty for characters. Populated for creatures with natural attacks.
    /// Contains ONLY innate biological attacks, NOT equipped weapons.
    /// </summary>
    public Godot.Collections.Array<MeleeAttackData> NaturalAttacks { get; set; } = new();

    /// <summary>
    /// Execute a melee attack using D&D 5e rules with dynamic calculation.
    /// </summary>
    /// <param name="attacker">The entity performing the attack</param>
    /// <param name="target">The target being attacked</param>
    /// <param name="attackData">The attack data to use</param>
    /// <param name="isNaturalAttack">Whether this is a natural attack (bite, claws, etc.)</param>
    public AttackResult MeleeAttack(Entity attacker, HealthComponent target, MeleeAttackData attackData, bool isNaturalAttack = false)
    {
        #region Hit Determination

        int proficiencyBonus = 0;
        int abilityModifier = 0;

        // Check proficiency using IsProficientWith()
        if (attacker.TryGetComponent<ProficiencyComponent>(out var proficiencyComponent))
        {
            bool isProficient = proficiencyComponent.IsProficientWith(attackData, isNaturalAttack);
            proficiencyBonus = isProficient ? proficiencyComponent.ProficiencyBonus : 0;
        }

        if(attacker.TryGetComponent<AbilityScoresComponent>(out var abilityScoresComponent))
        {
            abilityModifier = GetAbilityModifier(abilityScoresComponent, attackData);
        }

        int totalAttackBonus = abilityModifier + proficiencyBonus + attackData.AttackBonus;

        // Get advantage state from traits (stub for now)
        bool hasAdvantage = false;

        // Roll attack
        DiceRollResult diceRollResult = RollAttack(totalAttackBonus, hasAdvantage);
        int attackRoll = diceRollResult.Result;



        // Critical failure
        if (diceRollResult.DiceRollCategory == DiceRollCategory.CriticalFailure)
        {
            return AttackResult.Miss(attackRoll);
        }

        // Determine hit
        bool isCritical = diceRollResult.DiceRollCategory == DiceRollCategory.CriticalSuccess;
        bool hit = isCritical || attackRoll >= target.ArmorClass;

        if (!hit)
        {
            return AttackResult.Miss(attackRoll);
        }

        #endregion

        #region Damage Determination

        // Calculate damage
        int damage = RollDamage(attackData, abilityModifier, isCritical);
        target.TakeDamage(damage, attackData.DamageType);

        return isCritical
            ? AttackResult.CriticalHit(attackRoll, damage)
            : AttackResult.NormalHit(attackRoll, damage);

        #endregion
    }

    /// <summary>
    /// Determine which ability modifier to use based on weapon properties.
    /// Finesse weapons use the higher of STR or DEX. Normal melee weapons use STR.
    /// </summary>
    private int GetAbilityModifier(AbilityScoresComponent abilities, MeleeAttackData attackData)
    {
        if (abilities == null) return 0;

        // Check if weapon has Finesse property
        if (attackData.Properties?.Contains(WeaponProperties.Finesse) ?? false)
        {
            // Use higher of STR or DEX for finesse weapons
            return Math.Max(abilities.StrengthModifier, abilities.DexterityModifier);
        }

        return abilities.StrengthModifier;
    }

    /// <summary>
    /// Roll damage with D&D 5e rules.
    /// Critical hits double the number of dice rolled, not the modifiers.
    /// </summary>
    private int RollDamage(MeleeAttackData attackData, int abilityModifier, bool critical)
    {
        int totalDamage = 0;
        int numDice = critical ? attackData.NumDice * 2 : attackData.NumDice;

        // Roll damage dice
        for (int i = 0; i < numDice; i++)
        {
            totalDamage += DiceRollingService.RequestRoll(new(attackData.DieType, 1, 0)).Result;
        }

        // Add modifiers (ability modifier + weapon damage bonus)
        // Modifiers are NOT doubled on crits
        totalDamage += abilityModifier + attackData.DamageBonus;

        return totalDamage;
    }


    /// <summary>
    /// Roll a d20 attack roll with optional advantage.
    /// </summary>
    /// <param name="attackBonus">Total attack bonus to apply</param>
    /// <param name="advantage">If true, roll twice and take higher</param>
    /// <returns>DiceRollResult with the final attack roll</returns>
    private DiceRollResult RollAttack(int attackBonus, bool advantage)
    {
        if (!advantage)
        {
            return DiceRollingService.RequestRoll(new(DieType.D20, 1, attackBonus));
        }

        // Roll twice for advantage
        DiceRollResult roll1 = DiceRollingService.RequestRoll(new(DieType.D20, 1, attackBonus));
        DiceRollResult roll2 = DiceRollingService.RequestRoll(new(DieType.D20, 1, attackBonus));

        // Take higher roll
        return roll1.Result >= roll2.Result ? roll1 : roll2;
    }
}
