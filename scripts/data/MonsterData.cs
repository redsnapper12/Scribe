using Godot;
using Scribe.Scripts.Data.ComponentData;

namespace Scribe.Scripts.Data.EntityDataTypes;

[GlobalClass]
public partial class MonsterData : EntityData
{
    [ExportGroup("Combat Components")]
    [Export] public HealthComponentData HealthData { get; set; }
    [Export] public MeleeAttackComponentData MeleeAttackData { get; set; }
    [Export] public MovementComponentData MovementData { get; set; }
    [Export] public AIComponentData AIData { get; set; }
    
    [ExportGroup("Monster Info")]
    [Export] public float ChallengeRating { get; set; } = 0.0f;
    [Export] public MonsterType MonsterType { get; set; }
    [Export] public CreatureSize Size { get; set; } = CreatureSize.Medium;
}

public enum MonsterType
{
    Humanoid,
    Beast,
    Undead,
    Dragon,
    Aberration,
    Celestial,
    Construct,
    Elemental,
    Fey,
    Fiend,
    Giant,
    Monstrosity,
    Ooze,
    Plant
}

public enum CreatureSize
{
    Tiny,
    Small,
    Medium,
    Large,
    Huge,
    Gargantuan
}