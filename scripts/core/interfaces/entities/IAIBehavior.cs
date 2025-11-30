using Godot.Collections;
using Scribe.Scripts.Entities;

namespace Scribe.Scripts.AI;

public interface IAIBehavior
{
    void Execute(Entity self, BattleContext context);
    void Initialize(Dictionary parameters);
}