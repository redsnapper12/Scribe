namespace Scribe.Scripts.Core.Interfaces.Entities;

public interface ISkills : IEntityComponent
{
    bool IsProficient(Skill skill);
    int GetSkillModifier(Skill skill, IAbilityScores abilityScores, int proficiencyBonus);
}

public enum Skill
{
    // STR
    Athletics,
    // DEX
    Acrobatics,
    SleightOfHand,
    Stealth,
    // INT
    Arcana,
    History,
    Investigation,
    Nature,
    Religion,
    // WIS
    AnimalHandling,
    Insight,
    Medicine,
    Perception,
    Survival,
    // CHA
    Deception,
    Intimidation,
    Performance,
    Persuasion
}
