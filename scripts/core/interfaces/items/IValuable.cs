namespace Scribe.Scripts.Core.Interfaces.Items;

/// <summary>
/// Optional component for items with monetary value.
/// </summary>
public interface IValuable
{
    /// <summary>
    /// Gold piece value of this item.
    /// </summary>
    int GoldValue { get; }

    /// <summary>
    /// Rarity tier of this item.
    /// </summary>
    ItemRarity Rarity { get; }

    /// <summary>
    /// Whether this item can be sold to vendors.
    /// </summary>
    bool IsSellable { get; }
}

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    VeryRare,
    Legendary,
    Artifact
}
