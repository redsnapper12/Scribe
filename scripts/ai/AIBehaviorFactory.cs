using Godot;
using Godot.Collections;
using Scribe.Scripts.AI.Behaviors;
using Scribe.Scripts.Core.Interfaces;

namespace Scribe.Scripts.AI;

public static class AIBehaviorFactory
{
    public static IAIBehavior Create(AIBehaviorType behaviorType, Dictionary parameters)
    {
        IAIBehavior behavior = behaviorType switch
        {
            AIBehaviorType.SimpleAggressive => new SimpleAggressiveBehavior(),
            AIBehaviorType.Defensive => new SimpleAggressiveBehavior(),
            AIBehaviorType.RangedAttacker => new SimpleAggressiveBehavior(),
            AIBehaviorType.Support => new SimpleAggressiveBehavior(),
            _ => new SimpleAggressiveBehavior()
        };
        
        behavior.Initialize(parameters);
        return behavior;
    }
    
    public static IAIBehavior CreateFromScript(string scriptPath, Dictionary parameters)
    {
        var script = GD.Load<CSharpScript>(scriptPath);
        if (script == null)
        {
            GD.PrintErr($"Failed to load C# script: {scriptPath}");
            return Create(AIBehaviorType.SimpleAggressive, parameters);
        }
        
        var instance = script.New();
        if (instance.VariantType == Variant.Type.Object)
        {
            var obj = instance.AsGodotObject();
            if (obj is IAIBehavior behavior)
            {
                behavior.Initialize(parameters);
                return behavior;
            }
        }
        
        GD.PrintErr($"C# script at {scriptPath} does not implement IAIBehavior");
        return Create(AIBehaviorType.SimpleAggressive, parameters);
    }
}