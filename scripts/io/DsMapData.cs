using System.Collections.Generic;
using Godot;

namespace Scribe.Scripts.IO;

/// <summary>
/// Raw geometry and placement data extracted from a Dungeon Scrawl .ds file.
/// All positions are in DS pixel space — use DungeonScrawlConverter to convert to grid cells.
/// </summary>
public class DsMapData
{
    /// <summary>
    /// Pixels per grid cell as authored in Dungeon Scrawl (e.g. 36).
    /// Used to convert DS pixel coordinates to grid cell indices.
    /// </summary>
    public float CellDiameter { get; set; } = 36f;

    /// <summary>
    /// Minimum pixel coordinate across all geometry — used to offset the
    /// dungeon to a (0,0) grid origin before cell conversion.
    /// </summary>
    public Vector2 MinBounds { get; set; } = Vector2.Zero;

    /// <summary>
    /// Closed polygons that define walkable dungeon rooms and corridors.
    /// Each list is one polygon; points are in DS pixel space.
    /// These come from GEOMETRY nodes whose children include a MULTIPOLYGON
    /// named "Floor" or "Walls".
    /// </summary>
    public List<List<Vector2>> DungeonPolygons { get; set; } = new();

    /// <summary>
    /// Polygon outlines of door DUNGEON_ASSET geometry, in DS pixel space.
    /// Each inner list is one door's ring of vertices (same format as DungeonPolygons).
    /// Used with point-in-polygon rasterization to find exactly which cells a door covers.
    /// </summary>
    public List<List<Vector2>> DoorPolygons { get; set; } = new();

}
