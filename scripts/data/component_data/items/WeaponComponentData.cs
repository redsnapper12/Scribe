using System;
using System.Linq;
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
    /// <summary>
    /// List of attacks this weapon can make. Most weapons have 1 attack.
    /// </summary>
    [Export] public MeleeAttackData Attack { get; set; } = new();

    public override IItemComponent CreateComponent()
    {
        return new WeaponComponent
        {
            Attack = this.Attack
        };
    }
}

/// <summary>
/// Implementation of IWeapon component.
/// </summary>
public partial class WeaponComponent : RefCounted, IItemComponent, IWeapon
{
    /// <summary>
    /// Attack this weapon can make.
    /// </summary>
    public MeleeAttackData Attack { get; set; } = new();

    /// <summary>
    /// Check if this weapon has a specific property (Finesse, Reach, etc.)
    /// </summary>
    public bool HasProperty(WeaponProperties property)
    {
        if (Attack.Properties?.Contains(property) ?? false)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
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

/// <summary>
/// Weapon proficiency categories based on D&D 5e.
/// </summary>
public enum WeaponCategory
{
    None,      // No category (used for natural weapons)
    Simple,    // Simple weapons (club, dagger, etc.)
    Martial,   // Martial weapons (longsword, greatsword, etc.)
}
