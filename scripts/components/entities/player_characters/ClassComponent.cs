using Godot;
using Godot.Collections;
using Scribe.Scripts.Data.ComponentData;

public partial class ClassComponent : RefCounted, IEntityComponent
{
    // Class Details
    public string ClassName { get; set; } = "";
    public string ClassDescription { get; set; } = "";
    public Texture2D Icon { get; set; } = null;

    // Class Traits
    public AbilityScore PrimaryAbility { get; set; } = AbilityScore.Strength;
    public DieType HitPointDie { get; set; } = DieType.D6;
    public Array<AbilityScore> SavingThrowProficiencies { get; set; } = [];
    public Array<WeaponCategory> WeaponProficiencies { get; set; } = [];
    public Array<ArmorCategory> ArmorTraining { get; set; } = [];
}
