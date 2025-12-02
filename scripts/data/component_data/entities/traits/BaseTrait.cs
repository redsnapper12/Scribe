using Godot;
using Godot.Collections;
using Scribe.Scripts.Core.Interfaces.Entities;
using Scribe.Scripts.Entities;

namespace Scribe.Scripts.Data.ComponentData.Traits;

/// <summary>
/// Base class for all trait implementations.
/// Traits can provide passive bonuses, conditional effects, and active abilities.
/// Subclasses override hook methods to implement mechanical effects.
/// </summary>
public abstract partial class BaseTrait : RefCounted
{
    protected TraitInfo Info { get; private set; }

    public string Name => Info.Name;
    public string Description => Info.Description;
    public TraitType Type => Info.Type;

    protected BaseTrait(TraitInfo info)
    {
        Info = info;
    }

    // === HOOK METHODS: Subclasses override to provide mechanical effects ===

    /// <summary>
    /// Called when entity makes a skill check. Return modified roll modifier.
    /// </summary>
    /// <param name="skill">The skill being checked</param>
    /// <param name="baseModifier">The base skill modifier</param>
    /// <param name="entity">The entity making the check</param>
    /// <param name="context">Additional context (e.g., lighting conditions)</param>
    /// <returns>Modified modifier (base implementation returns unmodified)</returns>
    public virtual int ModifySkillCheck(Skill skill, int baseModifier, Entity entity,
        Dictionary context = null)
    {
        return baseModifier;  // No modification by default
    }

    /// <summary>
    /// Called when entity makes an attack roll. Returns advantage state.
    /// </summary>
    /// <param name="target">The target being attacked</param>
    /// <param name="entity">The attacking entity</param>
    /// <param name="context">Battle context for ally positioning, lighting, etc.</param>
    /// <returns>Advantage state: 0 = normal, 1 = advantage, -1 = disadvantage</returns>
    public virtual int ModifyAttackRoll(Entity target, Entity entity,
        Dictionary context = null)
    {
        return 0;  // Normal roll by default
    }

    /// <summary>
    /// Returns available bonus actions this trait provides.
    /// </summary>
    /// <returns>Array of action names (e.g., ["Disengage", "Hide"])</returns>
    public virtual string[] GetBonusActions()
    {
        return System.Array.Empty<string>();
    }

    /// <summary>
    /// Called when a bonus action from this trait is used.
    /// </summary>
    /// <param name="actionName">Name of the bonus action being executed</param>
    /// <param name="entity">The entity using the bonus action</param>
    /// <param name="context">Additional context for the action</param>
    public virtual void ExecuteBonusAction(string actionName, Entity entity,
        Dictionary context = null)
    {
        // Base implementation does nothing
    }
}
