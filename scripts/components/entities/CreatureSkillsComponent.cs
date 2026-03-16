using Godot;
using Godot.Collections;

namespace Scribe.Scripts.Data.ComponentData;

/// <summary>
/// Runtime skills component for creatures. Checks overrides first, then falls back to standard calculation.
/// </summary>
public partial class CreatureSkillsComponent : BaseSkillsComponent
{
    public Dictionary<Skill, int> SkillOverrides { get; set; } = new();

    public override int GetSkillModifier(Skill skill, AbilityScoresComponent abilityScores, int proficiencyBonus)
    {
        // Check for override first
        if (SkillOverrides.ContainsKey(skill))
        {
            return SkillOverrides[skill];
        }

        // Fallback to standard calculation
        return CalculateStandardModifier(skill, abilityScores, proficiencyBonus);
    }
}
