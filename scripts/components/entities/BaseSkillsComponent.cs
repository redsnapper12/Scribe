using Godot;
using Godot.Collections;

namespace Scribe.Scripts.Data.ComponentData;

/// <summary>
/// Base class for skills components with shared calculation logic.
/// </summary>
public abstract partial class BaseSkillsComponent : RefCounted, IEntityComponent
{
    public Array<Skill> Proficiencies { get; set; } = new();

    public bool IsProficient(Skill skill)
    {
        return Proficiencies.Contains(skill);
    }

    public abstract int GetSkillModifier(Skill skill, AbilityScoresComponent abilityScores, int proficiencyBonus);

    /// <summary>
    /// Standard D&D 5e skill calculation: ability modifier + proficiency bonus (if proficient).
    /// </summary>
    protected int CalculateStandardModifier(Skill skill, AbilityScoresComponent abilityScores, int proficiencyBonus)
    {
        AbilityScore linkedAbility = GetLinkedAbility(skill);
        int abilityMod = abilityScores.GetModifier(linkedAbility);
        int profBonus = IsProficient(skill) ? proficiencyBonus : 0;

        return abilityMod + profBonus;
    }

    /// <summary>
    /// Maps each skill to its associated ability score (D&D 5e standard).
    /// </summary>
    protected AbilityScore GetLinkedAbility(Skill skill)
    {
        return skill switch
        {
            Skill.Athletics => AbilityScore.Strength,

            Skill.Acrobatics => AbilityScore.Dexterity,
            Skill.SleightOfHand => AbilityScore.Dexterity,
            Skill.Stealth => AbilityScore.Dexterity,

            Skill.Arcana => AbilityScore.Intelligence,
            Skill.History => AbilityScore.Intelligence,
            Skill.Investigation => AbilityScore.Intelligence,
            Skill.Nature => AbilityScore.Intelligence,
            Skill.Religion => AbilityScore.Intelligence,

            Skill.AnimalHandling => AbilityScore.Wisdom,
            Skill.Insight => AbilityScore.Wisdom,
            Skill.Medicine => AbilityScore.Wisdom,
            Skill.Perception => AbilityScore.Wisdom,
            Skill.Survival => AbilityScore.Wisdom,

            Skill.Deception => AbilityScore.Charisma,
            Skill.Intimidation => AbilityScore.Charisma,
            Skill.Performance => AbilityScore.Charisma,
            Skill.Persuasion => AbilityScore.Charisma,

            _ => AbilityScore.Strength
        };
    }
}
