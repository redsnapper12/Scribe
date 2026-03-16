using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using Godot;

namespace Scribe.Scripts.IO;

/// <summary>
/// Parses a Dungeon Scrawl .ds file (ZIP archive containing a JSON "map" entry)
/// and returns raw geometry data as a <see cref="DsMapData"/>.
///
/// .ds node hierarchy used:
///   DOCUMENT → PAGE (cellDiameter)
///   TEMPLATE → GEOMETRY (geometryId) → MULTIPOLYGON "Floor"/"Walls"  → dungeon polygons
///   IMAGES   → DUNGEON_ASSET (transform) → GEOMETRY → MULTIPOLYGON "Door style"/"Stairs style"
/// </summary>
public static class DungeonScrawlParser
{
    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Parses a .ds file at the given OS file path.
    /// Use <c>ProjectSettings.GlobalizePath("res://...")</c> to convert resource paths.
    /// Returns null if parsing fails.
    /// </summary>
    public static DsMapData? Parse(string osFilePath)
    {
        try
        {
            using var zip = ZipFile.OpenRead(osFilePath);
            var mapEntry = zip.GetEntry("map");
            if (mapEntry == null)
            {
                GD.PrintErr("DungeonScrawlParser: No 'map' entry found in .ds archive.");
                return null;
            }

            using var stream = mapEntry.Open();
            using var doc = JsonDocument.Parse(stream);
            return ParseDocument(doc.RootElement);
        }
        catch (Exception e)
        {
            GD.PrintErr($"DungeonScrawlParser: Failed to parse '{osFilePath}': {e.Message}");
            return null;
        }
    }

    // -------------------------------------------------------------------------
    // Document traversal
    // -------------------------------------------------------------------------

    private static DsMapData? ParseDocument(JsonElement root)
    {
        if (!root.TryGetProperty("state", out var state) ||
            !state.TryGetProperty("document", out var document) ||
            !document.TryGetProperty("nodes", out var nodesEl))
        {
            GD.PrintErr("DungeonScrawlParser: Missing state.document.nodes.");
            return null;
        }

        if (!root.TryGetProperty("data", out var data) ||
            !data.TryGetProperty("geometry", out var geometryDataEl))
        {
            GD.PrintErr("DungeonScrawlParser: Missing data.geometry.");
            return null;
        }

        // Flatten all nodes into a dictionary for O(1) child lookups.
        var nodeMap = BuildNodeMap(nodesEl);

        var result = new DsMapData
        {
            CellDiameter = ExtractCellDiameter(nodeMap)
        };

        // Track processed geometryIds to avoid adding the same polygons twice
        // (a GEOMETRY node can have both "Floor" and "Walls" children but they
        //  share the same underlying polygon data).
        var processedGeoIds = new HashSet<string>();

        foreach (var (_, node) in nodeMap)
        {
            if (!node.TryGetProperty("type", out var typeEl)) continue;
            var type = typeEl.GetString();

            switch (type)
            {
                case "GEOMETRY":
                    TryExtractDungeonPolygons(node, nodeMap, geometryDataEl,
                        processedGeoIds, result.DungeonPolygons);
                    break;

                case "DUNGEON_ASSET":
                    TryExtractAssetPosition(node, nodeMap, geometryDataEl,
                        result.DoorPolygons);
                    break;
            }
        }

        result.MinBounds = ComputeMinBounds(result.DungeonPolygons);
        return result;
    }

    // -------------------------------------------------------------------------
    // Dungeon polygon extraction
    // -------------------------------------------------------------------------

    private static void TryExtractDungeonPolygons(
        JsonElement node,
        Dictionary<string, JsonElement> nodeMap,
        JsonElement geometryDataEl,
        HashSet<string> processedGeoIds,
        List<List<Vector2>> target)
    {
        if (!node.TryGetProperty("geometryId", out var geoIdEl)) return;
        var geoId = geoIdEl.GetString();
        if (geoId == null || !processedGeoIds.Add(geoId)) return; // already done

        // Only process GEOMETRY nodes whose children include a "Floor" MULTIPOLYGON.
        // The "Walls" MULTIPOLYGON is decorative hatching that extends BEYOND the
        // walkable floor boundary — including its geometry would corrupt MinBounds
        // and mark wall-hatch cells as walkable.
        if (!HasDirectMultipolygonChild(node, nodeMap, "Floor")) return;

        if (!geometryDataEl.TryGetProperty(geoId, out var geoEl)) return;

        ExtractClosedPolygons(geoEl, target);
    }

    private static void ExtractClosedPolygons(JsonElement geoEl, List<List<Vector2>> target)
    {
        if (!geoEl.TryGetProperty("polygons", out var polygonsEl)) return;

        // DS format: polygons = [ polygon, ... ]
        //            polygon  = [ ring, ... ]     (outer ring + optional hole rings)
        //            ring     = [ [x,y], ... ]
        foreach (var polygonEl in polygonsEl.EnumerateArray())
        {
            foreach (var ringEl in polygonEl.EnumerateArray())
            {
                var points = new List<Vector2>();
                foreach (var pointEl in ringEl.EnumerateArray())
                {
                    var coords = pointEl.EnumerateArray().ToArray();
                    if (coords.Length >= 2)
                        points.Add(new Vector2(coords[0].GetSingle(), coords[1].GetSingle()));
                }
                // A valid closed polygon needs at least 3 unique points
                if (points.Count >= 3)
                    target.Add(points);
            }
        }
    }

    // -------------------------------------------------------------------------
    // DUNGEON_ASSET position extraction
    // -------------------------------------------------------------------------

    private static void TryExtractAssetPosition(
        JsonElement node,
        Dictionary<string, JsonElement> nodeMap,
        JsonElement geometryDataEl,
        List<List<Vector2>> doors)
    {
        var (assetType, geoId) = ClassifyDungeonAsset(node, nodeMap);
        if (assetType != "door" || geoId == null) return;

        if (!geometryDataEl.TryGetProperty(geoId, out var geoEl)) return;

        ExtractClosedPolygons(geoEl, doors);
    }

    /// <summary>
    /// Identifies a DUNGEON_ASSET as "door" or "stairs" by inspecting its child
    /// GEOMETRY node for a MULTIPOLYGON with a known name.
    /// Returns (type, geometryId) so the caller can look up the geometry data.
    /// </summary>
    private static (string? type, string? geoId) ClassifyDungeonAsset(
        JsonElement node,
        Dictionary<string, JsonElement> nodeMap)
    {
        if (!node.TryGetProperty("children", out var children)) return (null, null);

        foreach (var childIdEl in children.EnumerateArray())
        {
            var childId = childIdEl.GetString();
            if (childId == null || !nodeMap.TryGetValue(childId, out var child)) continue;
            if (!child.TryGetProperty("type", out var typeEl)) continue;
            if (typeEl.GetString() != "GEOMETRY") continue;

            var geoId = child.TryGetProperty("geometryId", out var geoIdEl)
                ? geoIdEl.GetString() : null;

            if (HasDirectMultipolygonChild(child, nodeMap, "Door style")) return ("door", geoId);
        }
        return (null, null);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static float ExtractCellDiameter(Dictionary<string, JsonElement> nodeMap)
    {
        foreach (var (_, node) in nodeMap)
        {
            if (!node.TryGetProperty("type", out var typeEl)) continue;
            if (typeEl.GetString() != "PAGE") continue;

            if (node.TryGetProperty("grid", out var grid) &&
                grid.TryGetProperty("cellDiameter", out var diamEl))
                return diamEl.GetSingle();
        }
        GD.PrintErr("DungeonScrawlParser: No PAGE node found; defaulting cellDiameter to 36.");
        return 36f;
    }

    /// <summary>
    /// Builds a UUID → JsonElement map from the flat nodes dictionary.
    /// Avoids repeated O(n) searches when following child references.
    /// </summary>
    private static Dictionary<string, JsonElement> BuildNodeMap(JsonElement nodesEl)
    {
        var map = new Dictionary<string, JsonElement>();
        foreach (var node in nodesEl.EnumerateObject())
            map[node.Name] = node.Value;
        return map;
    }

    /// <summary>
    /// Returns true if any direct child of <paramref name="node"/> is a
    /// MULTIPOLYGON with the given name.
    /// </summary>
    private static bool HasDirectMultipolygonChild(
        JsonElement node,
        Dictionary<string, JsonElement> nodeMap,
        string multipolygonName)
    {
        if (!node.TryGetProperty("children", out var children)) return false;

        foreach (var childIdEl in children.EnumerateArray())
        {
            var childId = childIdEl.GetString();
            if (childId == null || !nodeMap.TryGetValue(childId, out var child)) continue;
            if (!child.TryGetProperty("type", out var typeEl)) continue;
            if (typeEl.GetString() != "MULTIPOLYGON") continue;
            if (child.TryGetProperty("name", out var nameEl) &&
                nameEl.GetString() == multipolygonName)
                return true;
        }
        return false;
    }

    private static Vector2 ComputeMinBounds(List<List<Vector2>> polygons)
    {
        float minX = float.MaxValue, minY = float.MaxValue;
        foreach (var poly in polygons)
        {
            foreach (var pt in poly)
            {
                if (pt.X < minX) minX = pt.X;
                if (pt.Y < minY) minY = pt.Y;
            }
        }
        return minX == float.MaxValue ? Vector2.Zero : new Vector2(minX, minY);
    }
}
