using Godot;

namespace Scribe.Scripts.IO;

/// <summary>
/// Draws a DungeonScrawlMapData grid overlay for the map editor.
/// Grid lines tile across the full image area (<see cref="ImageSize"/>).
/// Dungeon cells (colored fills + wall lines + door crosses) are drawn on top.
///
/// Set <see cref="MapData"/> and <see cref="ImageSize"/> then call QueueRedraw() to refresh.
/// <see cref="GridOffset"/> controls where the grid origin sits; drag to align with the PNG.
/// </summary>
public partial class MapEditorOverlay : Node2D
{
    // ── Colors ─────────────────────────────────────────────────────────────────

    private static readonly Color ColWalkable = new(0.2f, 0.8f, 0.2f, 0.25f);
    private static readonly Color ColWalled   = new(0.2f, 0.5f, 1.0f, 0.30f);
    private static readonly Color ColBlocked  = new(0.8f, 0.1f, 0.1f, 0.35f);
    private static readonly Color ColDoor     = new(1.0f, 0.9f, 0.0f, 0.70f);
    private static readonly Color ColWallEdge = new(1.0f, 1.0f, 1.0f, 0.85f);
    private static readonly Color ColGrid     = new(0.5f, 0.5f, 0.5f, 0.30f);

    // ── Wall bit constants — match GridManager.Direction ─────────────────────

    private const int BitNorth = 1 << 0;
    private const int BitEast  = 1 << 1;
    private const int BitSouth = 1 << 2;
    private const int BitWest  = 1 << 3;

    private const float CellSize = 64f;

    // ── Properties ─────────────────────────────────────────────────────────────

    private DungeonScrawlMapData _mapData;
    /// <summary>Map data to draw. Call QueueRedraw() after setting.</summary>
    public DungeonScrawlMapData MapData
    {
        get => _mapData;
        set { _mapData = value; QueueRedraw(); }
    }

    private Vector2 _imageSize = Vector2.Zero;
    /// <summary>
    /// Pixel dimensions of the background PNG. Grid lines tile across this area.
    /// Set to the image texture size after loading.
    /// </summary>
    public Vector2 ImageSize
    {
        get => _imageSize;
        set { _imageSize = value; QueueRedraw(); }
    }

    private Vector2 _gridOffset = Vector2.Zero;
    /// <summary>
    /// World-space origin of the grid. Drag the align tool to adjust.
    /// (0,0) places cell (0,0) at the PNG top-left corner.
    /// </summary>
    public Vector2 GridOffset
    {
        get => _gridOffset;
        set { _gridOffset = value; QueueRedraw(); }
    }

    // ── Draw ───────────────────────────────────────────────────────────────────

    public override void _Draw()
    {
        var baseOffset = _gridOffset;

        // ── Full-image grid lines ─────────────────────────────────────────────
        if (_imageSize != Vector2.Zero)
        {
            // Vertical lines
            int nStart = Mathf.FloorToInt(-baseOffset.X / CellSize);
            int nEnd   = Mathf.CeilToInt((_imageSize.X - baseOffset.X) / CellSize);
            for (int n = nStart; n <= nEnd; n++)
            {
                float x = baseOffset.X + n * CellSize;
                if (x < 0 || x > _imageSize.X) continue;
                DrawLine(new Vector2(x, 0), new Vector2(x, _imageSize.Y), ColGrid, 1f);
            }

            // Horizontal lines
            int mStart = Mathf.FloorToInt(-baseOffset.Y / CellSize);
            int mEnd   = Mathf.CeilToInt((_imageSize.Y - baseOffset.Y) / CellSize);
            for (int m = mStart; m <= mEnd; m++)
            {
                float y = baseOffset.Y + m * CellSize;
                if (y < 0 || y > _imageSize.Y) continue;
                DrawLine(new Vector2(0, y), new Vector2(_imageSize.X, y), ColGrid, 1f);
            }
        }

        if (_mapData == null) return;

        // ── Dungeon cell fills ────────────────────────────────────────────────
        int w = _mapData.Width;
        int h = _mapData.Height;

        // Cover all grid cells over the full image; cells outside map bounds are non-accessible
        int colStart, colEnd, rowStart, rowEnd;
        if (_imageSize != Vector2.Zero)
        {
            colStart = Mathf.FloorToInt(-baseOffset.X / CellSize);
            colEnd   = Mathf.CeilToInt((_imageSize.X - baseOffset.X) / CellSize) - 1;
            rowStart = Mathf.FloorToInt(-baseOffset.Y / CellSize);
            rowEnd   = Mathf.CeilToInt((_imageSize.Y - baseOffset.Y) / CellSize) - 1;
        }
        else
        {
            colStart = 0; colEnd = w - 1;
            rowStart = 0; rowEnd = h - 1;
        }

        for (int cy = rowStart; cy <= rowEnd; cy++)
        {
            for (int cx = colStart; cx <= colEnd; cx++)
            {
                var cell    = new Vector2I(cx, cy);
                var topLeft = baseOffset + new Vector2(cx * CellSize, cy * CellSize);
                var rect    = new Rect2(topLeft, new Vector2(CellSize, CellSize));

                bool inBounds = cx >= 0 && cy >= 0 && cx < w && cy < h;

                if (!inBounds || !_mapData.IsWalkable(cell))
                {
                    DrawRect(rect, ColBlocked);
                    continue;
                }

                int bitmask = _mapData.GetWallBitmask(cell);
                DrawRect(rect, bitmask != 0 ? ColWalled : ColWalkable);

                // Wall edge lines
                if (bitmask == 0) continue;
                var tr = topLeft + new Vector2(CellSize, 0);
                var bl = topLeft + new Vector2(0, CellSize);
                var br = topLeft + new Vector2(CellSize, CellSize);

                if ((bitmask & BitNorth) != 0) DrawLine(topLeft, tr, ColWallEdge, 2f);
                if ((bitmask & BitEast)  != 0) DrawLine(tr, br, ColWallEdge, 2f);
                if ((bitmask & BitSouth) != 0) DrawLine(bl, br, ColWallEdge, 2f);
                if ((bitmask & BitWest)  != 0) DrawLine(topLeft, bl, ColWallEdge, 2f);
            }
        }

        // ── Door crosses ─────────────────────────────────────────────────────
        const float CrossRadius = 10f;
        foreach (var centroid in _mapData.DoorCentroids)
        {
            var worldPos = baseOffset + centroid * CellSize;
            DrawLine(worldPos - new Vector2(CrossRadius, 0), worldPos + new Vector2(CrossRadius, 0), ColDoor, 3f);
            DrawLine(worldPos - new Vector2(0, CrossRadius), worldPos + new Vector2(0, CrossRadius), ColDoor, 3f);
            DrawCircle(worldPos, 4f, ColDoor);
        }
    }
}
