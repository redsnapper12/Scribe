using Godot;
using Godot.Collections;
using System;

namespace Scribe.Scripts.Data.ComponentData;

[GlobalClass]
public partial class HealthComponentData : EntityComponentData
{
    [Export] public int MaxHP { get; set; } = 10;
    [Export] public int ArmorClass { get; set; } = 10;
    [Export] public Array<DamageType> Resistances { get; set; } = new();
    [Export] public Array<DamageType> Vulnerabilities { get; set; } = new();
    [Export] public Array<DamageType> Immunities { get; set; } = new();

    public override IEntityComponent CreateComponent()
    {
        return new HealthComponent
        {
            MaxHP = this.MaxHP,
            CurrentHP = this.MaxHP,
            ArmorClass = this.ArmorClass,
            Resistances = this.Resistances,
            Vulnerabilities = this.Vulnerabilities,
            Immunities = this.Immunities
        };
    }
}

public enum DamageType
{
    Bludgeoning,
    Piercing,
    Slashing,
    Fire,
    Cold,
    Lightning,
    Thunder,
    Acid,
    Poison,
    Necrotic,
    Radiant,
    Force,
    Psychic
}
