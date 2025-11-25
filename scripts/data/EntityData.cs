using Godot;
using Godot.Collections;

namespace Scribe.Scripts.Data;

[GlobalClass]
public abstract partial class EntityData : Resource
{
    [Export] public string EntityName { get; set; } = "Unnamed Entity";
    [Export] public EntityType Type { get; set; }
    [Export] public Array<InterfaceType> ImplementedInterfaces { get; set; } = new();
    [Export] public string SpritePath { get; set; } = "";
}

public enum EntityType
{
    Character,
    Monster,
    Item,
    Furniture
}

public enum InterfaceType
{
    IDamageable,
    IMeleeAttacker,
    IAI,
    IRangedAttacker,
    ISpellcaster,
    IContainer,
    IEquipable,
    IUsable,
    ILootable,
    IMovable,
    IObstacle
}