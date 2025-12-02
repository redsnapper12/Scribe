namespace Scribe.Scripts.Data.ComponentData.Traits;

/// <summary>
/// Generic trait implementation with no mechanical effects.
/// Used for purely flavorful/descriptive traits or as a placeholder for future trait types.
/// </summary>
public partial class GenericTrait : BaseTrait
{
    public GenericTrait(TraitInfo info) : base(info)
    {
    }

    // No overrides - inherits all default behavior from BaseTrait (no mechanical effects)
}
