using Godot;
using Scribe.Scripts.AI;
using Scribe.Scripts.Core.Interfaces;
using Scribe.Scripts.Data.ComponentData;

namespace Scribe.Scripts.Core.Components;

public partial class AIComponent : Component, IAI
{
    private AIComponentData _data;
    private IAIBehavior _behavior;
    
    public AIBehaviorType BehaviorType { get; private set; }
    
    public void Initialize(AIComponentData data)
    {
        _data = data;
        BehaviorType = data.BehaviorType;
        
        if (!string.IsNullOrEmpty(data.CustomBehaviorScript))
        {
            _behavior = AIBehaviorFactory.CreateFromScript(data.CustomBehaviorScript, data.Parameters);
        }
        else
        {
            _behavior = AIBehaviorFactory.Create(data.BehaviorType, data.Parameters);
        }
    }
    
    public override void Initialize()
    {
    }
    
    public void SetBehavior(IAIBehavior behavior)
    {
        _behavior = behavior;
    }
    
    public void Act(BattleContext context)
    {
        _behavior?.Execute(_entity, context);
    }
}