using Godot;
using Scribe.Scripts.Core;
using Scribe.Scripts.Core.Interfaces.Maps;

namespace Scribe.Scripts.IO;

/// <summary>
/// Feeds an <see cref="IMapData"/> into a <see cref="GridManager"/> and places the
/// background PNG as a Sprite2D in the scene.
///
/// Usage (e.g. in CombatManager or a battle scene _Ready):
/// <code>
///   var mapData = ResourceLoader.Load&lt;DungeonScrawlMapData&gt;("user://maps/my_map.tres");
///   MapLoader.Load(mapData, GridManager, backgroundParent: this);
/// </code>
/// </summary>
public static class MapLoader
{
    /// <summary>
    /// Loads map data into the grid and creates/updates the background Sprite2D.
    /// </summary>
    /// <param name="mapData">Any <see cref="IMapData"/> source.</param>
    /// <param name="gridManager">The scene's GridManager node.</param>
    /// <param name="backgroundParent">
    /// Node that will own the background Sprite2D (typically the root of the combat scene).
    /// Pass null to skip background loading.
    /// </param>
    public static void Load(IMapData mapData, GridManager gridManager, Node2D backgroundParent = null)
    {
        if (mapData == null)
        {
            GD.PrintErr("[MapLoader] mapData is null.");
            return;
        }

        if (gridManager == null)
        {
            GD.PrintErr("[MapLoader] gridManager is null.");
            return;
        }

        // Feed walkability + wall data; sets GridManager position to GridOffset
        gridManager.LoadFromMapData(mapData);

        GD.Print($"[MapLoader] Loaded {mapData.Width}×{mapData.Height} map. GridOffset={mapData.GridOffset}");

        if (backgroundParent == null || string.IsNullOrEmpty(mapData.BackgroundImagePath))
            return;

        if (!System.IO.File.Exists(mapData.BackgroundImagePath))
        {
            GD.PrintErr($"[MapLoader] Background PNG not found: '{mapData.BackgroundImagePath}'");
            return;
        }

        // Reuse an existing MapBackground node if present, otherwise create one
        var background = backgroundParent.GetNodeOrNull<Sprite2D>("MapBackground")
                      ?? CreateBackground(backgroundParent);

        var img = Image.LoadFromFile(mapData.BackgroundImagePath);
        background.Texture = ImageTexture.CreateFromImage(img);

        // Background sits at world origin; GridManager.Position = GridOffset aligns the grid
        background.Position = Vector2.Zero;
    }

    /// <summary>Clears the map data override and removes the background sprite.</summary>
    public static void Unload(GridManager gridManager, Node2D backgroundParent = null)
    {
        gridManager?.LoadFromMapData(null);

        var background = backgroundParent?.GetNodeOrNull<Sprite2D>("MapBackground");
        background?.QueueFree();
    }

    private static Sprite2D CreateBackground(Node2D parent)
    {
        var sprite = new Sprite2D { Name = "MapBackground", Centered = false, ZIndex = -1 };
        parent.AddChild(sprite);
        return sprite;
    }
}
