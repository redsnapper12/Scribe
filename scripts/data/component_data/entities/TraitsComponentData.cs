using Godot;
using Godot.Collections;
using System.Linq;
using Scribe.Scripts.Core.Interfaces;
using Scribe.Scripts.Core.Interfaces.Entities;
using Scribe.Scripts.Data.ComponentData.Traits;
using Scribe.Scripts.Entities;

namespace Scribe.Scripts.Data.ComponentData;

[GlobalClass]
public partial class TraitsComponentData : EntityComponentData
{
    [Export] public Array<TraitInfo> Traits { get; set; } = [];

    public override IEntityComponent CreateComponent()
    {
        // NEW: Instantiate runtime trait objects from TraitInfo data
        var runtimeTraits = new Array<BaseTrait>();
        foreach (var traitInfo in Traits)
        {
            if (traitInfo != null)
            {
                runtimeTraits.Add(traitInfo.CreateTrait());
            }
        }

        return new TraitsComponent
        {
            TraitInfos = this.Traits,
            RuntimeTraits = runtimeTraits
        };
    }
}

public partial class TraitsComponent : RefCounted, IEntityComponent, ITraits
{
    // Keep original for backward compatibility / display
    public Array<TraitInfo> TraitInfos { get; set; } = new();

    // NEW: Runtime trait instances with behavior
    public Array<BaseTrait> RuntimeTraits { get; set; } = new();

    public bool HasTrait(string traitName)
    {
        return TraitInfos.Any(t => t.Name == traitName);
    }

    // NEW: Type-safe trait check
    public bool HasTrait(TraitType type)
    {
        return RuntimeTraits.Any(t => t.Type == type);
    }

    // NEW: Get trait by type
    public BaseTrait GetTrait(TraitType type)
    {
        return RuntimeTraits.FirstOrDefault(t => t.Type == type);
    }

    // === TRAIT EFFECT METHODS ===

    /// <summary>
    /// Apply all trait effects to a skill check.
    /// </summary>
    public int ApplyTraitSkillModifiers(Skill skill, int baseModifier, Entity entity,
        Dictionary context = null)
    {
        int modified = baseModifier;
        foreach (var trait in RuntimeTraits)
        {
            modified = trait.ModifySkillCheck(skill, modified, entity, context);
        }
        return modified;
    }

    /// <summary>
    /// Check if traits grant advantage/disadvantage on attack.
    /// Returns: 1 = advantage, 0 = normal, -1 = disadvantage
    /// </summary>
    public int GetAttackRollModifier(Entity target, Entity entity,
        Dictionary context = null)
    {
        int result = 0;
        foreach (var trait in RuntimeTraits)
        {
            int traitResult = trait.ModifyAttackRoll(target, entity, context);
            // Priority: disadvantage overrides advantage
            if (traitResult < 0) result = -1;
            else if (result == 0 && traitResult > 0) result = 1;
        }
        return result;
    }

    /// <summary>
    /// Get all bonus actions available from traits.
    /// </summary>
    public string[] GetAvailableBonusActions()
    {
        var actions = new System.Collections.Generic.List<string>();
        foreach (var trait in RuntimeTraits)
        {
            actions.AddRange(trait.GetBonusActions());
        }
        return actions.ToArray();
    }

    /// <summary>
    /// Execute a trait-provided bonus action.
    /// </summary>
    public void ExecuteBonusAction(string actionName, Entity entity,
        Dictionary context = null)
    {
        foreach (var trait in RuntimeTraits)
        {
            var available = trait.GetBonusActions();
            if (available.Contains(actionName))
            {
                trait.ExecuteBonusAction(actionName, entity, context);
                return;
            }
        }
    }
}
