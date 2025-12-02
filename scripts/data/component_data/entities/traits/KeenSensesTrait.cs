using Godot.Collections;
using Scribe.Scripts.Core.Interfaces.Entities;
using Scribe.Scripts.Entities;

namespace Scribe.Scripts.Data.ComponentData.Traits;

/// <summary>
/// Keen Hearing and Smell: Advantage on Perception checks that rely on hearing or smell.
/// For simplicity, grants advantage modifier on all Perception checks.
/// </summary>
public partial class KeenSensesTrait : BaseTrait
{
    public KeenSensesTrait(TraitInfo info) : base(info)
    {
    }

    public override int ModifySkillCheck(Skill skill, int baseModifier, Entity entity,
        Dictionary context = null)
    {
        if (skill == Skill.Perception)
        {
            // TODO: When advantage/disadvantage system is implemented, return advantage flag
            // For now, add a flat +5 bonus to simulate advantage (average advantage = ~+5)
            return baseModifier + 5;
        }
        return baseModifier;
    }
}
