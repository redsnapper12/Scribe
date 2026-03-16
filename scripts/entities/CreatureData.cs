using Godot;
using Scribe.Scripts.Data;
using Scribe.Scripts.Data.ComponentData;
using Scribe.Scripts.Entities;

[GlobalClass]
public partial class CreatureData : EntityData
{
    [ExportSubgroup("Required Components")]
    [Export] public AIComponentData AIComponentData { get; set; }
    [Export] public AbilityScoresComponentData AbilityScoresComponentData { get; set; }
    [Export] public ChallengeRatingComponentData ChallengeRatingComponentData { get; set; }
    [Export] public LanguagesComponentData LanguagesComponentData { get; set; }
    [Export] public SensesComponentData SensesComponentData { get; set; }
    [Export] public CreatureSkillsComponentData CreatureSkillsComponentData { get; set; }

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

        // Required creature components
        ComponentData.Add(AIComponentData);
        ComponentData.Add(AbilityScoresComponentData);
        ComponentData.Add(ChallengeRatingComponentData);
        ComponentData.Add(LanguagesComponentData);
        ComponentData.Add(SensesComponentData);
        ComponentData.Add(CreatureSkillsComponentData);
    }
}
