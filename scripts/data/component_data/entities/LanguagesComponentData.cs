using Godot;
using Godot.Collections;

namespace Scribe.Scripts.Data.ComponentData;

[GlobalClass]
public partial class LanguagesComponentData : EntityComponentData
{
    [Export] public Array<Language> KnownLanguages { get; set; } = new();

    public override IEntityComponent CreateComponent()
    {
        return new LanguagesComponent
        {
            KnownLanguages = this.KnownLanguages
        };
    }
}

public enum Language
{
    // Standard Languages
    Common,
    Dwarvish,
    Elvish,
    Giant,
    Gnomish,
    Goblin,
    Halfling,
    Orc,

    // Exotic Languages
    Abyssal,
    Celestial,
    Draconic,
    DeepSpeech,
    Infernal,
    Primordial,
    Sylvan,
    Undercommon
}

public enum LanguageScript
{
    None,
    Common,
    Dwarvish,
    Elvish,
    Infernal,
    Celestial,
    Draconic
}
