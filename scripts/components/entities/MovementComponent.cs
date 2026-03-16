using Godot;
using System;
using Scribe.Scripts.Core;

namespace Scribe.Scripts.Data.ComponentData;

public partial class MovementComponent : RefCounted, IEntityComponent
{
    public Vector2I GridPosition { get; set; }
    public int WalkSpeed { get; set; }
    public int FlySpeed { get; set; }
    public int SwimSpeed { get; set; }
    public int ClimbSpeed { get; set; }
    public int MovementRemaining { get; set; }

    [Signal]
    public delegate void MovedEventHandler(Vector2I from, Vector2I to, int cost);

    [Signal]
    public delegate void MovementExhaustedEventHandler();

    [Signal]
    public delegate void MovementResetEventHandler(int newMovement);

    public void ResetMovement()
    {
        MovementRemaining = WalkSpeed;
        EmitSignal(SignalName.MovementReset, MovementRemaining);
    }

    public bool CanAffordMove(int cost)
    {
        return MovementRemaining >= cost;
    }

    public void SpendMovement(int cost)
    {
        MovementRemaining = Math.Max(0, MovementRemaining - cost);

        if (MovementRemaining == 0)
        {
            EmitSignal(SignalName.MovementExhausted);
        }
    }

    /// <summary>
    /// Called by MovementService after successfully moving the entity.
    /// Emits the Moved signal for listeners (e.g., EntityNode for animation).
    /// </summary>
    public void NotifyMoved(Vector2I from, Vector2I to, int cost)
    {
        EmitSignal(SignalName.Moved, from, to, cost);
    }

    /// <summary>
    /// Gets the effective speed for a given terrain type.
    /// Accounts for swim/climb speeds if applicable.
    /// </summary>
    public int GetEffectiveSpeed(TerrainType terrain)
    {
        return terrain switch
        {
            TerrainType.Water when SwimSpeed > 0 => SwimSpeed,
            TerrainType.Climbing when ClimbSpeed > 0 => ClimbSpeed,
            _ => WalkSpeed
        };
    }
}
