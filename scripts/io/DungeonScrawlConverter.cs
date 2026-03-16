using System;
using System.Collections.Generic;
using Godot;

namespace Scribe.Scripts.IO;

/// <summary>
/// Converts raw <see cref="DsMapData"/> geometry (pixel-space polygons from a
/// Dungeon Scrawl .ds file) into a <see cref="DungeonScrawlMapData"/> resource
/// suitable for loading into the Scribe combat grid.
///
/// Algorithm overview
/// ------------------
/// 1. Compute grid bounds from the polygon bounding box.
/// 2. Rasterise: for each grid cell, test whether its centre falls inside any
///    dungeon polygon (ray-casting point-in-polygon). Walkable = inside.
/// 3. Wall bitmask: for each walkable cell, for each of its 4 cardinal faces,
///    determine whether a wall exists on that face:
///      • If the neighbour cell is non-walkable → wall (trivial case).
///      • If the neighbour IS walkable (shared interior) → check whether any
///        polygon edge physically crosses the shared cell-boundary segment.
///        This handles diagonal walls and rooms whose polygon boundaries
///        happen to run between two walkable cells.
/// 4. Convert door/stair DS pixel positions to grid-cell indices.
/// </summary>
public static class DungeonScrawlConverter
{
    // Wall bit constants — must match GridManager.Direction enum.
    private const int BitNorth = 1 << 0; // 1
    private const int BitEast  = 1 << 1; // 2
    private const int BitSouth = 1 << 2; // 4
    private const int BitWest  = 1 << 3; // 8

    // Extra cells of padding added around the computed bounding box so border
    // cells are never clipped. All padding lands on the trailing (right/bottom)
    // edges because MinBounds is already the minimum polygon vertex.
    public const int GridPadding = 1;

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Converts <paramref name="raw"/> into a <see cref="DungeonScrawlMapData"/>.
    /// Returns null and logs an error if <paramref name="raw"/> contains no polygon data.
    /// </summary>
    public static DungeonScrawlMapData? Convert(DsMapData raw)
    {
        if (raw.DungeonPolygons.Count == 0)
        {
            GD.PrintErr("DungeonScrawlConverter: No dungeon polygons found in DsMapData.");
            return null;
        }

        var (gridWidth, gridHeight) = ComputeGridSize(raw);

        var result = new DungeonScrawlMapData
        {
            GridSize       = new Vector2I(gridWidth, gridHeight),
            MinBounds      = raw.MinBounds,
            DsCellDiameter = raw.CellDiameter,
            Walkable       = new int[gridWidth * gridHeight],
            WallBitmasks   = new int[gridWidth * gridHeight],
        };

        // Phase 1 — walkable cells
        RasteriseWalkableCells(raw, result);

        // Phase 2 — wall bitmasks
        ComputeAllWallBitmasks(raw, result);

        // Phase 3 — door / stairs grid positions
        ConvertObjectPositions(raw, result);

        GD.Print($"DungeonScrawlConverter: Converted {gridWidth}×{gridHeight} grid. " +
                 $"Doors: {result.DoorCentroids.Count}.");
        return result;
    }

    // -------------------------------------------------------------------------
    // Phase 1 — Rasterise walkable cells
    // -------------------------------------------------------------------------

    private static void RasteriseWalkableCells(DsMapData raw, DungeonScrawlMapData result)
    {
        int w = result.Width, h = result.Height;
        for (int cy = 0; cy < h; cy++)
        {
            for (int cx = 0; cx < w; cx++)
            {
                var centre = CellCentreInDsSpace(cx, cy, raw);
                bool walkable = IsPointInsideAnyPolygon(centre, raw.DungeonPolygons);
                result.Walkable[cy * w + cx] = walkable ? 1 : 0;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Phase 2 — Wall bitmasks
    // -------------------------------------------------------------------------

    private static void ComputeAllWallBitmasks(DsMapData raw, DungeonScrawlMapData result)
    {
        int w = result.Width, h = result.Height;
        for (int cy = 0; cy < h; cy++)
        {
            for (int cx = 0; cx < w; cx++)
            {
                if (result.Walkable[cy * w + cx] == 0) continue;
                result.WallBitmasks[cy * w + cx] = ComputeWallBitmask(cx, cy, raw, result);
            }
        }
    }

    /// <summary>
    /// Returns the 4-bit wall bitmask for cell (cx, cy).
    ///
    /// For each cardinal direction the rule is:
    ///   • Non-walkable neighbour → wall (the cell is on the dungeon boundary).
    ///   • Walkable neighbour but a polygon edge crosses their shared boundary
    ///     segment → wall (diagonal wall or separate-room boundary).
    /// </summary>
    private static int ComputeWallBitmask(
        int cx, int cy, DsMapData raw, DungeonScrawlMapData result)
    {
        int bitmask = 0;

        bitmask |= EvaluateFace(cx, cy, cx,     cy - 1, raw, result, BitNorth,
            FaceNorth(cx, cy, raw));

        bitmask |= EvaluateFace(cx, cy, cx + 1, cy,     raw, result, BitEast,
            FaceEast(cx, cy, raw));

        bitmask |= EvaluateFace(cx, cy, cx,     cy + 1, raw, result, BitSouth,
            FaceSouth(cx, cy, raw));

        bitmask |= EvaluateFace(cx, cy, cx - 1, cy,     raw, result, BitWest,
            FaceWest(cx, cy, raw));

        return bitmask;
    }

    /// <param name="nx">Neighbour cell X</param>
    /// <param name="ny">Neighbour cell Y</param>
    /// <param name="bit">Wall bit to set if a wall is found</param>
    /// <param name="faceP1">Start of the shared boundary segment in DS pixel space</param>
    /// <param name="faceP2">End of the shared boundary segment in DS pixel space</param>
    private static int EvaluateFace(
        int cx, int cy, int nx, int ny,
        DsMapData raw, DungeonScrawlMapData result,
        int bit, (Vector2 P1, Vector2 P2) face)
    {
        // Out-of-bounds neighbour → treat as non-walkable (dungeon boundary).
        bool neighbourWalkable = result.InBounds(new Vector2I(nx, ny)) &&
                                 result.IsWalkable(new Vector2I(nx, ny));

        if (!neighbourWalkable)
            return bit;

        // Both cells are walkable — check whether any polygon edge crosses the
        // shared boundary.  This can happen when:
        //   a) Two separate room polygons share a wall that straddles the boundary.
        //   b) A diagonal polygon edge runs through the transition between cells.
        if (AnyPolygonEdgeCrossesSegment(face.P1, face.P2, raw.DungeonPolygons))
            return bit;

        return 0;
    }

    // -------------------------------------------------------------------------
    // Shared boundary segments — in DS pixel space
    // -------------------------------------------------------------------------
    // Each "face" is the 1-cell-wide edge between this cell and its neighbour.
    // P1 and P2 run along that edge so the segment fully covers it.

    private static (Vector2 P1, Vector2 P2) FaceNorth(int cx, int cy, DsMapData raw)
    {
        float d  = raw.CellDiameter;
        float ox = raw.MinBounds.X, oy = raw.MinBounds.Y;
        float y  = cy * d + oy;
        return (new Vector2(cx * d + ox, y), new Vector2((cx + 1) * d + ox, y));
    }

    private static (Vector2 P1, Vector2 P2) FaceSouth(int cx, int cy, DsMapData raw)
    {
        float d  = raw.CellDiameter;
        float ox = raw.MinBounds.X, oy = raw.MinBounds.Y;
        float y  = (cy + 1) * d + oy;
        return (new Vector2(cx * d + ox, y), new Vector2((cx + 1) * d + ox, y));
    }

    private static (Vector2 P1, Vector2 P2) FaceEast(int cx, int cy, DsMapData raw)
    {
        float d  = raw.CellDiameter;
        float ox = raw.MinBounds.X, oy = raw.MinBounds.Y;
        float x  = (cx + 1) * d + ox;
        return (new Vector2(x, cy * d + oy), new Vector2(x, (cy + 1) * d + oy));
    }

    private static (Vector2 P1, Vector2 P2) FaceWest(int cx, int cy, DsMapData raw)
    {
        float d  = raw.CellDiameter;
        float ox = raw.MinBounds.X, oy = raw.MinBounds.Y;
        float x  = cx * d + ox;
        return (new Vector2(x, cy * d + oy), new Vector2(x, (cy + 1) * d + oy));
    }

    // -------------------------------------------------------------------------
    // Phase 3 — Object positions
    // -------------------------------------------------------------------------

    private static void ConvertObjectPositions(DsMapData raw, DungeonScrawlMapData result)
    {
        // Doors: record the centroid of each door polygon in fractional grid-cell
        // coordinates. The MapLoader instantiates a free-floating Door node at each
        // centroid (worldPos = gridOrigin + centroid * godotCellSize).
        foreach (var doorPoly in raw.DoorPolygons)
        {
            var centre = PolygonCentroid(doorPoly);
            float cx = (centre.X - raw.MinBounds.X) / raw.CellDiameter;
            float cy = (centre.Y - raw.MinBounds.Y) / raw.CellDiameter;
            result.DoorCentroids.Add(new Vector2(cx, cy));
        }
    }

    private static (Vector2 Min, Vector2 Max) PolygonBounds(List<Vector2> poly)
    {
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;
        foreach (var p in poly)
        {
            if (p.X < minX) minX = p.X;
            if (p.Y < minY) minY = p.Y;
            if (p.X > maxX) maxX = p.X;
            if (p.Y > maxY) maxY = p.Y;
        }
        return (new Vector2(minX, minY), new Vector2(maxX, maxY));
    }

    /// <summary>
    /// True polygon centroid via the signed-area (shoelace) formula.
    /// More accurate than the bounding-box midpoint for non-rectangular polygons
    /// (e.g. rotated or irregular door shapes in Dungeon Scrawl).
    /// Falls back to vertex average for degenerate (zero-area) polygons.
    /// </summary>
    private static Vector2 PolygonCentroid(List<Vector2> poly)
    {
        float area = 0f, cx = 0f, cy = 0f;
        int n = poly.Count;
        for (int i = 0; i < n; i++)
        {
            var p0 = poly[i];
            var p1 = poly[(i + 1) % n];
            float cross = p0.X * p1.Y - p1.X * p0.Y;
            area += cross;
            cx   += (p0.X + p1.X) * cross;
            cy   += (p0.Y + p1.Y) * cross;
        }
        area *= 0.5f;

        if (Math.Abs(area) < 1e-6f)
        {
            // Degenerate polygon — fall back to vertex average.
            var sum = Vector2.Zero;
            foreach (var v in poly) sum += v;
            return sum / n;
        }

        float inv = 1f / (6f * area);
        return new Vector2(cx * inv, cy * inv);
    }

    // -------------------------------------------------------------------------
    // Geometry utilities
    // -------------------------------------------------------------------------

    /// <summary>
    /// Ray-casting point-in-polygon test.
    /// Returns true if <paramref name="point"/> is strictly inside the polygon.
    /// Points on edges are treated as inside.
    /// </summary>
    private static bool IsPointInsidePolygon(Vector2 point, List<Vector2> polygon)
    {
        int n = polygon.Count;
        bool inside = false;

        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            float xi = polygon[i].X, yi = polygon[i].Y;
            float xj = polygon[j].X, yj = polygon[j].Y;

            // Ignore degenerate (duplicate-point) edges
            if (Math.Abs(xi - xj) < 1e-6f && Math.Abs(yi - yj) < 1e-6f) continue;

            bool crossesYPlane = (yi > point.Y) != (yj > point.Y);
            if (!crossesYPlane) continue;

            float xIntersect = (xj - xi) * (point.Y - yi) / (yj - yi) + xi;
            if (point.X < xIntersect)
                inside = !inside;
        }

        return inside;
    }

    private static bool IsPointInsideAnyPolygon(Vector2 point, List<List<Vector2>> polygons)
    {
        foreach (var polygon in polygons)
        {
            if (IsPointInsidePolygon(point, polygon))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true if any edge of any polygon crosses the segment (p1, p2).
    /// </summary>
    private static bool AnyPolygonEdgeCrossesSegment(
        Vector2 p1, Vector2 p2, List<List<Vector2>> polygons)
    {
        foreach (var polygon in polygons)
        {
            int n = polygon.Count;
            for (int i = 0; i < n; i++)
            {
                var v1 = polygon[i];
                var v2 = polygon[(i + 1) % n];

                // Skip degenerate (closing/duplicate) edges
                if (Math.Abs(v1.X - v2.X) < 1e-6f && Math.Abs(v1.Y - v2.Y) < 1e-6f)
                    continue;

                if (SegmentsIntersect(p1, p2, v1, v2))
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns true if polygon edge (p3→p4) crosses through the INTERIOR of face segment (p1→p2).
    /// Endpoint-only touches (t=0 or t=1) are excluded: a polygon vertex sitting exactly at
    /// a face corner is a normal room corner, not a wall between two walkable cells.
    /// </summary>
    private static bool SegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        float d1x = p2.X - p1.X, d1y = p2.Y - p1.Y;
        float d2x = p4.X - p3.X, d2y = p4.Y - p3.Y;
        float cross = d1x * d2y - d1y * d2x;

        // Parallel or collinear — treat as non-intersecting for wall purposes
        if (Math.Abs(cross) < 1e-10f) return false;

        float dx = p3.X - p1.X, dy = p3.Y - p1.Y;
        float t = (dx * d2y - dy * d2x) / cross;
        float u = (dx * d1y - dy * d1x) / cross;

        // t must be strictly interior to the face segment to count as a genuine crossing.
        // Polygon edges that only touch a face endpoint (t ≈ 0 or t ≈ 1) are corner contacts,
        // not walls — including them caused every boundary floor cell to gain false wall bits.
        const float eps = 1e-5f;
        return t > eps && t < 1f - eps && u >= 0f && u <= 1f;
    }

    // -------------------------------------------------------------------------
    // Coordinate utilities
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the centre of grid cell (cx, cy) in DS pixel space.
    /// Uses 0.499 instead of 0.5 to bias centres fractionally inward, preventing
    /// boundary cells whose centres land marginally outside the polygon from being
    /// incorrectly marked non-walkable due to floating-point precision.
    /// </summary>
    private static Vector2 CellCentreInDsSpace(int cx, int cy, DsMapData raw)
    {
        float d = raw.CellDiameter;
        return new Vector2(
            (cx + 0.499f) * d + raw.MinBounds.X,
            (cy + 0.499f) * d + raw.MinBounds.Y);
    }

    /// <summary>
    /// Computes the minimum grid dimensions that contain all dungeon polygons,
    /// plus <see cref="GridPadding"/> cells of margin on every side.
    /// </summary>
    private static (int width, int height) ComputeGridSize(DsMapData raw)
    {
        float maxX = float.MinValue, maxY = float.MinValue;

        foreach (var poly in raw.DungeonPolygons)
        {
            foreach (var pt in poly)
            {
                if (pt.X > maxX) maxX = pt.X;
                if (pt.Y > maxY) maxY = pt.Y;
            }
        }

        int width  = (int)Math.Ceiling((maxX - raw.MinBounds.X) / raw.CellDiameter) + GridPadding * 2;
        int height = (int)Math.Ceiling((maxY - raw.MinBounds.Y) / raw.CellDiameter) + GridPadding * 2;

        return (Math.Max(width, 1), Math.Max(height, 1));
    }
}
