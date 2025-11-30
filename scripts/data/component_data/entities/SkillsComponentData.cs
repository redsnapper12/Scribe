using Godot;
using Godot.Collections;
using Scribe.Scripts.Core.Interfaces;
using Scribe.Scripts.Core.Interfaces.Entities;

namespace Scribe.Scripts.Data.ComponentData;

[GlobalClass]
public partial class SkillsComponentData : EntityComponentData
{
    [Export] public Array<Skill> Proficiencies { get; set; } = new();

    public override IEntityComponent CreateComponent()
    {
        return new SkillsComponent
        {
            Proficiencies = this.Proficiencies
        };
    }
}

public partial class SkillsComponent : RefCounted, IEntityComponent, ISkills
{
    public Array<Skill> Proficiencies { get; set; } = new();

    public bool IsProficient(Skill skill)
    {
        return Proficiencies.Contains(skill);
    }

    public int GetSkillModifier(Skill skill, IAbilityScores abilityScores, int proficiencyBonus)
    {
        AbilityScore linkedAbility = GetLinkedAbility(skill);
        int abilityMod = abilityScores.GetModifier(linkedAbility);
        int profBonus = IsProficient(skill) ? proficiencyBonus : 0;

        return abilityMod + profBonus;
    }

    private AbilityScore GetLinkedAbility(Skill skill)
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
