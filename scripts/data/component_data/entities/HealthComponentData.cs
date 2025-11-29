using Godot;
using Godot.Collections;
using Scribe.Scripts.Core.Interfaces;

namespace Scribe.Scripts.Data.ComponentData;

[GlobalClass]
public partial class HealthComponentData : ComponentData
{
    [Export] public int MaxHP { get; set; } = 10;
    [Export] public int ArmorClass { get; set; } = 10;
    [Export] public Array<DamageType> Resistances { get; set; } = new();
    [Export] public Array<DamageType> Vulnerabilities { get; set; } = new();
    [Export] public Array<DamageType> Immunities { get; set; } = new();
}