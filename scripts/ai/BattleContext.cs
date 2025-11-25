using Godot;
using System.Collections.Generic;
using System.Linq;
using Scribe.Scripts.Core.Entities;
using Scribe.Scripts.Core.Interfaces;
using Scribe.Scripts.Core;
using System;

namespace Scribe.Scripts.AI;

public partial class BattleContext : RefCounted
{
    public List<Entity> AllEntities { get; set; } = new();
    public TileMapLayer BattleGrid { get; set; }
    public GridManager GridManager { get; set; }
    
    public Entity FindNearestEnemy(Entity self)
    {
        Entity nearest = null;
        float minDistance = float.MaxValue;
        
        foreach (var entity in AllEntities)
        {
            if (entity == self || !entity.IsAlive)
                continue;
            
            float distance = GetDistance(self, entity);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = entity;
            }
        }
        
        return nearest;
    }
    
    public Entity FindWeakestEnemy(Entity self)
    {
        return AllEntities
            .Where(e => e != self && e.IsAlive && e is IDamageable)
            .OrderBy(e => e.CurrentHP)
            .FirstOrDefault();
    }
    
    public List<Entity> GetEnemiesInRange(Entity self, float range)
    {
        return AllEntities
            .Where(e => e != self && e.IsAlive && GetDistance(self, e) <= range)
            .ToList();
    }
    
    public float GetDistance(Entity a, Entity b)
    {
        return GridManager.GetDistanceInFeet(a.GridPosition, b.GridPosition);
    }
    
    public bool IsInMeleeRange(Entity attacker, Entity target)
    {
        int distance = GridManager.GetDistanceInFeet(attacker.GridPosition, target.GridPosition);
        return distance <= attacker.MeleeRange;
    }
    
    /// <summary>
    /// Finds a path considering both terrain and entity positions.
    /// Returns empty list if no path exists.
    /// </summary>
    public List<Vector2I> GetPath(Entity from, Vector2I to)
    {
        if (GridManager == null)
        {
            GD.PushWarning("GridManager not set in BattleContext");
            return new List<Vector2I>();
        }
        
        bool IsBlockedByEntity(Vector2I gridPos)
        {
            foreach (var entity in AllEntities)
            {
                if (entity == from || !entity.IsAlive)
                    continue;
                
                if (entity.GridPosition == gridPos)
                    return true;
            }
            return false;
        }
        
        var path = GridManager.FindPath(from.GridPosition, to, IsBlockedByEntity);
        return path ?? new List<Vector2I>();
    }
    
    public void MoveToward(Entity entity, Vector2I targetPosition, int maxCells)
    {
        var path = GetPath(entity, targetPosition);
        
        if (path.Count <= 1)
            return;
        
        // Move up to maxCells along the path (skip index 0 which is current position)
        int cellsToMove = Math.Min(maxCells, path.Count - 1);
        entity.GridPosition = path[cellsToMove];
    }
    
    public Vector2I FindRetreatPosition(Entity self, Entity threat, int desiredDistanceFeet)
    {
        // Simple retreat: move away from threat
        Vector2I selfPos = self.GridPosition;
        Vector2I threatPos = threat.GridPosition;
        
        Vector2I direction = new Vector2I(
            Math.Sign(selfPos.X - threatPos.X),
            Math.Sign(selfPos.Y - threatPos.Y)
        );
        
        int cells = desiredDistanceFeet / GridManager.FEET_PER_CELL;
        Vector2I retreatTarget = selfPos + direction * cells;
        
        if (GridManager.IsValidGridPosition(retreatTarget) && GridManager.IsWalkable(retreatTarget))
            return retreatTarget;
        
        return selfPos;
    }
    
    public bool HasLineOfSight(Entity observer, Entity target)
    {
        return !IsLineBlocked(observer.GridPosition, target.GridPosition);
    }
    
    private bool IsPositionBlocked(Vector2I gridPos, Entity movingEntity)
    {
        // Check terrain blocking using GridManager
        if (GridManager != null && !GridManager.IsWalkable(gridPos))
            return true;
        
        // Check entity blocking
        foreach (var entity in AllEntities)
        {
            if (entity == movingEntity || !entity.IsAlive)
                continue;
            
            if (entity.GridPosition == gridPos)
                return true;
        }
        
        return false;
    }
    
    private bool IsLineBlocked(Vector2I from, Vector2I to)
    {
        if (GridManager == null)
            return false;
        
        // Bresenham's line algorithm
        int dx = Mathf.Abs(to.X - from.X);
        int dy = Mathf.Abs(to.Y - from.Y);
        int x = from.X;
        int y = from.Y;
        int n = 1 + dx + dy;
        int xInc = (to.X > from.X) ? 1 : -1;
        int yInc = (to.Y > from.Y) ? 1 : -1;
        int error = dx - dy;
        dx *= 2;
        dy *= 2;
        
        for (; n > 0; --n)
        {
            if (GridManager.BlocksSight(new Vector2I(x, y)))
            {
                return true;
            }
            
            if (error > 0)
            {
                x += xInc;
                error -= dy;
            }
            else
            {
                y += yInc;
                error += dx;
            }
        }
        
        return false;
    }
}