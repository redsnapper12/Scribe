using Godot;
using Godot.Collections;
using Scribe.Scripts.Data.ComponentData;

public partial class BackgroundComponent : RefCounted, IEntityComponent
{
    public string BackgroundName { get; set; } = "";
    public string BackgroundDescription { get; set; } = "";
    public Array<AbilityScore> AbilityScores { get; set; } = [];
    public Array<Skill> Proficiencies { get; set; } = [];

}
