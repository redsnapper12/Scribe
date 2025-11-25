using Godot;
using Godot.Collections;
using Scribe.Scripts.Core;
using Scribe.Scripts.Core.Entities;
using Scribe.Scripts.Core.Interfaces;

namespace Scribe.Scripts.AI.Behaviors;

public partial class SimpleAggressiveBehavior : RefCounted, IAIBehavior
{
    private int _movementCells = 6;  // Default 30ft = 6 cells
    
    public void Initialize(Dictionary parameters)
    {
        if (parameters.ContainsKey("movement_speed"))
        {
            // Convert feet to cells
            int feet = parameters["movement_speed"].AsInt32();
            _movementCells = feet / GridManager.FEET_PER_CELL;
        }
    }
    
    public void Execute(Entity self, BattleContext context)
    {
        var target = context.FindNearestEnemy(self);
        
        if (target == null)
            return;
        
        if (context.IsInMeleeRange(self, target))
        {
            if (self is IMeleeAttacker attacker && target is IDamageable damageable)
            {
                var result = attacker.MeleeAttack(damageable);
                
                GD.Print($"{self.Name} attacks {target.Name}!");
                if (result.Hit)
                {
                    GD.Print($"  Hit! Rolled {result.AttackRoll}, dealt {result.Damage} damage{(result.CriticalHit ? " (CRITICAL!)" : "")}");
                }
                else
                {
                    GD.Print($"  Miss! Rolled {result.AttackRoll}");
                }
            }
        }
        else
        {
            context.MoveToward(self, target.GridPosition, _movementCells);
            GD.Print($"{self.Name} moves toward {target.Name}");
        }
    }
}