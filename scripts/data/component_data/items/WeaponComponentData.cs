using Godot;
using Scribe.Scripts.Core.Interfaces;
using Scribe.Scripts.Items.Components;

namespace Scribe.Scripts.Items.Data;

/// <summary>
/// Resource data for weapon item components.
/// </summary>
[GlobalClass]
public partial class WeaponComponentData : ItemComponentData
{
    [Export] public int NumDice { get; set; } = 1;
    [Export] public DieType DieType { get; set; } = DieType.D6;
    [Export] public int DamageBonus { get; set; } = 0;
    [Export] public DamageType DamageType { get; set; } = DamageType.Slashing;
    [Export] public int AttackBonus { get; set; } = 0;
    [Export] public int MeleeRange { get; set; } = 5;
    [Export] public string AttackName { get; set; } = "Attack";
    [Export] public bool IsTwoHanded { get; set; } = false;

    public override IItemComponent CreateComponent()
    {
        return new WeaponComponent
        {
            NumDice = this.NumDice,
            DieType = this.DieType,
            DamageBonus = this.DamageBonus,
            DamageType = this.DamageType,
            AttackBonus = this.AttackBonus,
            MeleeRange = this.MeleeRange,
            AttackName = this.AttackName,
            IsTwoHanded = this.IsTwoHanded
        };
    }
}

/// <summary>
/// Implementation of IWeapon component.
/// </summary>
public class WeaponComponent : IWeapon
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
