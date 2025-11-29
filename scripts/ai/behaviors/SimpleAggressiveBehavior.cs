using Godot;
using Godot.Collections;
using Scribe.Scripts.Core;
using Scribe.Scripts.Core.Entities;
using Scribe.Scripts.Core.Interfaces;

namespace Scribe.Scripts.AI.Behaviors;

public partial class SimpleAggressiveBehavior : RefCounted, IAIBehavior
{
    public void Initialize(Dictionary parameters)
    {
    
    }

    public void Execute(Entity self, BattleContext context)
    {
        var target = context.FindNearestEnemy(self);

        if (target == null)
            return;

        if (context.CanMeleeAttack(self, target))
        {
            PerformAttack(self, target);
        }
        else
        {
            var destination = FindAdjacentCell(self, target, context);

            if (destination.HasValue)
            {
                var request = new MovementRequest(destination.Value, ControllerType.AI);
                var result = MovementService.RequestMove(self, request);

                if (result.Success && result.MovementSpent > 0)
                {
                    GD.Print($"{self.Name} moves toward {target.Name}");

                    if (context.CanMeleeAttack(self, target))
                    {
                        PerformAttack(self, target);
                    }
                }
            }
        }
    }
    
    private Vector2I? FindAdjacentCell(Entity self, Entity target, BattleContext context)
    {
        Vector2I targetPos = target.GridPosition;
        Vector2I selfPos = self.GridPosition;
        Vector2I? bestCell = null;
        int shortestPathLength = int.MaxValue;
        float bestDirectionScore = float.MaxValue;
        
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;
                
                Vector2I candidate = new Vector2I(targetPos.X + dx, targetPos.Y + dy);
                
                if (!context.GridManager.IsValidGridPosition(candidate))
                    continue;
                if (!context.GridManager.IsWalkable(candidate))
                    continue;
                if (GameManager.Instance.IsCellOccupied(candidate, self))
                    continue;

                // Check if we can actually attack the target from this position
                // No point moving here if walls block the attack
                if (!context.GridManager.CanAttackAcross(candidate, targetPos))
                    continue;

                var path = context.GridManager.FindPath(
                    selfPos, 
                    candidate, 
                    pos => GameManager.Instance.IsCellOccupied(pos, self)
                );
                
                if (path != null && path.Count > 0)
                {
                    // Calculate how well this candidate aligns with our approach direction
                    // Lower score = better (candidate is in the direction we're coming from)
                    float directionScore = Mathf.Sqrt(
                        Mathf.Pow(candidate.X - selfPos.X, 2) + 
                        Mathf.Pow(candidate.Y - selfPos.Y, 2)
                    );
                    
                    // Prefer shorter paths, then prefer cells closer to our current position
                    if (path.Count < shortestPathLength || 
                        (path.Count == shortestPathLength && directionScore < bestDirectionScore))
                    {
                        shortestPathLength = path.Count;
                        bestDirectionScore = directionScore;
                        bestCell = candidate;
                    }
                }
            }
        }
        
        return bestCell;
    }

    private void PerformAttack(Entity self, Entity target)
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
}