using Godot;
using Scribe.Scripts.AI;
using Scribe.Scripts.Core.Interfaces.Combat;
using Scribe.Scripts.Data;
using Scribe.Scripts.Data.ComponentData;
using Scribe.Scripts.Entities;

namespace Scribe.Scripts.Combat.Actions;

/// <summary>
/// The Attack action allows an entity to make one melee or ranged attack.
/// With Extra Attack, multiple attacks can be made with a single Attack action.
/// </summary>
public class AttackAction : IUsableAction
{
    public string Name => "Attack";
    public string Description => "Make one melee or ranged attack.";
    public ActionCost Cost => ActionCost.Action;
    public bool RequiresTarget => true;

    public bool CanExecute(Entity user, BattleContext context)
    {
        // Check if user has an action economy component and can attack
        if (!user.TryGetComponent<ActionEconomyComponent>(out var actionEconomy))
            return false;

        if (!actionEconomy.CanAttack())
            return false;

        // Check if user has attack component
        if (!user.TryGetComponent<AttackComponent>(out _))
            return false;

        return true;
    }

    public void Execute(Entity user, BattleContext context, Entity target = null)
    {
        if (target == null)
        {
            GD.PrintErr($"{user.EntityName}: Attack action requires a target");
            return;
        }

        if (!CanExecute(user, context))
        {
            GD.PrintErr($"{user.EntityName} cannot use Attack action");
            return;
        }

        // Get components
        if (!user.TryGetComponent<ActionEconomyComponent>(out var actionEconomy))
            return;

        if (!user.TryGetComponent<AttackComponent>(out var attackComponent))
            return;

        if (!target.TryGetComponent<HealthComponent>(out var targetHealth))
        {
            GD.PrintErr($"{target.EntityName} cannot take damage");
            return;
        }

        // Validate melee range
        if (!context.CanMeleeAttack(user, target))
        {
            GD.PrintErr($"Invalid attack: {user.EntityName} cannot attack {target.EntityName} (out of range or blocked)");
            return;
        }

        // Begin attack action if not already in one
        if (!actionEconomy.IsInAttackAction)
        {
            if (!actionEconomy.BeginAttackAction())
            {
                GD.PrintErr($"{user.EntityName} cannot begin attack action");
                return;
            }
        }

        // Get attack data
        MeleeAttackData attackData = GetAttackData(user);
        if (attackData == null)
        {
            GD.PrintErr($"{user.EntityName} has no valid attack");
            actionEconomy.EndAttackAction();
            return;
        }

        // Execute the attack
        var result = attackComponent.MeleeAttack(user, targetHealth, attackData);

        GD.Print($"{user.EntityName} attacks {target.EntityName}!");
        if (result.Hit)
        {
            GD.Print($"  Hit! Rolled {result.AttackRoll}, dealt {result.Damage} damage{(result.IsCritical ? " (CRITICAL!)" : "")}");
        }
        else
        {
            GD.Print($"  Miss! Rolled {result.AttackRoll}");
        }

        // Use one attack from the attack action
        actionEconomy.UseAttack();
    }

    private MeleeAttackData GetAttackData(Entity user)
    {
        // Try to get weapon attack data from equipment
        if (user.TryGetComponent<EquipmentComponent>(out var equipment))
        {
            var weaponData = equipment.GetMainHandWeaponMeleeData();
            if (weaponData != null)
                return weaponData;
        }

        // Fall back to natural attacks
        if (user.TryGetComponent<AttackComponent>(out var attackComponent))
        {
            if (attackComponent.NaturalAttacks.Count > 0)
                return attackComponent.NaturalAttacks[0];
        }

        return null;
    }
}
