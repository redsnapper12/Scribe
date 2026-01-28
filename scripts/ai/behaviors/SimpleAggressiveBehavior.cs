using Godot;
using Godot.Collections;
using Scribe.Scripts.Core;
using Scribe.Scripts.Core.Interfaces;
using Scribe.Scripts.Core.Services;
using Scribe.Scripts.Entities;
using Scribe.Scripts.Data;
using Scribe.Scripts.Data.ComponentData;

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
                    GD.Print($"{self.EntityName} moves toward {target.EntityName}");

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
                if (context.GridManager.IsCellOccupied(candidate, GameManager.Instance.AllEntities, self))
                    continue;

                // Check if we can actually attack the target from this position
                // No point moving here if walls block the attack
                if (!context.GridManager.CanAttackAcross(candidate, targetPos))
                    continue;

                var path = context.GridManager.FindPath(
                    selfPos, 
                    candidate, 
                    pos => context.GridManager.IsCellOccupied(pos, GameManager.Instance.AllEntities, self)
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
        if (!self.TryGetComponent<AttackComponent>(out var meleeAttack))
            return;

        if (!target.TryGetComponent<HealthComponent>(out var targetHealth))
            return;

        // TODO: Replace with an attack selection method.
        MeleeAttackData attackData;
        if (target.TryGetComponent<EquipmentComponent>(out var equipment))
        {
            attackData = equipment.GetMainHandWeaponMeleeData();
        }
        else if(meleeAttack.NaturalAttacks.Count > 0)
        {
            
            attackData = meleeAttack.NaturalAttacks[0];
        }
        else
        {
            return;
        }

        AttackResult result = meleeAttack.MeleeAttack(self, targetHealth, attackData);

        GD.Print($"{self.EntityName} attacks {target.EntityName}!");
        if (result.Hit)
        {
            GD.Print($"  Hit! Rolled {result.AttackRoll}, dealt {result.Damage} damage{(result.IsCritical ? " (CRITICAL!)" : "")}");
        }
        else
        {
            GD.Print($"  Miss! Rolled {result.AttackRoll}");
        }
    }
}