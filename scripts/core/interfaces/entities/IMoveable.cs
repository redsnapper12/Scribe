using Godot;

namespace Scribe.Scripts.Core.Interfaces;

public interface IMovable
{
    Vector2I GridPosition { get; set; }
    int WalkSpeed { get; }
    int FlySpeed { get; }
    int SwimSpeed { get; }
    int ClimbSpeed { get; }
    int MovementRemaining { get; }

    void ResetMovement();
    bool CanAffordMove(int cost);
    void SpendMovement(int cost);
}