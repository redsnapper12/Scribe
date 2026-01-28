using Godot;

namespace Scribe.Scripts.Data.ComponentData;

[GlobalClass]
public partial class AbilityScoresComponentData : EntityComponentData
{
    [Export] public int Strength { get; set; } = 10;
    [Export] public int Dexterity { get; set; } = 10;
    [Export] public int Constitution { get; set; } = 10;
    [Export] public int Intelligence { get; set; } = 10;
    [Export] public int Wisdom { get; set; } = 10;
    [Export] public int Charisma { get; set; } = 10;

    public override IEntityComponent CreateComponent()
    {
        return new AbilityScoresComponent
        {
            Strength = this.Strength,
            Dexterity = this.Dexterity,
            Constitution = this.Constitution,
            Intelligence = this.Intelligence,
            Wisdom = this.Wisdom,
            Charisma = this.Charisma
        };
    }
}

public partial class AbilityScoresComponent : RefCounted, IEntityComponent
{
    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Constitution { get; set; }
    public int Intelligence { get; set; }
    public int Wisdom { get; set; }
    public int Charisma { get; set; }

    public int StrengthModifier => GetModifier(AbilityScore.Strength);
    public int DexterityModifier => GetModifier(AbilityScore.Dexterity);
    public int ConstitutionModifier => GetModifier(AbilityScore.Constitution);
    public int IntelligenceModifier => GetModifier(AbilityScore.Intelligence);
    public int WisdomModifier => GetModifier(AbilityScore.Wisdom);
    public int CharismaModifier => GetModifier(AbilityScore.Charisma);

    public int GetModifier(AbilityScore ability)
    {
        int score = ability switch
        {
            AbilityScore.Strength => Strength,
            AbilityScore.Dexterity => Dexterity,
            AbilityScore.Constitution => Constitution,
            AbilityScore.Intelligence => Intelligence,
            AbilityScore.Wisdom => Wisdom,
            AbilityScore.Charisma => Charisma,
            _ => 10
        };

        return (score - 10) / 2;
    }
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