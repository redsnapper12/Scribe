using Godot;

namespace Scribe.Scripts.Data.ComponentData;

/// <summary>
/// Skills component for player characters. Always calculates skill modifiers from ability scores.
/// Use this for entities that should dynamically calculate skills based on their stats.
/// </summary>
[GlobalClass]
public partial class CharacterSkillsComponentData : BaseSkillsComponentData
{
    public override IEntityComponent CreateComponent()
    {
        return new CharacterSkillsComponent
        {
            Proficiencies = this.Proficiencies
        };
    }
}

/// <summary>
/// Runtime skills component for characters. Always uses automatic calculation.
/// </summary>
public partial class CharacterSkillsComponent : BaseSkillsComponent
{
    public override int GetSkillModifier(Skill skill, AbilityScoresComponent abilityScores, int proficiencyBonus)
    {
        return CalculateStandardModifier(skill, abilityScores, proficiencyBonus);
    }
}
