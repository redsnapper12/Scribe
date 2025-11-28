using Godot;
using System;
using System.Collections.Generic;
using Scribe.Scripts.Core.Components;
using Scribe.Scripts.Core.Entities;

namespace Scribe.Scripts.Core;

#region Enums and Types

public enum ControllerType
{
    Player,     // A player controls this entity
    AI,         // AIComponent controls this entity
    DM          // DM controls this entity manually
}

public enum MovementMode
{
    Combat,         // Movement is budgeted per turn
    Exploration,    // Free movement, no budget tracking
    Reposition      // DM override, ignores all rules
}

public enum MovementFailureReason
{
    None,
    NotAuthorized,
    NotEntityTurn,
    NoPath,
    InsufficientMovement,
    InvalidTarget,
    NoMovementComponent
}

public struct MovementRequest
{
    public Vector2I TargetPosition;
    public ControllerType RequestedBy;
    public bool IsReposition;
    
    public MovementRequest(Vector2I target, ControllerType requestedBy, bool isReposition = false)
    {
        TargetPosition = target;
        RequestedBy = requestedBy;
        IsReposition = isReposition;
    }
}

public struct MovementResult
{
    public bool Success;
    public MovementFailureReason FailureReason;
    public List<Vector2I> PathTaken;
    public int MovementSpent;
    public int MovementRemaining;
    
    public static MovementResult Succeeded(List<Vector2I> path, int spent, int remaining)
    {
        return new MovementResult
        {
            Success = true,
            FailureReason = MovementFailureReason.None,
            PathTaken = path,
            MovementSpent = spent,
            MovementRemaining = remaining
        };
    }
    
    public static MovementResult Failed(MovementFailureReason reason)
    {
        return new MovementResult
        {
            Success = false,
            FailureReason = reason,
            PathTaken = new List<Vector2I>(),
            MovementSpent = 0,
            MovementRemaining = 0
        };
    }
}

#endregion

public static class MovementService
{
    private static GameManager _gameManager;
    private static GridManager _gridManager;
    private static bool _initialized = false;
    
    public static void Initialize(GameManager gameManager, GridManager gridManager)
    {
        _gameManager = gameManager;
        _gridManager = gridManager;
        _initialized = true;
    }
    
    public static MovementResult RequestMove(Entity entity, MovementRequest request)
    {
        if (!_initialized)
        {
            GD.PrintErr("MovementService not initialized!");
            return MovementResult.Failed(MovementFailureReason.None);
        }
        
        // DM reposition bypasses all validation
        if (request.IsReposition && request.RequestedBy == ControllerType.DM)
        {
            return ExecuteReposition(entity, request.TargetPosition);
        }
        
        // Validate authority
        if (!_gameManager.HasAuthority(entity, request.RequestedBy))
        {
            return MovementResult.Failed(MovementFailureReason.NotAuthorized);
        }
        
        // Validate entity has movement component
        var movementComponent = entity.GetComponent<MovementComponent>();
        if (movementComponent == null)
        {
            return MovementResult.Failed(MovementFailureReason.NoMovementComponent);
        }
        
        // Validate target position
        if (!_gridManager.IsValidGridPosition(request.TargetPosition))
        {
            return MovementResult.Failed(MovementFailureReason.InvalidTarget);
        }

        if (!_gridManager.IsWalkable(request.TargetPosition))
        {
            return MovementResult.Failed(MovementFailureReason.InvalidTarget);
        }

        var path = _gridManager.FindPath(
            entity.GridPosition,
            request.TargetPosition,
            pos => _gameManager.IsCellOccupied(pos, entity)
        );
        
        if (path == null || path.Count == 0)
        {
            return MovementResult.Failed(MovementFailureReason.NoPath);
        }
        
        // In combat, validate turn and budget
        if (_gameManager.IsInCombat)
        {
            if (!_gameManager.IsEntityTurn(entity))
            {
                return MovementResult.Failed(MovementFailureReason.NotEntityTurn);
            }
            
            return ExecuteCombatMove(entity, movementComponent, path);
        }
        else
        {
            return ExecuteExplorationMove(entity, movementComponent, path);
        }
    }
    
    private static MovementResult ExecuteReposition(Entity entity, Vector2I target)
    {
        var from = entity.GridPosition;
        entity.GridPosition = target;
        
        var path = new List<Vector2I> { from, target };
        
        GD.Print($"[Reposition] {entity.Name} moved from {from} to {target}");
        
        return MovementResult.Succeeded(path, 0, entity.MovementRemaining);
    }
    
    private static MovementResult ExecuteCombatMove(Entity entity, MovementComponent movement, List<Vector2I> fullPath)
    {
        var from = entity.GridPosition;
        int totalCost = 0;
        var actualPath = new List<Vector2I> { from };
        Vector2I finalPosition = from;

        // Walk along path, spending movement for each cell
        for (int i = 1; i < fullPath.Count; i++)
        {
            var nextCell = fullPath[i];
            int cellCost = _gridManager.GetCellMovementCost(nextCell);
            
            if (!movement.CanAffordMove(cellCost)) break;
            
            movement.SpendMovement(cellCost);
            totalCost += cellCost;
            actualPath.Add(nextCell);
            finalPosition = nextCell;
        }
        
        // Update entity position
        entity.GridPosition = finalPosition;
        
        // Notify component for signal emission
        if (totalCost > 0)
        {
            movement.NotifyMoved(from, finalPosition, totalCost);
            GD.Print($"[Combat Move] {entity.Name}: {from} -> {finalPosition} (cost: {totalCost}ft, remaining: {movement.MovementRemaining}ft)");
        }
        
        // Consider it success even if partial movement
        return MovementResult.Succeeded(actualPath, totalCost, movement.MovementRemaining);
    }
    
    private static MovementResult ExecuteExplorationMove(Entity entity, MovementComponent movement, List<Vector2I> path)
    {
        var from = entity.GridPosition;
        var to = path[path.Count - 1];
        
        // In exploration, no movement budget - just move to destination
        entity.GridPosition = to;
        
        movement.NotifyMoved(from, to, 0);
        GD.Print($"[Exploration Move] {entity.Name}: {from} -> {to}");
        
        return MovementResult.Succeeded(path, 0, movement.MovementRemaining);
    }
    
    /// <summary>
    /// Calculates the total movement cost for a path without executing the move.
    /// Useful for UI to show if a move is affordable.
    /// </summary>
    public static int CalculatePathCost(List<Vector2I> path)
    {
        if (path == null || path.Count <= 1)
            return 0;
        
        int totalCost = 0;
        for (int i = 1; i < path.Count; i++)
        {
            totalCost += _gridManager.GetCellMovementCost(path[i]);
        }
        
        return totalCost;
    }
    
    /// <summary>
    /// Gets all cells an entity can reach with their remaining movement.
    /// Useful for UI to highlight reachable cells.
    /// </summary>
    public static List<Vector2I> GetReachableCells(Entity entity)
    {
        var reachable = new List<Vector2I>();
        var movement = entity.GetComponent<MovementComponent>();
        
        if (movement == null || !_initialized)
            return reachable;
        
        int maxCells = movement.MovementRemaining / GridManager.FEET_PER_CELL;
        var start = entity.GridPosition;
        
        // Simple flood fill up to max range
        var visited = new HashSet<Vector2I>();
        var queue = new Queue<(Vector2I pos, int costSoFar)>();
        queue.Enqueue((start, 0));
        visited.Add(start);
        
        while (queue.Count > 0)
        {
            var (current, costSoFar) = queue.Dequeue();
            
            // Add to reachable if not the starting position
            if (current != start)
            {
                reachable.Add(current);
            }
            
            // Check neighbors
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;
                    
                    var neighbor = new Vector2I(current.X + dx, current.Y + dy);
                    
                    if (visited.Contains(neighbor))
                        continue;
                    
                    if (!_gridManager.IsValidGridPosition(neighbor))
                        continue;
                    
                    if (!_gridManager.IsWalkable(neighbor))
                        continue;
                    
                    if(_gridManager.IsMovementBlocked(current, neighbor))
                        continue;

                    int cellCost = _gridManager.GetCellMovementCost(neighbor);
                    int newCost = costSoFar + cellCost;
                    
                    if (newCost <= movement.MovementRemaining)
                    {
                        visited.Add(neighbor);
                        queue.Enqueue((neighbor, newCost));
                    }
                }
            }
        }
        
        return reachable;
    }
    
}