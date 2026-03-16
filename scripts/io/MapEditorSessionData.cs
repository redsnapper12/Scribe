using Godot;

namespace Scribe.Scripts.IO;

/// <summary>
/// Persists the map editor's working state across sessions.
/// Saved to user://maps/.editor_session.tres on scene close;
/// loaded back in _Ready() to restore the editor exactly as it was left.
/// </summary>
[GlobalClass]
public partial class MapEditorSessionData : Resource
{
    /// <summary>Absolute filesystem path to the last imported .ds file.</summary>
    [Export] public string DsFilePath { get; set; } = "";

    /// <summary>Absolute filesystem path to the companion background PNG.</summary>
    [Export] public string BackgroundImagePath { get; set; } = "";

    /// <summary>
    /// A snapshot of the map data at the time the session was saved.
    /// Captures painted walkability edits and the current GridOffset,
    /// so unsaved changes survive a close/reopen without requiring a .tres save.
    /// </summary>
    [Export] public DungeonScrawlMapData MapData { get; set; }

    /// <summary>World-space camera position.</summary>
    [Export] public Vector2 CameraPosition { get; set; } = Vector2.Zero;

    /// <summary>Camera zoom level (scalar; applied to both X and Y).</summary>
    [Export] public float CameraZoom { get; set; } = 1f;
}
