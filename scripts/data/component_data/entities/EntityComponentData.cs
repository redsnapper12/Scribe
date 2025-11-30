using Godot;
using Scribe.Scripts.Core.Components;
using Scribe.Scripts.Core.Interfaces.Entities;

namespace Scribe.Scripts.Data.ComponentData;

/// <summary>
/// Base class for all entity component data resources.
/// Component data is serialized in Godot .tres files and used to instantiate entity component instances.
/// </summary>
[GlobalClass]
public abstract partial class EntityComponentData : Resource
{
    /// <summary>
    /// Creates an IEntityComponent instance from this data.
    /// Each subclass implements this to create its specific component type.
    /// </summary>
    public abstract IEntityComponent CreateComponent();
}
