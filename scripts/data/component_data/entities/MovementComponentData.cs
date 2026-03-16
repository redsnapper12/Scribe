using Godot;
using System;
using Scribe.Scripts.Core;

namespace Scribe.Scripts.Data.ComponentData;

[GlobalClass]
public partial class MovementComponentData : EntityComponentData
{
    [Export] public int WalkSpeed { get; set; } = 30;
    [Export] public int FlySpeed { get; set; } = 0;
    [Export] public int SwimSpeed { get; set; } = 0;
    [Export] public int ClimbSpeed { get; set; } = 0;

    public override IEntityComponent CreateComponent()
    {
        return new MovementComponent
        {
            WalkSpeed = this.WalkSpeed,
            FlySpeed = this.FlySpeed,
            SwimSpeed = this.SwimSpeed,
            ClimbSpeed = this.ClimbSpeed,
            MovementRemaining = this.WalkSpeed
        };
    }
}
