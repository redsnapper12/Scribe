using Godot;
using Godot.Collections;
using System;
using Scribe.Scripts.Core.Services;
using Scribe.Scripts.Data.ComponentData;
using Scribe.Scripts.Entities;
using Scribe.Scripts.Core.Attributes;

namespace Scribe.Scripts.Data;

[GlobalClass]
public partial class EntityData : Resource
{
    [ExportGroup("Identity")]
    [Export] public string EntityName { get; set; } = "Unnamed Entity";
    [Export] public EntityType Type { get; set; } = EntityType.Humanoid;
    [Export] public EntitySize Size { get; set; } = EntitySize.Medium;
    [Export] public EntityAlignment Alignment { get; set; } = EntityAlignment.TrueNeutral;
    [Export] public ControllerType Controller { get; set; } = ControllerType.AI;
    [Export] public Texture2D Icon { get; set; } = null;

    [ExportGroup("Components")]

    [ExportSubgroup("Required Components")]
    [Export] public HealthComponentData HealthComponentData { get; set; }
    [Export] public MovementComponentData MovementComponentData { get; set; }
    [Export] public AttackComponentData AttackComponentData { get; set; }
    [Export] public ActionEconomyComponentData ActionEconomyComponentData { get; set; }

    [ExportSubgroup("Additional Components")]
    [Export] public Array<EntityComponentData> ComponentData { get; set; } = new();

    /// <summary>
    /// Factory method to create an Item instance from this data.
    /// Each subclass can override to create specialized item types.
    /// </summary>
    public virtual Entity CreateEntity(Vector2I gridPosition)
    {
        var entity = new Entity
        {
            EntityName = this.EntityName,
            Icon = this.Icon,
            Controller = this.Controller,
            Size = this.Size,
            Type = this.Type,
            Alignment = this.Alignment,
            GridPosition = gridPosition
        };

        AppendRequiredComponents();
        FilterDuplicates();

        // Add all components from component data
        foreach (var componentData in ComponentData)
        {
            if (componentData != null)
            {
                var component = componentData.CreateComponent();
                if (component is IEntityComponent entityComponent)
                {
                    entity.AddComponent(entityComponent);
                }
            }
        }

        ValidateRequiredComponents(entity);
        return entity;
    }

    /// <summary>
    /// Validates that the entity has all required components specified by RequiresComponent attributes.
    /// Throws InvalidOperationException if any required components are missing.
    /// </summary>
    private void ValidateRequiredComponents(Entity entity)
    {
        var entityType = entity.GetType();
        var attributes = entityType.GetCustomAttributes(typeof(RequiresComponentAttribute), true);

        foreach (RequiresComponentAttribute attr in attributes)
        {
            var hasComponentMethod = typeof(Entity).GetMethod("HasComponent");
            var genericMethod = hasComponentMethod.MakeGenericMethod(attr.ComponentType);
            bool hasComponent = (bool)genericMethod.Invoke(entity, null);

            if (!hasComponent)
            {
                throw new InvalidOperationException(
                    $"Entity '{entity.EntityName}' of type {entityType.Name} requires component " +
                    $"{attr.ComponentType.Name} but it was not added during creation. " +
                    $"Check your EntityData configuration."
                );
            }
        }
    }

    /// <summary>
    /// Adds required components to the ComponentData array
    /// </summary>
    public virtual void AppendRequiredComponents()
    {
        ComponentData.Add(HealthComponentData);
        ComponentData.Add(MovementComponentData);
        ComponentData.Add(AttackComponentData);
        ComponentData.Add(ActionEconomyComponentData);
    }

    /// <summary>
    /// Removes duplicate component data from the ComponentData array.
    /// Keeps only the last occurrence of each component type.
    /// </summary>
    public void FilterDuplicates()
    {
        var seen = new System.Collections.Generic.Dictionary<System.Type, EntityComponentData>();
        var filtered = new Array<EntityComponentData>();

        // Iterate through all components
        foreach (var componentData in ComponentData)
        {
            if (componentData != null)
            {
                var type = componentData.GetType();
                // Store the component, overwriting any previous one of the same type
                seen[type] = componentData;
            }
        }

        // Build the filtered array with unique components
        foreach (var kvp in seen)
        {
            filtered.Add(kvp.Value);
        }

        ComponentData = filtered;
    }
}

public enum EntityType
{
    Aberration,
    Beast,
    Celestial,
    Construct,
    Dragon,
    Elemental,
    Fey,
    Fiend,
    Giant,
    Humanoid,
    Monstrosity,
    Ooze,
    Plant,
    Undead
}

public enum EntitySize
{
    Tiny,
    Small,
    Medium,
    Large,
    Huge,
    Gargantuan
}

public enum EntityAlignment
{
    ChaoticGood,
    ChaoticNeutral,
    ChaoticEvil,
    NeutralGood,
    TrueNeutral,
    NeutralEvil,
    LawfulGood,
    LawfulNeutral,
    LawfulEvil
}