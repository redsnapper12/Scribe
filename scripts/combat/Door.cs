using Godot;

namespace Scribe.Scripts.Combat;

/// <summary>
/// Marker node for a door. Positioned freely in world space (not grid-snapped).
/// Instantiated by MapLoader at: gridOrigin + DoorCentroid * godotCellSize.
/// </summary>
public partial class Door : Node2D
{
    [Export] public bool IsOpen { get; set; } = false;
}
