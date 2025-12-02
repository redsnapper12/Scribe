using Godot.Collections;
using Scribe.Scripts.Data.ComponentData;
using Scribe.Scripts.Data.ComponentData.Traits;
using Scribe.Scripts.Entities;

namespace Scribe.Scripts.Core.Interfaces.Entities;

public interface ITraits : IEntityComponent
{
    // EXISTING: Keep for backward compatibility / display purposes
    Array<TraitInfo> TraitInfos { get; set; }
    bool HasTrait(string traitName);

    // NEW: Type-safe trait queries
    bool HasTrait(TraitType type);
    BaseTrait GetTrait(TraitType type);

    // NEW: Trait effect application methods
    /// <summary>
    /// Apply all trait effects to a skill check.
    /// </summary>
    int ApplyTraitSkillModifiers(Skill skill, int baseModifier, Entity entity, Dictionary context = null);

    /// <summary>
    /// Check if traits grant advantage/disadvantage on attack.
    /// Returns: 1 = advantage, 0 = normal, -1 = disadvantage
    /// </summary>
    int GetAttackRollModifier(Entity target, Entity entity, Dictionary context = null);

    /// <summary>
    /// Get all bonus actions available from traits.
    /// </summary>
    string[] GetAvailableBonusActions();

    /// <summary>
    /// Execute a trait-provided bonus action.
    /// </summary>
    void ExecuteBonusAction(string actionName, Entity entity, Dictionary context = null);
}
