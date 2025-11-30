using Godot;
using Scribe.Scripts.Core.Interfaces;
using Scribe.Scripts.Core.Interfaces.Entities;

namespace Scribe.Scripts.Data.ComponentData;

[GlobalClass]
public partial class ChallengeRatingComponentData : EntityComponentData
{
    [Export] public float ChallengeRating { get; set; } = 0.25f;
    [Export] public int ProficiencyBonus { get; set; } = 2;
    [Export] public int ExperiencePoints { get; set; } = 50;

    public override IEntityComponent CreateComponent()
    {
        return new ChallengeRatingComponent
        {
            ChallengeRating = this.ChallengeRating,
            ProficiencyBonus = this.ProficiencyBonus,
            ExperiencePoints = this.ExperiencePoints
        };
    }
}

public partial class ChallengeRatingComponent : RefCounted, IEntityComponent, IChallengeRating
{
    public float ChallengeRating { get; set; }
    public int ProficiencyBonus { get; set; }
    public int ExperiencePoints { get; set; }
}
