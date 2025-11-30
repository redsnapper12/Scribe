using Godot;
using Godot.Collections;
using Scribe.Scripts.Core.Interfaces;
using Scribe.Scripts.Core.Interfaces.Entities;

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

public partial class LanguagesComponent : RefCounted, IEntityComponent, ILanguages
{
    public Array<Language> KnownLanguages { get; set; } = new();

    public bool Speaks(Language language)
    {
        return KnownLanguages.Contains(language);
    }

    public LanguageScript GetScript(Language language)
    {
        return language switch
        {
            // Standard Languages
            Language.Common => LanguageScript.Common,
            Language.Dwarvish => LanguageScript.Dwarvish,
            Language.Elvish => LanguageScript.Elvish,
            Language.Giant => LanguageScript.Dwarvish,
            Language.Gnomish => LanguageScript.Dwarvish,
            Language.Goblin => LanguageScript.Dwarvish,
            Language.Halfling => LanguageScript.Common,
            Language.Orc => LanguageScript.Dwarvish,

            // Exotic Languages
            Language.Abyssal => LanguageScript.Infernal,
            Language.Celestial => LanguageScript.Celestial,
            Language.Draconic => LanguageScript.Draconic,
            Language.DeepSpeech => LanguageScript.None,
            Language.Infernal => LanguageScript.Infernal,
            Language.Primordial => LanguageScript.Dwarvish,
            Language.Sylvan => LanguageScript.Elvish,
            Language.Undercommon => LanguageScript.Elvish,

            _ => LanguageScript.None
        };
    }
}
