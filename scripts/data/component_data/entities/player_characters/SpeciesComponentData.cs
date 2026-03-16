using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Scribe.Scripts.Data;
using Scribe.Scripts.Data.ComponentData;

[GlobalClass]
public partial class SpeciesComponentData : EntityComponentData
{
    [ExportGroup("Species Details")]
    [Export] public string SpeciesName { get; set; } = "Example Name";
    [Export(PropertyHint.MultilineText)] public string SpeciesDescription { get; set; } = "";
    [Export] public Texture2D Icon { get; set; } = null;

    [ExportGroup("Species Traits")]
    [Export] public EntityType CreatureType { get; set; } = EntityType.Humanoid;
    [Export] public EntitySize Size { get; set; } = EntitySize.Medium;
    [Export] public int Speed { get; set; } = 30;



    public override IEntityComponent CreateComponent()
    {
        return new SpeciesComponent
        {
            // Species Details
            SpeciesName = this.SpeciesName,
            SpeciesDescription = this.SpeciesDescription,
            Icon = this.Icon,

            // Species Traits
            CreatureType = this.CreatureType,
            Size = this.Size,
            Speed = this.Speed
        };
    }
}
