using Godot;
using System;

namespace Scribe.Scripts.Data.ComponentData;

/// <summary>
/// Defines the action cost type for abilities and actions.
/// </summary>
public enum ActionCost
{
    None,        // Free action (dropping weapon, speaking)
    Action,      // Standard action (Attack, Cast Spell, Dash, etc.)
    BonusAction, // Bonus action (Two-Weapon Fighting, certain spells)
    Reaction     // Reaction (Attack of Opportunity, Shield spell)
}

[GlobalClass]
public partial class ActionEconomyComponentData : EntityComponentData
{
    [Export] public int BaseActions { get; set; } = 1;
    [Export] public int BaseBonusActions { get; set; } = 1;
    [Export] public int BaseReactions { get; set; } = 1;
    [Export] public int BaseAttacksPerAction { get; set; } = 1;

    public override IEntityComponent CreateComponent()
    {
        return new ActionEconomyComponent
        {
            BaseActions = this.BaseActions,
            BaseBonusActions = this.BaseBonusActions,
            BaseReactions = this.BaseReactions,
            BaseAttacksPerAction = this.BaseAttacksPerAction,
            ActionsRemaining = this.BaseActions,
            BonusActionsRemaining = this.BaseBonusActions,
            ReactionsRemaining = this.BaseReactions,
            AttacksRemainingThisAction = 0
        };
    }
}
