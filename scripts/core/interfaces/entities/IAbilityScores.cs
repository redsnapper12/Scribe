namespace Scribe.Scripts.Core.Interfaces.Entities;

public interface IAbilityScores : IEntityComponent
{
    int Strength { get; set; }
    int Dexterity { get; set; }
    int Constitution { get; set; }
    int Intelligence { get; set; }
    int Wisdom { get; set; }
    int Charisma { get; set; }

    int GetModifier(AbilityScore ability);
    int StrengthModifier { get; }
    int DexterityModifier { get; }
    int ConstitutionModifier { get; }
    int IntelligenceModifier { get; }
    int WisdomModifier { get; }
    int CharismaModifier { get; }
}

public enum AbilityScore
{
    Strength,
    Dexterity,
    Constitution,
    Intelligence,
    Wisdom,
    Charisma
}
