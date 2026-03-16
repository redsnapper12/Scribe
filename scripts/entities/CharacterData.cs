using Godot;
using Scribe.Scripts.Data;
using Scribe.Scripts.Data.ComponentData;
using Scribe.Scripts.Entities;

[GlobalClass]
public partial class CharacterData : EntityData
{
    [ExportSubgroup("Character Origin")]
    [Export] public ClassComponentData ClassComponentData { get; set; }
    [Export] public SpeciesComponentData SpeciesComponentData { get; set; }
    [Export] public BackgroundComponentData BackgroundComponentData { get; set; }

    [ExportSubgroup("Required Components")]
    [Export] public AbilityScoresComponentData AbilityScoresComponentData { get; set; }
    [Export] public EquipmentComponentData EquipmentComponentData { get; set; }
    [Export] public LanguagesComponentData LanguagesComponentData { get; set; }
    [Export] public CharacterSkillsComponentData CharacterSkillsComponentData { get; set; }

    public override Entity CreateEntity(Vector2I gridPosition)
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

    public override void AppendRequiredComponents()
    {
        // Required entity components
        ComponentData.Add(HealthComponentData);
        ComponentData.Add(MovementComponentData);
        ComponentData.Add(AttackComponentData);
        ComponentData.Add(ActionEconomyComponentData);

        // Character origin components
        ComponentData.Add(ClassComponentData);
        ComponentData.Add(SpeciesComponentData);
        ComponentData.Add(BackgroundComponentData);

        // Required character components
        ComponentData.Add(AbilityScoresComponentData);
        ComponentData.Add(EquipmentComponentData);
        ComponentData.Add(LanguagesComponentData);
        ComponentData.Add(CharacterSkillsComponentData);
    }
}
