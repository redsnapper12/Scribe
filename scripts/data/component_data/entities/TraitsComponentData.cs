using Godot;
using Godot.Collections;
using System.Linq;
using Scribe.Scripts.Core.Interfaces;
using Scribe.Scripts.Core.Interfaces.Entities;

namespace Scribe.Scripts.Data.ComponentData;

[GlobalClass]
public partial class TraitsComponentData : EntityComponentData
{
    [Export] public Array<TraitInfo> Traits { get; set; } = [];

    public override IEntityComponent CreateComponent()
    {
        return new TraitsComponent
        {
            Traits = this.Traits
        };
    }
}

public partial class TraitsComponent : RefCounted, IEntityComponent, ITraits
{
    public Array<TraitInfo> Traits { get; set; } = new();

    public bool HasTrait(string traitName)
    {
        return Traits.Any(t => t.Name == traitName);
    }
}
