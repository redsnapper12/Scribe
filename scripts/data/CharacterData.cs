using Godot;
using Scribe.Scripts.Data.ComponentData;

namespace Scribe.Scripts.Data.EntityDataTypes;

[GlobalClass]
public partial class CharacterData : EntityData
{
    [ExportGroup("Combat Components")]
    [Export] public HealthComponentData HealthData { get; set; }
    [Export] public MeleeAttackComponentData MeleeAttackData { get; set; }
    
    [ExportGroup("Character Info")]
    [Export] public CharacterClass Class { get; set; }
    [Export] public int Level { get; set; } = 1;
    
    [ExportGroup("Ability Scores")]
    [Export] public int Strength { get; set; } = 10;
    [Export] public int Dexterity { get; set; } = 10;
    [Export] public int Constitution { get; set; } = 10;
    [Export] public int Intelligence { get; set; } = 10;
    [Export] public int Wisdom { get; set; } = 10;
    [Export] public int Charisma { get; set; } = 10;
}

public enum CharacterClass
{
    Fighter,
    Wizard,
    Rogue,
    Cleric,
    Barbarian,
    Paladin,
    Ranger,
    Bard,
    Druid,
    Monk,
    Warlock,
    Sorcerer
}