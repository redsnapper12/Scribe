using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Scribe.Scripts.Items;
using Scribe.Scripts.Entities;
using Scribe.Scripts.Data.ComponentData;
using Scribe.Scripts.Core.Interfaces.Maps;
namespace Scribe.Scripts.Core;

public enum TerrainType
{
    Normal,
    Difficult,
    Water,
    Climbing
}

public enum Direction
{
    North = 0,  // -Y
    East = 1,   // +X
    South = 2,  // +Y
    West = 3    // -X
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
    [Export] public bool DrawWalkability { get; set; } = false;

    public TileMapLayer GroundLayer { get; set; }    // Base terrain (floors, grass, etc.)
    public TileMapLayer OverlayLayer { get; set; }   // Objects on ground (walls, furniture, etc.)

    // When set, all walkability/wall queries use this data instead of TileMapLayers
    private IMapData _mapDataOverride;

    // Ground item management
    private readonly Dictionary<Vector2I, List<ItemNode>> _groundItems = new();
    private Node3D _itemContainer;

    public override void _Ready()
    {
        // Create container for item nodes
        _itemContainer = new Node3D { Name = "ItemContainer" };
        AddChild(_itemContainer);

        // Draw grid once on ready for static performance
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (DrawWalkability)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    var cell = new Vector2I(x, y);
                    var color = IsWalkable(cell)
                        ? new Color(0f, 1f, 0f, 0.25f)
                        : new Color(1f, 0f, 0f, 0.25f);
                    DrawRect(new Rect2(x * CELL_SIZE, y * CELL_SIZE, CELL_SIZE, CELL_SIZE), color);
                }
            }
        }

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

    /// <summary>
    /// Loads map data from any <see cref="IMapData"/> source, overriding TileMapLayer queries.
    /// Pass null to clear the override and revert to TileMapLayer-based checks.
    /// </summary>
    // Tracks the active grid-to-world offset so the static GridToWorld helper stays accurate
    // when a map is loaded via IMapData (which shifts GridManager.Position to GridOffset).
    private static Vector2 _staticWorldOffset = Vector2.Zero;

    public void LoadFromMapData(IMapData mapData)
    {
        _mapDataOverride = mapData;
        if (mapData != null)
        {
            GridWidth  = mapData.Width;
            GridHeight = mapData.Height;
            Position   = mapData.GridOffset;
            _staticWorldOffset = mapData.GridOffset;
        }
        else
        {
            _staticWorldOffset = Vector2.Zero;
        }
        QueueRedraw();
    }

    #region Static Grid Utilities

    /// <summary>
    /// Converts world pixel position to grid coordinates (0-indexed).
    /// </summary>
    public static Vector2I WorldToGrid(Vector2 worldPosition)
    {
        var adjusted = worldPosition - _staticWorldOffset;
        return new Vector2I(
            Mathf.FloorToInt(adjusted.X / CELL_SIZE),
            Mathf.FloorToInt(adjusted.Y / CELL_SIZE)
        );
    }

    /// <summary>
    /// Converts grid coordinates to world pixel position (center of cell).
    /// </summary>
    public static Vector2 GridToWorld(Vector2I gridPosition)
    {
        return _staticWorldOffset + new Vector2(
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

    #region Attack Validation

    /// <summary>
    /// Checks if a melee attack can be made from one cell to an adjacent cell.
    /// Uses the same logic as IsMovementBlocked() - if movement is blocked by walls, attacks are also blocked.
    /// </summary>
    /// <param name="attackerPos">Attacker's grid position</param>
    /// <param name="targetPos">Target's grid position</param>
    /// <returns>True if attack is not blocked by walls</returns>
    public bool CanAttackAcross(Vector2I attackerPos, Vector2I targetPos)
    {
        // If movement is blocked by walls, attack is also blocked
        // Rationale: If you can't physically reach through a wall to move,
        // you can't reach through it to swing a sword
        return !IsMovementBlocked(attackerPos, targetPos);
    }

    #endregion

    #region Wall Blocking Utilities

    /// <summary>
    /// Gets the cardinal direction from a movement delta vector.
    /// Returns null if the delta is not a cardinal direction.
    /// </summary>
    private Direction? GetCardinalDirection(Vector2I delta)
    {
        if (delta == new Vector2I(0, -1)) return Direction.North;
        if (delta == new Vector2I(1, 0)) return Direction.East;
        if (delta == new Vector2I(0, 1)) return Direction.South;
        if (delta == new Vector2I(-1, 0)) return Direction.West;
        return null;
    }

    /// <summary>
    /// Checks if a cell has a wall that blocks movement in the specified direction.
    /// In Godot 4, the wall_bitmask needs to be set per rotation in the TileSet editor.
    /// </summary>
    /// <param name="cell">Grid position to check</param>
    /// <param name="direction">Direction to check for blocking</param>
    /// <returns>True if the cell has a wall blocking that direction</returns>
    private bool CellBlocksDirection(Vector2I cell, Direction direction)
    {
        if (!IsValidGridPosition(cell)) return false;

        if (_mapDataOverride != null)
        {
            int bitmask = _mapDataOverride.GetWallBitmask(cell);
            return (bitmask & (1 << (int)direction)) != 0;
        }

        if (OverlayLayer == null) return false;

        var tileData = OverlayLayer.GetCellTileData(cell);
        if (tileData == null) return false;

        var wallBitmaskData = tileData.GetCustomData("wall_bitmask");
        if (wallBitmaskData.VariantType == Variant.Type.Nil) return false;

        int wallBitmask = wallBitmaskData.AsInt32();
        if (wallBitmask == 0) return false;

        bool blocks = (wallBitmask & (1 << (int)direction)) != 0;
        return blocks;
    }

    /// <summary>
    /// Checks if movement from one cell to another is blocked by walls.
    /// Checks both source and destination cells for blocking walls.
    /// </summary>
    /// <param name="from">Source cell</param>
    /// <param name="to">Destination cell</param>
    /// <returns>True if movement is blocked</returns>
    public bool IsMovementBlocked(Vector2I from, Vector2I to)
    {
        // Check if either cell is unwalkable
        if (!IsWalkable(from) || !IsWalkable(to))
            return true;

        // Calculate movement direction
        Vector2I delta = to - from;

        Direction? moveDirection = GetCardinalDirection(delta);
        if (moveDirection.HasValue)
        {
            // Block if the shared edge is blocked by either cell.
            // e.g. moving East from 'from' to 'to' is blocked if:
            //    - the source cell has an East wall, OR
            //    - the destination cell has a West wall
            if (CellBlocksDirection(from, moveDirection.Value))
                return true;

            Direction oppositeDir = (Direction)(((int)moveDirection.Value + 2) % 4);
            if (CellBlocksDirection(to, oppositeDir))
                return true;

            return false;
        }

        // Handle diagonal movement (NE/SE/SW/NW)
        if (Mathf.Abs(delta.X) == 1 && Mathf.Abs(delta.Y) == 1)
        {
            return IsDiagonalBlocked(from, to, delta);
        }

        // Not a valid adjacent move
        return true;
    }

    /// <summary>
    /// Checks if diagonal movement is blocked.
    /// Changed: diagonal movement is now blocked if either adjacent cardinal move is blocked.
    /// Rationale: prevents corner-cutting across a single edge wall (matches single-sided wall blocking).
    /// Example: moving NE is blocked if moving N is blocked OR moving E is blocked.
    /// </summary>
    private bool IsDiagonalBlocked(Vector2I from, Vector2I to, Vector2I delta)
    {
        // Cardinal neighbors
        Vector2I c1 = from + new Vector2I(delta.X, 0);  // west/east
        Vector2I c2 = from + new Vector2I(0, delta.Y);  // north/south

        // If either cardinal move is blocked then diagonal blocked
        if (IsMovementBlocked(from, c1)) return true;
        if (IsMovementBlocked(from, c2)) return true;

        // 2. destination might block entering from either cardinal direction
        Direction enterDir1 = GetCardinalDirection(new Vector2I(-delta.X, 0)).Value;  
        Direction enterDir2 = GetCardinalDirection(new Vector2I(0, -delta.Y)).Value;

        if (CellBlocksDirection(to, enterDir1)) return true;
        if (CellBlocksDirection(to, enterDir2)) return true;

        return false;
    }

    #endregion

    #region Instance Grid Utilities

    /// <summary>
    /// Checks if grid position is within the grid bounds.
    /// </summary>
    public bool IsValidGridPosition(Vector2I gridPosition)
    {
        int w = _mapDataOverride?.Width  ?? GridWidth;
        int h = _mapDataOverride?.Height ?? GridHeight;
        return gridPosition.X >= 0 && gridPosition.X < w &&
               gridPosition.Y >= 0 && gridPosition.Y < h;
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

    public bool IsCellOccupied(Vector2I gridPosition, List<Entity> entities, Entity excludeEntity = null)
    {
        foreach (var entity in entities)
        {
            if (entity == excludeEntity)
                continue;

            if (!entity.TryGetComponent<HealthComponent>(out var health) || !health.IsAlive)
                continue;

            if (entity.GridPosition == gridPosition)
                return true;
        }
        return false;
    }

    #endregion

    #region Tile Map Utilities

    /// <summary>
    /// Checks if a grid cell is walkable (can be occupied).
    /// Checks both ground and overlay layers.
    /// Ground layer determines base walkability, overlay layer can restrict it.
    /// Does NOT check directional walls - use IsMovementBlocked() for that.
    /// </summary>
    public bool IsWalkable(Vector2I gridPosition)
    {
        if (!IsValidGridPosition(gridPosition))
            return false;

        if (_mapDataOverride != null)
            return _mapDataOverride.IsWalkable(gridPosition);

        // Check ground layer first
        if (GroundLayer != null)
        {
            var groundTileData = GroundLayer.GetCellTileData(gridPosition);
            if (groundTileData != null)
            {
                var groundWalkable = groundTileData.GetCustomData("walkable");
                // If ground is explicitly unwalkable, cell is unwalkable
                if (groundWalkable.VariantType != Variant.Type.Nil && !groundWalkable.AsBool())
                    return false;
            }
        }

        // Check overlay layer (can only restrict, not enable)
        if (OverlayLayer != null)
        {
            var overlayTileData = OverlayLayer.GetCellTileData(gridPosition);
            if (overlayTileData != null)
            {
                var overlayWalkable = overlayTileData.GetCustomData("walkable");
                // If overlay is explicitly unwalkable, cell is unwalkable
                if (overlayWalkable.VariantType != Variant.Type.Nil && !overlayWalkable.AsBool())
                    return false;
            }
        }

        // If we reach here, cell is walkable
        return true;
    }

    /// <summary>
    /// Checks if a tile blocks line of sight.
    /// Checks both ground and overlay layers.
    /// Uses TileData custom property "blocks_sight" (defaults to false).
    /// </summary>
    public bool BlocksSight(Vector2I gridPosition)
    {
        if (!IsValidGridPosition(gridPosition))
            return false;

        // Check ground layer
        if (GroundLayer != null)
        {
            var groundTileData = GroundLayer.GetCellTileData(gridPosition);
            if (groundTileData != null)
            {
                var blocksData = groundTileData.GetCustomData("blocks_sight");
                if (blocksData.VariantType != Variant.Type.Nil && blocksData.AsBool())
                    return true;
            }
        }

        // Check overlay layer
        if (OverlayLayer != null)
        {
            var overlayTileData = OverlayLayer.GetCellTileData(gridPosition);
            if (overlayTileData != null)
            {
                var blocksData = overlayTileData.GetCustomData("blocks_sight");
                if (blocksData.VariantType != Variant.Type.Nil && blocksData.AsBool())
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the terrain type of a grid cell from the ground layer.
    /// Uses TileData custom property "terrain_type" (defaults to Normal).
    /// </summary>
    public TerrainType GetTerrainType(Vector2I gridPosition)
    {
        if (GroundLayer == null || !IsValidGridPosition(gridPosition))
            return TerrainType.Normal;

        var tileData = GroundLayer.GetCellTileData(gridPosition);

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
                if (!IsValidGridPosition(neighbor))
                    continue;

                // Check if movement is blocked by walls
                if (IsMovementBlocked(cell, neighbor))
                    continue;

                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    #endregion

    #region Ground Item Management

    /// <summary>
    /// Places an item on the ground at the specified grid position.
    /// </summary>
    public ItemNode PlaceItem(Item item, Vector2I position)
    {
        if (item == null || !IsValidGridPosition(position))
        {
            GD.PrintErr($"PlaceItem: Invalid item or position {position}");
            return null;
        }
        // Create ItemNode
        var itemNodeScene = GD.Load<PackedScene>("res://scenes/items/item_node.tscn");
        var itemNode = itemNodeScene.Instantiate<ItemNode>();
        _itemContainer.AddChild(itemNode);
        itemNode.Initialize(item, position);

        // Track in ground items dictionary
        if (!_groundItems.ContainsKey(position))
        {
            _groundItems[position] = new List<ItemNode>();
        }
        _groundItems[position].Add(itemNode);

        GD.Print($"Placed {item.DisplayName} at {position}");
        return itemNode;
    }

    /// <summary>
    /// Removes an item node from the ground.
    /// </summary>
    public void RemoveItem(ItemNode itemNode)
    {
        if (itemNode == null)
            return;

        var position = itemNode.GridPosition;
        if (_groundItems.TryGetValue(position, out var items))
        {
            items.Remove(itemNode);
            if (items.Count == 0)
            {
                _groundItems.Remove(position);
            }
        }

        itemNode.QueueFree();
        GD.Print($"Removed {itemNode.Item.DisplayName} from {position}");
    }

    /// <summary>
    /// Gets all items at a specific grid position.
    /// </summary>
    public List<ItemNode> GetItemsAtPosition(Vector2I position)
    {
        if (_groundItems.TryGetValue(position, out var items))
        {
            return [.. items];
        }
        return new List<ItemNode>();
    }

    /// <summary>
    /// Gets the nearest item to a grid position within a maximum distance.
    /// </summary>
    public ItemNode GetNearestItem(Vector2I position, int maxDistance = 1)
    {
        ItemNode nearest = null;
        int minDistance = int.MaxValue;

        foreach (var kvp in _groundItems)
        {
            int distance = GetDistanceInCells(position, kvp.Key);
            if (distance <= maxDistance && distance < minDistance)
            {
                minDistance = distance;
                if (kvp.Value.Count > 0)
                {
                    nearest = kvp.Value[0];
                }
            }
        }

        return nearest;
    }

    /// <summary>
    /// Checks if there are any items at the specified position.
    /// </summary>
    public bool HasItemsAt(Vector2I position)
    {
        return _groundItems.TryGetValue(position, out var items) && items.Count > 0;
    }

    /// <summary>
    /// Gets all ground items in the grid.
    /// </summary>
    public IEnumerable<ItemNode> GetAllGroundItems()
    {
        return _groundItems.Values.SelectMany(list => list);
    }

    #endregion
}