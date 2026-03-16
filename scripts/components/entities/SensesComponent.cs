using Godot;

namespace Scribe.Scripts.Data.ComponentData;

public partial class SensesComponent : RefCounted, IEntityComponent
{
    public int DarkvisionRange { get; set; }
    public int BlindsightRange { get; set; }
    public int TremorsenseRange { get; set; }
    public int TruesightRange { get; set; }
    public int PassivePerception { get; set; }
}
