using Godot;
using Godot.Collections;
using Scribe.Scripts.Data.ComponentData;


[GlobalClass]
public partial class BackgroundComponentData : EntityComponentData
{
    [Export] public string BackgroundName { get; set; } = "";
    [Export(PropertyHint.MultilineText)] public string BackgroundDescription { get; set; } = "";
    [Export] public Array<AbilityScore> AbilityScores { get; set; } = [AbilityScore.Strength, AbilityScore.Dexterity, AbilityScore.Constitution];
    [Export] public Array<Skill> Proficiencies { get; set; } = [];

    public override IEntityComponent CreateComponent()
    {
        return new BackgroundComponent
        {
            BackgroundName = this.BackgroundName,
            BackgroundDescription = this.BackgroundDescription,
            AbilityScores = this.AbilityScores,
            Proficiencies = this.Proficiencies
        };
    }
}

public partial class BackgroundComponent : RefCounted, IEntityComponent
{
    public string BackgroundName { get; set; } = "";
    public string BackgroundDescription { get; set; } = "";
    public Array<AbilityScore> AbilityScores { get; set; } = [];
    public Array<Skill> Proficiencies { get; set; } = [];
    
}
