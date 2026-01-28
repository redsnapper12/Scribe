using Godot;

namespace Scribe.Scripts.Data.ComponentData;

[GlobalClass]
public partial class ChallengeRatingComponentData : EntityComponentData
{
    [Export] public float ChallengeRating { get; set; } = 0.25f;
    [Export] public int ExperiencePoints { get; set; } = 50;

    public override IEntityComponent CreateComponent()
    {
        return new ChallengeRatingComponent
        {
            ChallengeRating = this.ChallengeRating,
            ExperiencePoints = this.ExperiencePoints
        };
    }
}

public partial class ChallengeRatingComponent : RefCounted, IEntityComponent
{
    public float ChallengeRating { get; set; }

    public int ExperiencePoints { get; set; }
}
