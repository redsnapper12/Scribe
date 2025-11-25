using Godot;
using Godot.Collections;
using Scribe.Scripts.Core.Interfaces;

namespace Scribe.Scripts.Data.ComponentData;

[GlobalClass]
public partial class AIComponentData : ComponentData
{
    [Export] public AIBehaviorType BehaviorType { get; set; } = AIBehaviorType.SimpleAggressive;
    [Export] public string CustomBehaviorScript { get; set; } = "";
    [Export] public Dictionary Parameters { get; set; } = new();
}