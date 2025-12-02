using Godot.Collections;
using Scribe.Scripts.Core.Interfaces.Entities;
using Scribe.Scripts.Entities;

namespace Scribe.Scripts.Data.ComponentData.Traits;

/// <summary>
/// Sunlight Sensitivity: Disadvantage on attack rolls and Perception checks in sunlight.
/// </summary>
public partial class SunlightSensitivityTrait : BaseTrait
{
    public SunlightSensitivityTrait(TraitInfo info) : base(info)
    {
    }

    public override int ModifyAttackRoll(Entity target, Entity entity,
        Dictionary context = null)
    {
        if (IsInSunlight(context))
        {
            return -1;  // Disadvantage
        }
        return 0;
    }

    public override int ModifySkillCheck(Skill skill, int baseModifier, Entity entity,
        Dictionary context = null)
    {
        if (skill == Skill.Perception && IsInSunlight(context))
        {
            // TODO: When advantage/disadvantage system is implemented, return disadvantage flag
            // For now, apply a flat -5 penalty to simulate disadvantage
            return baseModifier - 5;
        }
        return baseModifier;
    }

    private bool IsInSunlight(Dictionary context)
    {
        if (context == null) return false;
        if (context.ContainsKey("Lighting"))
        {
            return context["Lighting"].AsString() == "Sunlight";
        }
        return false;  // Default: no sunlight in dungeons
    }
}
