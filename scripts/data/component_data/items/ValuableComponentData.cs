using Godot;
using Scribe.Scripts.Core.Interfaces.Items;

namespace Scribe.Scripts.Data.ComponentData;

/// <summary>
/// Resource data for valuable item components.
/// </summary>
[GlobalClass]
public partial class ValuableComponentData : ItemComponentData
{
    [Export] public int GoldValue { get; set; } = 0;
    [Export] public ItemRarity Rarity { get; set; } = ItemRarity.Common;
    [Export] public bool IsSellable { get; set; } = true;

    public override IItemComponent CreateComponent()
    {
        return new ValuableComponent
        {
            GoldValue = this.GoldValue,
            Rarity = this.Rarity,
            IsSellable = this.IsSellable
        };
    }
}

/// <summary>
/// Implementation of IValuable component.
/// </summary>
public partial class ValuableComponent : RefCounted, IItemComponent, IValuable
{
    public int GoldValue { get; set; }
    public ItemRarity Rarity { get; set; }
    public bool IsSellable { get; set; }
}
