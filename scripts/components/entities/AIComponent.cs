using Godot;
using Scribe.Scripts.AI;
using Scribe.Scripts.Entities;

namespace Scribe.Scripts.Data.ComponentData;

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
