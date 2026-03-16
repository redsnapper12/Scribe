using Godot;
using System;

namespace Scribe.Scripts.Data.ComponentData;

/// <summary>
/// Tracks action economy resources per turn/round following D&D 5e rules.
/// Actions and Bonus Actions reset at turn start.
/// Reactions reset at round start.
/// </summary>
public partial class ActionEconomyComponent : RefCounted, IEntityComponent
{
    // Base values (from ComponentData, can be modified by class features)
    public int BaseActions { get; set; } = 1;
    public int BaseBonusActions { get; set; } = 1;
    public int BaseReactions { get; set; } = 1;
    public int BaseAttacksPerAction { get; set; } = 1;

    // Temporary bonus modifiers (from Action Surge, Haste, etc.)
    public int BonusActions { get; set; } = 0;
    public int BonusBonusActions { get; set; } = 0;
    public int BonusReactions { get; set; } = 0;
    public int BonusAttacksPerAction { get; set; } = 0;

    // Current remaining resources
    public int ActionsRemaining { get; set; }
    public int BonusActionsRemaining { get; set; }
    public int ReactionsRemaining { get; set; }

    // Extra Attack tracking
    public int AttacksRemainingThisAction { get; set; }
    public bool IsInAttackAction { get; set; } = false;

    // Computed totals
    public int TotalActions => BaseActions + BonusActions;
    public int TotalBonusActions => BaseBonusActions + BonusBonusActions;
    public int TotalReactions => BaseReactions + BonusReactions;
    public int TotalAttacksPerAction => BaseAttacksPerAction + BonusAttacksPerAction;

    // Signals
    [Signal]
    public delegate void ActionSpentEventHandler(ActionCost actionType);

    [Signal]
    public delegate void ActionsExhaustedEventHandler();

    [Signal]
    public delegate void TurnResetEventHandler();

    [Signal]
    public delegate void RoundResetEventHandler();

    [Signal]
    public delegate void AttackUsedEventHandler(int attacksRemaining);

    /// <summary>
    /// Checks if the entity has an action available.
    /// </summary>
    public bool HasAction() => ActionsRemaining > 0;

    /// <summary>
    /// Checks if the entity has a bonus action available.
    /// </summary>
    public bool HasBonusAction() => BonusActionsRemaining > 0;

    /// <summary>
    /// Checks if the entity has a reaction available.
    /// </summary>
    public bool HasReaction() => ReactionsRemaining > 0;

    /// <summary>
    /// Checks if the entity can afford a specific action cost.
    /// </summary>
    public bool CanAfford(ActionCost cost)
    {
        return cost switch
        {
            ActionCost.None => true,
            ActionCost.Action => HasAction(),
            ActionCost.BonusAction => HasBonusAction(),
            ActionCost.Reaction => HasReaction(),
            _ => false
        };
    }

    /// <summary>
    /// Spends an action resource. Returns true if successful.
    /// </summary>
    public bool SpendAction(ActionCost cost)
    {
        if (!CanAfford(cost)) return false;

        switch (cost)
        {
            case ActionCost.Action:
                ActionsRemaining--;
                break;
            case ActionCost.BonusAction:
                BonusActionsRemaining--;
                break;
            case ActionCost.Reaction:
                ReactionsRemaining--;
                break;
            case ActionCost.None:
                return true;
        }

        EmitSignal(SignalName.ActionSpent, (int)cost);

        if (ActionsRemaining == 0 && BonusActionsRemaining == 0)
        {
            EmitSignal(SignalName.ActionsExhausted);
        }

        return true;
    }

    /// <summary>
    /// Begins an Attack action (for Extra Attack handling).
    /// Sets up the attack budget based on attacks per action.
    /// </summary>
    public bool BeginAttackAction()
    {
        if (!HasAction()) return false;

        IsInAttackAction = true;
        AttacksRemainingThisAction = TotalAttacksPerAction;
        return true;
    }

    /// <summary>
    /// Uses one attack within the current Attack action.
    /// Automatically ends the Attack action when all attacks are used.
    /// </summary>
    public bool UseAttack()
    {
        if (!IsInAttackAction || AttacksRemainingThisAction <= 0)
            return false;

        AttacksRemainingThisAction--;
        EmitSignal(SignalName.AttackUsed, AttacksRemainingThisAction);

        if (AttacksRemainingThisAction == 0)
        {
            EndAttackAction();
        }

        return true;
    }

    /// <summary>
    /// Ends the current Attack action, spending the Action resource.
    /// Called automatically when all attacks are used, or manually to end early.
    /// </summary>
    public void EndAttackAction()
    {
        if (IsInAttackAction)
        {
            IsInAttackAction = false;
            AttacksRemainingThisAction = 0;
            SpendAction(ActionCost.Action);
        }
    }

    /// <summary>
    /// Checks if the entity can currently attack (has action or is in attack action with attacks remaining).
    /// </summary>
    public bool CanAttack()
    {
        if (IsInAttackAction)
            return AttacksRemainingThisAction > 0;
        return HasAction();
    }

    /// <summary>
    /// Resets Actions and Bonus Actions at turn start.
    /// Clears temporary bonuses and attack action state.
    /// </summary>
    public void ResetTurn()
    {
        ActionsRemaining = TotalActions;
        BonusActionsRemaining = TotalBonusActions;
        IsInAttackAction = false;
        AttacksRemainingThisAction = 0;

        // Clear temporary turn-based bonuses
        BonusActions = 0;
        BonusBonusActions = 0;

        EmitSignal(SignalName.TurnReset);
    }

    /// <summary>
    /// Resets Reactions at round start.
    /// </summary>
    public void ResetRound()
    {
        ReactionsRemaining = TotalReactions;
        BonusReactions = 0;

        EmitSignal(SignalName.RoundReset);
    }

    /// <summary>
    /// Grants additional actions this turn (e.g., Action Surge).
    /// </summary>
    public void GrantBonusAction(int count = 1)
    {
        BonusActions += count;
        ActionsRemaining += count;
    }

    /// <summary>
    /// Grants additional bonus actions this turn.
    /// </summary>
    public void GrantExtraBonusAction(int count = 1)
    {
        BonusBonusActions += count;
        BonusActionsRemaining += count;
    }

    /// <summary>
    /// Grants additional reactions this round.
    /// </summary>
    public void GrantExtraReaction(int count = 1)
    {
        BonusReactions += count;
        ReactionsRemaining += count;
    }

    /// <summary>
    /// Sets the base attacks per action (e.g., Extra Attack class feature).
    /// </summary>
    public void SetAttacksPerAction(int attacks)
    {
        BaseAttacksPerAction = Math.Max(1, attacks);
    }
}
