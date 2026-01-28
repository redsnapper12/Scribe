using Godot;
using Godot.Collections;
using Scribe.Scripts.AI;
using Scribe.Scripts.Entities;

namespace Scribe.Scripts.Data.ComponentData;

public enum AIBehaviorType
{
    SimpleAggressive,
    Custom
}

[GlobalClass]
public partial class AIComponentData : EntityComponentData
{
    [Export] public AIBehaviorType BehaviorType { get; set; } = AIBehaviorType.SimpleAggressive;
    [Export] public string CustomBehaviorScript { get; set; } = "";
    [Export] public Dictionary Parameters { get; set; } = new();

    public override IEntityComponent CreateComponent()
    {
        var component = new AIComponent
        {
            BehaviorType = this.BehaviorType
        };

        // Create behavior using factory
        if (!string.IsNullOrEmpty(this.CustomBehaviorScript))
        {
            component.Behavior = AIBehaviorFactory.CreateFromScript(
                this.CustomBehaviorScript,
                this.Parameters
            );
        }
        else
        {
            component.Behavior = AIBehaviorFactory.Create(
                this.BehaviorType,
                this.Parameters
            );
        }

        return component;
    }
}

public partial class AIComponent : RefCounted, IEntityComponent
{
    public AIBehaviorType BehaviorType { get; set; }
    public IAIBehavior Behavior { get; set; }

    public void SetBehavior(IAIBehavior behavior)
    {
        Behavior = behavior;
    }

    public void Act(Entity self, BattleContext context)
    {
        Behavior?.Execute(self, context);
    }
}
