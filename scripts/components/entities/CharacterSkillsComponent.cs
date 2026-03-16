using Godot;

namespace Scribe.Scripts.Data.ComponentData;

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
