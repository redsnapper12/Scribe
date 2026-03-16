using Godot;
using Godot.Collections;
using Scribe.Scripts.Data.ComponentData;
using Scribe.Scripts.Data;

[GlobalClass]
public partial class ProficiencyComponentData : EntityComponentData
{
    [Export] public int ProficiencyBonus { get; set; } = 2;

    /// <summary>
    /// Weapon categories this entity is proficient with (Simple, Martial).
    /// </summary>
    [Export] public Array<WeaponCategory> WeaponCategories { get; set; } = new();

    /// <summary>
    /// Specific weapons this entity is proficient with by name.
    /// Used for edge cases (e.g., Wizard proficient with Dagger, Quarterstaff).
    /// </summary>
    [Export] public Array<string> SpecificWeapons { get; set; } = new();

    public override IEntityComponent CreateComponent()
    {
        return new ProficiencyComponent
        {
            ProficiencyBonus = this.ProficiencyBonus,
            WeaponCategories = this.WeaponCategories,
            SpecificWeapons = this.SpecificWeapons
        };
    }
}
