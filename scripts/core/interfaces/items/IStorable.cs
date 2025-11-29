using Godot;

namespace Scribe.Scripts.Items.Components;

/// <summary>
/// Required interface for all items. Defines inventory and storage properties.
/// </summary>
public interface IStorable : IItemComponent
{
    /// <summary>
    /// Whether this item can stack with identical items.
    /// </summary>
    bool IsStackable { get; }

    /// <summary>
    /// Maximum number of items that can be in a single stack.
    /// </summary>
    int MaxStackSize { get; }

    /// <summary>
    /// Weight of a single item in pounds.
    /// </summary>
    float Weight { get; }

    /// <summary>
    /// Icon texture displayed in inventory UI.
    /// </summary>
    Texture2D Icon { get; }
}
