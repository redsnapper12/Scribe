using Godot;
using Godot.Collections;

namespace Scribe.Scripts.Data.ComponentData;

/// <summary>
/// Skills component for monsters/creatures. Supports skill overrides from D&D 5e stat blocks.
/// Use this for entities with fixed skill bonuses that don't follow the standard formula.
/// Example: Goblin with Stealth +6 (instead of calculated +4).
/// </summary>
[GlobalClass]
public partial class CreatureSkillsComponentData : BaseSkillsComponentData
{
    /// <summary>
    /// Dictionary of skill overrides. Key = Skill enum, Value = Final skill modifier.
    /// When a skill is in this dictionary, the override value is used instead of calculating from ability scores.
    /// </summary>
    [Export] public Dictionary<Skill, int> SkillOverrides { get; set; } = new();

    public override IEntityComponent CreateComponent()
    {
        return new CreatureSkillsComponent
        {
            Proficiencies = this.Proficiencies,
            SkillOverrides = this.SkillOverrides
        };
    }
}
