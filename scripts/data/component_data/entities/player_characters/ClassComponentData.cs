using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Scribe.Scripts.Data.ComponentData;

[GlobalClass]
public partial class ClassComponentData : EntityComponentData
{
    [ExportGroup("Class Details")]
    [Export] public string ClassName { get; set; } = "Example Name";
    [Export(PropertyHint.MultilineText)] public string ClassDescription { get; set; } = "";
    [Export] public Texture2D Icon { get; set; } = null;

    [ExportGroup("Class Traits")]
    [Export] public AbilityScore PrimaryAbility { get; set; } = AbilityScore.Strength;
    [Export] public DieType HitPointDie { get; set; } = DieType.D6;
    [Export] public Array<AbilityScore> SavingThrowProficiencies { get; set; } = [];
    [Export] public Array<WeaponCategory> WeaponProficiencies { get; set; } = [];
    [Export] public Array<ArmorCategory> ArmorTraining { get; set; } = [];


    public override IEntityComponent CreateComponent()
    {
        return new ClassComponent
        {
            // Class Details
            ClassName = this.ClassName,
            ClassDescription = this.ClassDescription,
            Icon = this.Icon,

            // Class Traits
            PrimaryAbility = this.PrimaryAbility,
            HitPointDie = this.HitPointDie,
            SavingThrowProficiencies = this.SavingThrowProficiencies,
            WeaponProficiencies = this.WeaponProficiencies,
            ArmorTraining = this.ArmorTraining
        };
    }
}
