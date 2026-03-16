using Godot;
using Godot.Collections;

namespace Scribe.Scripts.Data.ComponentData;

/// <summary>
/// Base class for skills component data. Contains shared proficiency tracking.
/// Use CharacterSkillsComponentData for automatic calculation or CreatureSkillsComponentData for stat block overrides.
/// </summary>
public abstract partial class BaseSkillsComponentData : EntityComponentData
{
    [Export] public Array<Skill> Proficiencies { get; set; } = new();
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
