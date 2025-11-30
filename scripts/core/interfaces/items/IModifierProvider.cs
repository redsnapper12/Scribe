using Scribe.Scripts.Entities;

namespace Scribe.Scripts.Core.Interfaces.Items;

/// <summary>
/// Optional component for items that provide stat modifiers when equipped.
/// This is a future integration point for the modifier/buff system.
/// </summary>
public interface IModifierProvider
{
    /// <summary>
    /// Applies modifiers to the entity wearing this item.
    /// Implementation will be added when modifier/buff system is created.
    /// </summary>
    void ApplyModifiers(Entity wearer);

    /// <summary>
    /// Removes modifiers from the entity.
    /// Implementation will be added when modifier/buff system is created.
    /// </summary>
    void RemoveModifiers(Entity wearer);
}
