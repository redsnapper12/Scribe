using Godot.Collections;
using Scribe.Scripts.Data.ComponentData;

namespace Scribe.Scripts.Core.Interfaces.Entities;

public interface ITraits : IEntityComponent
{
    Array<TraitInfo> Traits { get; set; }
    bool HasTrait(string traitName);
}
