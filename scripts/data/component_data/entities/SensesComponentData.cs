using Godot;

namespace Scribe.Scripts.Data.ComponentData;

[GlobalClass]
public partial class SensesComponentData : EntityComponentData
{
    [Export] public int DarkvisionRange { get; set; } = 0;
    [Export] public int BlindsightRange { get; set; } = 0;
    [Export] public int TremorsenseRange { get; set; } = 0;
    [Export] public int TruesightRange { get; set; } = 0;
    [Export] public int PassivePerception { get; set; } = 10;

    public override IEntityComponent CreateComponent()
    {
        return new SensesComponent
        {
            DarkvisionRange = this.DarkvisionRange,
            BlindsightRange = this.BlindsightRange,
            TremorsenseRange = this.TremorsenseRange,
            TruesightRange = this.TruesightRange,
            PassivePerception = this.PassivePerception
        };
    }
}
