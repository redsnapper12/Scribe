using Godot;
using Scribe.Scripts.Core.Interfaces.Items;

namespace Scribe.Scripts.Data.ComponentData;

/// <summary>
/// Base class for all item component data resources.
/// Component data is serialized in Godot .tres files and used to instantiate IItemComponent instances.
/// </summary>
[GlobalClass]
public abstract partial class ItemComponentData : Resource
{
    /// <summary>
    /// Creates an IItemComponent instance from this data.
    /// Each subclass implements this to create its specific component type.
    /// </summary>
    public abstract IItemComponent CreateComponent();
}
