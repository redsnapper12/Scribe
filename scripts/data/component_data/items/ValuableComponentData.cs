using Godot;
using Scribe.Scripts.Items.Components;

namespace Scribe.Scripts.Items.Data;

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
public class ValuableComponent : IValuable
{
    public int GoldValue { get; set; }
    public ItemRarity Rarity { get; set; }
    public bool IsSellable { get; set; }
}
