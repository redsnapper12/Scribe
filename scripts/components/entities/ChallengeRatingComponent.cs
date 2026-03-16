using Godot;

namespace Scribe.Scripts.Data.ComponentData;

public partial class ChallengeRatingComponent : RefCounted, IEntityComponent
{
    public float ChallengeRating { get; set; }

    public int ExperiencePoints { get; set; }
}
