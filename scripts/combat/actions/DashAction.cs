using Godot;
using Scribe.Scripts.AI;
using Scribe.Scripts.Core.Interfaces.Combat;
using Scribe.Scripts.Data.ComponentData;
using Scribe.Scripts.Entities;

namespace Scribe.Scripts.Combat.Actions;

/// <summary>
/// The Dash action doubles the entity's movement for the current turn.
/// When you take the Dash action, you gain extra movement for the current turn.
/// The increase equals your speed, after applying any modifiers.
/// </summary>
public class DashAction : IUsableAction
{
    public string Name => "Dash";
    public string Description => "Gain extra movement equal to your speed for this turn.";
    public ActionCost Cost => ActionCost.Action;
    public bool RequiresTarget => false;

    public bool CanExecute(Entity user, BattleContext context)
    {
        // Check if user has an action available
        if (!user.TryGetComponent<ActionEconomyComponent>(out var actionEconomy))
            return false;

        if (!actionEconomy.CanAfford(Cost))
            return false;

        // Check if user has movement component
        if (!user.TryGetComponent<MovementComponent>(out _))
            return false;

        return true;
    }

    public void Execute(Entity user, BattleContext context, Entity target = null)
    {
        if (!CanExecute(user, context))
        {
            GD.PrintErr($"{user.EntityName} cannot use Dash action");
            return;
        }

        // Spend the action
        if (user.TryGetComponent<ActionEconomyComponent>(out var actionEconomy))
        {
            actionEconomy.SpendAction(Cost);
        }

        // Add movement equal to walk speed
        if (user.TryGetComponent<MovementComponent>(out var movement))
        {
            int extraMovement = movement.WalkSpeed;
            movement.MovementRemaining += extraMovement;

            GD.Print($"{user.EntityName} uses Dash! Movement increased by {extraMovement} ft (now {movement.MovementRemaining} ft)");
        }
    }
}
