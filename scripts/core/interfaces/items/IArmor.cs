namespace Scribe.Scripts.Items.Components;

/// <summary>
/// Optional component for items that provide armor.
/// </summary>
public interface IArmor : IItemComponent
{
    /// <summary>
    /// Armor class bonus provided by this armor.
    /// </summary>
    int ArmorClassBonus { get; }

    /// <summary>
    /// Type of armor.
    /// </summary>
    ArmorType ArmorType { get; }

    /// <summary>
    /// Maximum dexterity bonus allowed while wearing this armor.
    /// -1 means no limit.
    /// </summary>
    int MaxDexBonus { get; }

    /// <summary>
    /// Whether this armor imposes disadvantage on stealth checks.
    /// </summary>
    bool StealthDisadvantage { get; }
}

public enum ArmorType
{
    Light,
    Medium,
    Heavy,
    Shield
}
