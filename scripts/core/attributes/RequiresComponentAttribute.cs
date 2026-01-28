using System;

namespace Scribe.Scripts.Core.Attributes;

/// <summary>
/// Marks an Entity class as requiring specific components at runtime.
/// Validation occurs during Entity creation in EntityData.CreateEntity().
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class RequiresComponentAttribute : Attribute
{
    public Type ComponentType { get; }

    public RequiresComponentAttribute(Type componentType)
    {
        if (!typeof(IEntityComponent).IsAssignableFrom(componentType))
        {
            throw new ArgumentException(
                $"Type {componentType.Name} does not implement IEntityComponent",
                nameof(componentType)
            );
        }
        ComponentType = componentType;
    }
}
