using Scribe.Scripts.AI;
using Scribe.Scripts.Data.ComponentData;
using Scribe.Scripts.Entities;

namespace Scribe.Scripts.Core.Interfaces.Combat;

/// <summary>
/// Interface for actions that can be taken during combat.
/// Represents standard D&D 5e actions like Attack, Dash, Dodge, etc.
/// </summary>
public interface IUsableAction
{
    /// <summary>
    /// Display name of the action.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Description of what the action does.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// The action economy cost (Action, BonusAction, Reaction, or None).
    /// </summary>
    ActionCost Cost { get; }

    /// <summary>
    /// Whether this action requires selecting a target.
    /// </summary>
    bool RequiresTarget { get; }

    /// <summary>
    /// Checks if the action can be executed by the given entity.
    /// </summary>
    /// <param name="user">The entity attempting to use the action</param>
    /// <param name="context">The battle context</param>
    /// <returns>True if the action can be executed</returns>
    bool CanExecute(Entity user, BattleContext context);

    /// <summary>
    /// Executes the action.
    /// </summary>
    /// <param name="user">The entity using the action</param>
    /// <param name="context">The battle context</param>
    /// <param name="target">Optional target entity (if RequiresTarget is true)</param>
    void Execute(Entity user, BattleContext context, Entity target = null);
}
