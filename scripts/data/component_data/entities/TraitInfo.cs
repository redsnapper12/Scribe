using Godot;
using Godot.Collections;
using System;
using Scribe.Scripts.Data.ComponentData.Traits;

namespace Scribe.Scripts.Data.ComponentData;

/// <summary>
/// Serializable trait data resource. Stores trait type and optional configuration parameters.
/// Designers create these in Godot editor and select the trait type from a dropdown.
/// </summary>
[GlobalClass]
public partial class TraitInfo : Resource
{
    [Export] public string Name { get; set; } = "";
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";

    /// <summary>
    /// Trait type determines which mechanical behavior is applied.
    /// </summary>
    [Export] public TraitType Type { get; set; } = TraitType.None;

    /// <summary>
    /// Optional configuration parameters for traits (e.g., "RangeInFeet" for Pack Tactics).
    /// </summary>
    [Export] public Dictionary Parameters { get; set; } = new();

    public TraitInfo() : base()
    {
    }

    /// <summary>
    /// Factory method to create runtime trait behavior based on the trait type.
    /// </summary>
    public BaseTrait CreateTrait()
    {
        return Type switch
        {
            TraitType.NimbleEscape => new NimbleEscapeTrait(this),
            TraitType.PackTactics => new PackTacticsTrait(this),
            TraitType.KeenSenses => new KeenSensesTrait(this),
            TraitType.SunlightSensitivity => new SunlightSensitivityTrait(this),
            TraitType.None => new GenericTrait(this),
            _ => new GenericTrait(this)
        };
    }
}
