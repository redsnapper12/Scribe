namespace Scribe.Scripts.Data.ComponentData.Traits;

/// <summary>
/// Defines all available trait types in the game.
/// Each type maps to a concrete BaseTrait implementation with mechanical effects.
/// </summary>
public enum TraitType
{
    /// <summary>
    /// Generic trait with no mechanical effects (purely descriptive).
    /// </summary>
    None,

    /// <summary>
    /// Nimble Escape: Can take Disengage or Hide as a bonus action on each turn.
    /// </summary>
    NimbleEscape,

    /// <summary>
    /// Pack Tactics: Has advantage on attack rolls against a creature if at least one ally
    /// is within 5 feet of the target and isn't incapacitated.
    /// </summary>
    PackTactics,

    /// <summary>
    /// Keen Senses (Hearing and Smell): Has advantage on Perception checks that rely on hearing or smell.
    /// </summary>
    KeenSenses,

    /// <summary>
    /// Sunlight Sensitivity: Has disadvantage on attack rolls and Perception checks when in direct sunlight.
    /// </summary>
    SunlightSensitivity
}
