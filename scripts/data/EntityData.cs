using Godot;
using Godot.Collections;
using Scribe.Scripts.Core.Interfaces.Entities;
using Scribe.Scripts.Core.Services;
using Scribe.Scripts.Data.ComponentData;
using Scribe.Scripts.Entities;

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
    [Export] public MeleeAttackComponentData MeleeAttackComponentData { get; set; }

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

        return entity;
    }

    private void AppendRequiredComponents()
    {
        ComponentData.Add(HealthComponentData);
        ComponentData.Add(MovementComponentData);
        ComponentData.Add(MeleeAttackComponentData);
    }

    /// <summary>
    /// Removes duplicate component data from the ComponentData array.
    /// Keeps only the last occurrence of each component type.
    /// </summary>
    private void FilterDuplicates()
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