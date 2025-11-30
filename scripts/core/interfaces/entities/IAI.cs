using Scribe.Scripts.AI;
using Scribe.Scripts.Entities;

namespace Scribe.Scripts.Core.Interfaces;

public interface IAI
{
    void Act(Entity self, BattleContext context);
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