using Godot;
using Scribe.Scripts.Data;
using Scribe.Scripts.Data.ComponentData;

public partial class SpeciesComponent : RefCounted, IEntityComponent
{
    public string SpeciesName { get; set; } = "";
    public string SpeciesDescription { get; set; } = "";
    public Texture2D Icon { get; set; } = null;
    public EntityType CreatureType { get; set; } = EntityType.Humanoid;
    public EntitySize Size { get; set; } = EntitySize.Medium;
    public int Speed { get; set; } = 30;
}
