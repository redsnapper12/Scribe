using Godot;

namespace Scribe.Scripts.Core.Interfaces.Maps;

/// <summary>
/// Provides grid walkability and wall data to <see cref="Scribe.Scripts.Core.GridManager"/>.
/// Implement this on any map resource (Dungeon Scrawl, hand-authored, procedural, etc.)
/// so it can be fed into the combat grid via <see cref="Scribe.Scripts.IO.MapLoader"/>.
/// </summary>
public interface IMapData
{
    /// <summary>Grid width in cells.</summary>
    int Width { get; }

    /// <summary>Grid height in cells.</summary>
    int Height { get; }

    /// <summary>
    /// World-space pixel offset of grid cell (0,0) relative to the background image origin.
    /// Applied as the GridManager's scene Position so world coordinates align with the PNG.
    /// </summary>
    Vector2 GridOffset { get; }

    /// <summary>
    /// Absolute filesystem path to the background PNG displayed behind the grid.
    /// Empty string if no background image exists.
    /// </summary>
    string BackgroundImagePath { get; }

    /// <summary>Returns true if the given cell can be entered.</summary>
    bool IsWalkable(Vector2I cell);

    /// <summary>
    /// Returns the wall bitmask for a cell.
    /// Bit layout matches <see cref="Scribe.Scripts.Core.Direction"/>:
    ///   bit 0 = North, bit 1 = East, bit 2 = South, bit 3 = West.
    /// Returns 0 if the cell has no walls or is out of bounds.
    /// </summary>
    int GetWallBitmask(Vector2I cell);
}
