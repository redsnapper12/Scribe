using Godot;
using Scribe.Scripts.Core.Interfaces.Maps;

namespace Scribe.Scripts.IO;

/// <summary>
/// Godot Resource storing the converted, editor-ready data for a Dungeon Scrawl map.
/// Produced by <see cref="DungeonScrawlConverter"/> from a parsed <see cref="DsMapData"/>
/// and saved as a .tres file that the in-game editor can refine.
///
/// Grid layout: flat row-major arrays where index = cell.Y * GridSize.X + cell.X.
/// </summary>
[GlobalClass]
public partial class DungeonScrawlMapData : Resource, IMapData
{
    // -------------------------------------------------------------------------
    // Exported properties (serialised to .tres)
    // -------------------------------------------------------------------------

    [Export] public Vector2I GridSize { get; set; }

    /// <summary>
    /// Minimum DS pixel coordinate across all dungeon polygon vertices.
    /// Used to convert between DS pixel space and Godot/PNG pixel space:
    ///   GodotPixelOffset = MinBounds * (GodotCellSize / DsCellDiameter)
    /// </summary>
    [Export] public Vector2 MinBounds { get; set; }

    /// <summary>
    /// Pixels per grid cell in Dungeon Scrawl's coordinate space (e.g. 36).
    /// Together with MinBounds, lets callers reconstruct the full DS→Godot transform.
    /// </summary>
    [Export] public float DsCellDiameter { get; set; } = 36f;

    /// <summary>
    /// Flat walkability array. 1 = walkable, 0 = blocked.
    /// Index: cell.Y * GridSize.X + cell.X
    /// </summary>
    [Export] public int[] Walkable { get; set; } = System.Array.Empty<int>();

    /// <summary>
    /// Flat wall bitmask array. Bit layout matches GridManager.Direction:
    ///   bit 0 (1)  = North wall  (-Y face)
    ///   bit 1 (2)  = East wall   (+X face)
    ///   bit 2 (4)  = South wall  (+Y face)
    ///   bit 3 (8)  = West wall   (-X face)
    /// Index: cell.Y * GridSize.X + cell.X
    /// </summary>
    [Export] public int[] WallBitmasks { get; set; } = System.Array.Empty<int>();

    /// <summary>
    /// Fractional grid-cell coordinates of each door's centre, as computed from
    /// the door polygon centroid in DS pixel space.
    /// Values are NOT snapped to integer cells — use these to position free-floating
    /// Door nodes in world space: worldPos = gridOrigin + centroid * godotCellSize.
    /// </summary>
    [Export] public Godot.Collections.Array<Vector2> DoorCentroids { get; set; } = new();

    /// <summary>
    /// Resource path to the map background PNG
    /// (e.g. "res://assets/maps/dwarven_ruin.png").
    /// Set manually after export from Dungeon Scrawl.
    /// </summary>
    [Export] public string BackgroundImagePath { get; set; } = "";

    /// <summary>
    /// World-space pixel offset of the grid origin relative to the top-left of the
    /// background PNG. Tuned in the map editor via the Align tool and saved here so
    /// MapLoader can reproduce the same alignment at runtime.
    /// </summary>
    [Export] public Vector2 GridOffset { get; set; } = Vector2.Zero;

    // -------------------------------------------------------------------------
    // Convenience helpers (not serialised)
    // -------------------------------------------------------------------------

    public int Width  => GridSize.X;
    public int Height => GridSize.Y;

    public bool InBounds(Vector2I cell) =>
        cell.X >= 0 && cell.X < Width && cell.Y >= 0 && cell.Y < Height;

    public bool IsWalkable(Vector2I cell) =>
        InBounds(cell) && Walkable[cell.Y * Width + cell.X] == 1;

    public int GetWallBitmask(Vector2I cell) =>
        InBounds(cell) ? WallBitmasks[cell.Y * Width + cell.X] : 0;

    public void SetWalkable(Vector2I cell, bool value)
    {
        if (InBounds(cell))
            Walkable[cell.Y * Width + cell.X] = value ? 1 : 0;
    }

    public void SetWallBit(Vector2I cell, int bitIndex, bool value)
    {
        if (!InBounds(cell)) return;
        int i = cell.Y * Width + cell.X;
        if (value) WallBitmasks[i] |=  (1 << bitIndex);
        else       WallBitmasks[i] &= ~(1 << bitIndex);
    }
}
