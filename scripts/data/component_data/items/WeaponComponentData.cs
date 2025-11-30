using System;
using Godot;
using Godot.Collections;
using Scribe.Scripts.Core.Interfaces;
using Scribe.Scripts.Core.Interfaces.Items;

namespace Scribe.Scripts.Data.ComponentData;

/// <summary>
/// Resource data for weapon item components.
/// </summary>
[GlobalClass]
public partial class WeaponComponentData : ItemComponentData
{
    [Export] public int NumDice { get; set; } = 1;
    [Export] public DieType DieType { get; set; } = DieType.D6;
    [Export] public DamageType DamageType { get; set; } = DamageType.Slashing;
    [Export] public int MeleeRange { get; set; } = 5;
    [Export] public string AttackName { get; set; } = "Attack";
    [Export] public Array<WeaponProperties> Properties { get; set;}


    public override IItemComponent CreateComponent()
    {
        return new WeaponComponent
        {
            NumDice = this.NumDice,
            DieType = this.DieType,
            DamageType = this.DamageType,
            MeleeRange = this.MeleeRange,
            AttackName = this.AttackName,
        };
    }
}

/// <summary>
/// Implementation of IWeapon component.
/// </summary>
public partial class WeaponComponent : RefCounted, IItemComponent, IWeapon
{
    public int NumDice { get; set; }
    public DieType DieType { get; set; }
    public int DamageBonus { get; set; }
    public DamageType DamageType { get; set; }
    public int AttackBonus { get; set; }
    public int MeleeRange { get; set; }
    public string AttackName { get; set; }
    public bool IsTwoHanded { get; set; }
}

public enum WeaponProperties
{
    Ammunition,
    Finesse,
    Heavy,
    Light,
    Loading,
    Reach,
    Thrown,
    TwoHanded,
    Versatile,
}
