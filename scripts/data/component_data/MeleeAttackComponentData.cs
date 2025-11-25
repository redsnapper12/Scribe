using Godot;
using Scribe.Scripts.Core.Interfaces;

namespace Scribe.Scripts.Data.ComponentData;

[GlobalClass]
public partial class MeleeAttackComponentData : ComponentData
{
    [Export] public int AttackBonus { get; set; } = 0;
    
    [ExportGroup("Damage")]
    [Export] public int NumDice { get; set; } = 1;
    [Export] public DieType DieType { get; set; } = DieType.D6;
    [Export] public int DamageBonus { get; set; } = 0;
    [Export] public DamageType DamageType { get; set; } = DamageType.Bludgeoning;
    
    [Export] public int Range { get; set; } = 5;
    [Export] public string AttackName { get; set; } = "Melee Attack";
}