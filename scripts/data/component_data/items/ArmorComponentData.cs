using Godot;
using Scribe.Scripts.Core.Interfaces.Items;

namespace Scribe.Scripts.Data.ComponentData;

/// <summary>
/// Resource data for armor item components.
/// </summary>
[GlobalClass]
public partial class ArmorComponentData : ItemComponentData
{
    [Export] public int ArmorClassBonus { get; set; } = 10;
    [Export] public ArmorCategory ArmorCategory { get; set; } = ArmorCategory.Light;
    [Export] public int MaxDexBonus { get; set; } = -1;
    [Export] public bool StealthDisadvantage { get; set; } = false;

    public override IItemComponent CreateComponent()
    {
        return new ArmorComponent
        {
            ArmorClassBonus = this.ArmorClassBonus,
            ArmorType = this.ArmorCategory,
            MaxDexBonus = this.MaxDexBonus,
            StealthDisadvantage = this.StealthDisadvantage
        };
    }
}

/// <summary>
/// Implementation of IArmor component.
/// </summary>
public partial class ArmorComponent : RefCounted, IItemComponent
{
    public int ArmorClassBonus { get; set; }
    public ArmorCategory ArmorType { get; set; }
    public int MaxDexBonus { get; set; }
    public bool StealthDisadvantage { get; set; }
}

public enum ArmorCategory
{
    None,
    Light,
    Medium,
    Heavy,
    Shield
}