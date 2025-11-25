using Godot;

namespace Scribe.Scripts.Data.ComponentData;

[GlobalClass]
public partial class MovementComponentData : ComponentData
{
    [Export] public int WalkSpeed { get; set; } = 30;
    [Export] public int FlySpeed { get; set; } = 0;
    [Export] public int SwimSpeed { get; set; } = 0;
    [Export] public int ClimbSpeed { get; set; } = 0;
}