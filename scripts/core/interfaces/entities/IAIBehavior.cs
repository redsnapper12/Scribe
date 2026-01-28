using Godot.Collections;
using Scribe.Scripts.AI;
using Scribe.Scripts.Entities;

public interface IAIBehavior
{
    public void Execute(Entity self, BattleContext battleContext);
    public void Initialize(Dictionary parameters);
}