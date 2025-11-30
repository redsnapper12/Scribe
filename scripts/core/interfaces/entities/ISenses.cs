namespace Scribe.Scripts.Core.Interfaces.Entities;

public interface ISenses : IEntityComponent
{
    int DarkvisionRange { get; set; }
    int BlindsightRange { get; set; }
    int TremorsenseRange { get; set; }
    int TruesightRange { get; set; }
    int PassivePerception { get; set; }
}
