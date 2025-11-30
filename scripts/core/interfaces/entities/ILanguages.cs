using Godot.Collections;

namespace Scribe.Scripts.Core.Interfaces.Entities;

public interface ILanguages : IEntityComponent
{
    Array<Language> KnownLanguages { get; set; }
    bool Speaks(Language language);
    LanguageScript GetScript(Language language);
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
