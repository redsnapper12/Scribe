namespace Scribe.Scripts.Core.Interfaces.Entities;

public interface IChallengeRating : IEntityComponent
{
    float ChallengeRating { get; set; }
    int ProficiencyBonus { get; set; }
    int ExperiencePoints { get; set; }
}
