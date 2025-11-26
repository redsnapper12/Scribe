using Godot;
using System;
using System.Collections.Generic;

namespace Scribe.Scripts.Core;

public enum TerrainType
{
    Normal,
    Difficult,
    Water,
    Climbing
}

/// <summary>
/// Manages grid visualization and provides static utility methods for grid/world conversions.
/// Each cell represents 5 feet in D&D 5e.
/// </summary>
public partial class GridManager : Node2D
{
    public const int CELL_SIZE = 64;
    public const int FEET_PER_CELL = 5;

    [ExportCategory("Grid Configuration")]
    [Export] public int GridWidth { get; set; } = 30;
    [Export] public int GridHeight { get; set; } = 20;

    [ExportCategory("Visual Settings")]
    [Export] public Color GridColor { get; set; } = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    [Export] public float LineWidth { get; set; } = 1.0f;
    [Export] public bool DrawGrid { get; set; } = true;

    public TileMapLayer WalkableLayer { get; set; }

    public override void _Ready()
    {
        // Draw grid once on ready for static performance
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (!DrawGrid)
            return;

        int totalWidth = GridWidth * CELL_SIZE;
        int totalHeight = GridHeight * CELL_SIZE;

        // Draw vertical lines
        for (int x = 0; x <= GridWidth; x++)
        {
            int xPos = x * CELL_SIZE;
            DrawLine(
                new Vector2(xPos, 0),
                new Vector2(xPos, totalHeight),
                GridColor,
                LineWidth
            );
        }

        // Draw horizontal lines
        for (int y = 0; y <= GridHeight; y++)
        {
            int yPos = y * CELL_SIZE;
            DrawLine(
                new Vector2(0, yPos),
                new Vector2(totalWidth, yPos),
                GridColor,
                LineWidth
            );
        }
    }

    #region Static Grid Utilities

    /// <summary>
    /// Converts world pixel position to grid coordinates (0-indexed).
    /// </summary>
    public static Vector2I WorldToGrid(Vector2 worldPosition)
    {
        return new Vector2I(
            Mathf.FloorToInt(worldPosition.X / CELL_SIZE),
            Mathf.FloorToInt(worldPosition.Y / CELL_SIZE)
        );
    }

    /// <summary>
    /// Converts grid coordinates to world pixel position (center of cell).
    /// </summary>
    public static Vector2 GridToWorld(Vector2I gridPosition)
    {
        return new Vector2(
            gridPosition.X * CELL_SIZE + CELL_SIZE / 2,
            gridPosition.Y * CELL_SIZE + CELL_SIZE / 2
        );
    }

    /// <summary>
    /// Calculates distance in feet between two grid positions.
    /// Uses D&D 5e grid distance (diagonal = 5ft).
    /// </summary>
    public static int GetDistanceInFeet(Vector2I from, Vector2I to)
    {
        // D&D 5e uses simplified diagonal movement: each square (including diagonal) = 5ft
        int dx = Mathf.Abs(to.X - from.X);
        int dy = Mathf.Abs(to.Y - from.Y);
        int cells = Mathf.Max(dx, dy); // Chebyshev distance

        return cells * FEET_PER_CELL;
    }

    /// <summary>
    /// Calculates distance in grid cells between two positions.
    /// </summary>
    public static int GetDistanceInCells(Vector2I from, Vector2I to)
    {
        int dx = Mathf.Abs(to.X - from.X);
        int dy = Mathf.Abs(to.Y - from.Y);
        return Mathf.Max(dx, dy); // Chebyshev distance
    }

    #endregion

    #region Instance Grid Utilities

    /// <summary>
    /// Checks if grid position is within the grid bounds.
    /// </summary>
    public bool IsValidGridPosition(Vector2I gridPosition)
    {
        return gridPosition.X >= 0 && gridPosition.X < GridWidth &&
               gridPosition.Y >= 0 && gridPosition.Y < GridHeight;
    }

    /// <summary>
    /// Gets all grid cells within a radius (in feet) of a center position.
    /// </summary>
    public List<Vector2I> GetCellsInRadius(Vector2I center, int radiusFeet)
    {
        var cells = new List<Vector2I>();
        int radiusCells = radiusFeet / FEET_PER_CELL;

        for (int x = center.X - radiusCells; x <= center.X + radiusCells; x++)
        {
            for (int y = center.Y - radiusCells; y <= center.Y + radiusCells; y++)
            {
                var cell = new Vector2I(x, y);
                if (IsValidGridPosition(cell) && GetDistanceInCells(center, cell) <= radiusCells)
                {
                    cells.Add(cell);
                }
            }
        }

        return cells;
    }

    #endregion

    #region Tile Map Utilities

    /// <summary>
    /// Checks if a grid cell is walkable (not blocked by terrain).
    /// Returns true if walkable, false if blocked.
    /// Uses TileData custom property "walkable" (defaults to true if no tile exists).
    /// </summary>
    public bool IsWalkable(Vector2I gridPosition)
    {
        if (WalkableLayer == null)
        {
            GD.PushWarning("WalkableLayer not assigned to GridManager");
            return true; // Default to walkable if no layer assigned
        }

        if (!IsValidGridPosition(gridPosition))
            return false;

        var tileData = WalkableLayer.GetCellTileData(gridPosition);

        // If no tile exists, it's walkable
        if (tileData == null)
            return true;

        // Check custom data property "walkable" (default true)
        var walkableData = tileData.GetCustomData("walkable");
        if (walkableData.VariantType == Variant.Type.Nil)
            return true;

        return walkableData.AsBool();
    }

    /// <summary>
    /// Checks if a tile blocks line of sight.
    /// Uses TileData custom property "blocks_sight" (defaults to false).
    /// </summary>
    public bool BlocksSight(Vector2I gridPosition)
    {
        if (WalkableLayer == null)
            return false;

        if (!IsValidGridPosition(gridPosition))
            return false;

        var tileData = WalkableLayer.GetCellTileData(gridPosition);

        // If no tile exists, doesn't block sight
        if (tileData == null)
            return false;

        var blocksData = tileData.GetCustomData("blocks_sight");
        if (blocksData.VariantType == Variant.Type.Nil)
            return false;

        return blocksData.AsBool();
    }

    /// <summary>
    /// Gets the terrain type of a grid cell.
    /// Uses TileData custom property "terrain_type" (defaults to Normal).
    /// </summary>
    public TerrainType GetTerrainType(Vector2I gridPosition)
    {
        if (WalkableLayer == null)
            return TerrainType.Normal;

        if (!IsValidGridPosition(gridPosition))
            return TerrainType.Normal;

        var tileData = WalkableLayer.GetCellTileData(gridPosition);
        
        if (tileData == null)
            return TerrainType.Normal;
        
        var terrainData = tileData.GetCustomData("terrain_type");
        if (terrainData.VariantType == Variant.Type.Nil)
            return TerrainType.Normal;
        
        // Terrain type stored as int in tile data
        return (TerrainType)terrainData.AsInt32();
    }

    /// <summary>
    /// Calculates movement cost in feet for a single cell based on terrain.
    /// </summary>
    public int GetCellMovementCost(Vector2I gridPosition)
    {
        var terrain = GetTerrainType(gridPosition);
        
        return terrain switch
        {
            TerrainType.Difficult => FEET_PER_CELL * 2,  // 10 feet
            TerrainType.Water => FEET_PER_CELL * 2,      // 10 feet (without swim speed)
            TerrainType.Climbing => FEET_PER_CELL * 2,   // 10 feet (without climb speed)
            _ => FEET_PER_CELL                            // 5 feet
        };
    }

    /// <summary>
    /// Finds a path between two grid positions using A* pathfinding.
    /// Returns null if no path exists.
    /// </summary>
    /// <param name="start">Starting grid position</param>
    /// <param name="goal">Target grid position</param>
    /// <param name="additionalBlockingCheck">Optional function to check if a cell is blocked (e.g., by entities)</param>
    public List<Vector2I> FindPath(Vector2I start, Vector2I goal, Func<Vector2I, bool> additionalBlockingCheck = null)
    {
        if (!IsValidGridPosition(start) || !IsValidGridPosition(goal))
            return null;

        if (!IsWalkable(goal) || (additionalBlockingCheck != null && additionalBlockingCheck(goal)))
            return null;

        var openSet = new List<Vector2I> { start };
        var cameFrom = new Dictionary<Vector2I, Vector2I>();
        var gScore = new Dictionary<Vector2I, int> { [start] = 0 };
        var fScore = new Dictionary<Vector2I, float> { [start] = GetDistanceInCells(start, goal) };

        // Pre-calculate the direction vector for cross-product tiebreaker
        float dx = goal.X - start.X;
        float dy = goal.Y - start.Y;

        while (openSet.Count > 0)
        {
            // Find node with lowest fScore
            var current = openSet[0];
            float lowestF = fScore.GetValueOrDefault(current, float.MaxValue);

            foreach (var node in openSet)
            {
                float nodeF = fScore.GetValueOrDefault(node, float.MaxValue);
                if (nodeF < lowestF)
                {
                    current = node;
                    lowestF = nodeF;
                }
            }

            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);

            foreach (var neighbor in GetNeighbors(current))
            {
                // Check terrain walkability
                if (!IsWalkable(neighbor))
                    continue;

                // Check additional blocking (e.g., entities)
                if (additionalBlockingCheck != null && additionalBlockingCheck(neighbor))
                    continue;

                int tentativeGScore = gScore.GetValueOrDefault(current, int.MaxValue) + 1;

                if (tentativeGScore < gScore.GetValueOrDefault(neighbor, int.MaxValue))
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    
                    // Calculate heuristic with cross-product tiebreaker
                    float h = GetDistanceInCells(neighbor, goal);
                    
                    // Cross-product tiebreaker: penalize deviation from direct line
                    float dx2 = neighbor.X - goal.X;
                    float dy2 = neighbor.Y - goal.Y;
                    float cross = Mathf.Abs(dx * dy2 - dy * dx2);
                    h += cross * 0.001f;  // Small penalty for deviation
                    
                    fScore[neighbor] = tentativeGScore + h;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return null;
    }

    private List<Vector2I> ReconstructPath(Dictionary<Vector2I, Vector2I> cameFrom, Vector2I current)
    {
        var path = new List<Vector2I> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }

    private List<Vector2I> GetNeighbors(Vector2I cell)
    {
        var neighbors = new List<Vector2I>();

        // 8-directional movement (including diagonals per D&D 5e)
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                var neighbor = new Vector2I(cell.X + dx, cell.Y + dy);
                if (IsValidGridPosition(neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }
        }

        return neighbors;
    }

    #endregion
}