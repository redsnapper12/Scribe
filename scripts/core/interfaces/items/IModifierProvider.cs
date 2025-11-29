using Scribe.Scripts.Core.Entities;

namespace Scribe.Scripts.Items.Components;

/// <summary>
/// Optional component for items that provide stat modifiers when equipped.
/// This is a future integration point for the modifier/buff system.
/// </summary>
public interface IModifierProvider : IItemComponent
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
