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

public enum AbilityScore
{
    Strength,
    Dexterity,
    Constitution,
    Intelligence,
    Wisdom,
    Charisma
}
