using Scribe.Scripts.AI;

namespace Scribe.Scripts.Core.Interfaces;

public interface IAI
{
    void Act(BattleContext context);
    AIBehaviorType BehaviorType { get; }
}

public enum AIBehaviorType
{
    SimpleAggressive,
    Defensive,
    RangedAttacker,
    Support,
    Custom
}