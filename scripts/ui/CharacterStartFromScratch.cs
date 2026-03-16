using Godot;
using Godot.Collections;
using Scribe.Scripts.Core.Services;
using Scribe.Scripts.Data;
using Scribe.Scripts.Data.ComponentData;
using Scribe.Scripts.UI.AbilityScoreGenerators;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class CharacterStartFromScratch : Control
{
    [ExportGroup("Debug")]
    [Export] private bool enableDebug = false;

    [ExportGroup("Control")]
    [Export] private TabContainer _characterCreationStageContainer = null;
    [Export] private Button _globalStageProgressButton = null;
    private int _stageCount = 0;

    [ExportSubgroup("Determine Class")]
    [Export] private ItemList _classItemList = null;
    [Export] private Label _classNameLabel = null;
    [Export] private Label _classDescriptionLabel = null;
    [Export] private Button _selectClassButton = null;

    [ExportSubgroup("Determine Species")]
    [Export] private ItemList _speciesItemList = null;
    [Export] private Label _speciesNameLabel = null;
    [Export] private Label _speciesDescriptionLabel = null;
    [Export] private Button _selectSpeciesButton = null;

    [ExportSubgroup("Determine Background")]
    [Export] private ItemList _backgroundItemList = null;
    [Export] private Label _backgroundNameLabel = null;
    [Export] private Label _backgroundDescriptionLabel = null;
    [Export] private Button _selectBackgroundButton = null;

    [ExportSubgroup("Determine Ability Scores")]
    [Export] private OptionButton _methodDropdown = null;
    [Export] private Button _confirmAbilityScoresButton = null;
    [Export] private Control _standardArrayPanel = null;
    [Export] private Control _pointBuyPanel = null;
    [Export] private Control _rollingPanel = null;

    [ExportSubgroup("Standard Array UI")]
    [Export] private OptionButton _strengthArrayDropdown = null;
    [Export] private OptionButton _dexterityArrayDropdown = null;
    [Export] private OptionButton _constitutionArrayDropdown = null;
    [Export] private OptionButton _intelligenceArrayDropdown = null;
    [Export] private OptionButton _wisdomArrayDropdown = null;
    [Export] private OptionButton _charismaArrayDropdown = null;

    [ExportSubgroup("Point Buy UI")]
    [Export] private Label _pointsRemainingLabel = null;
    [Export] private Label _strengthPointBuyLabel = null;
    [Export] private Label _dexterityPointBuyLabel = null;
    [Export] private Label _constitutionPointBuyLabel = null;
    [Export] private Label _intelligencePointBuyLabel = null;
    [Export] private Label _wisdomPointBuyLabel = null;
    [Export] private Label _charismaPointBuyLabel = null;
    [Export] private Button _strengthIncrementButton = null;
    [Export] private Button _strengthDecrementButton = null;
    [Export] private Button _dexterityIncrementButton = null;
    [Export] private Button _dexterityDecrementButton = null;
    [Export] private Button _constitutionIncrementButton = null;
    [Export] private Button _constitutionDecrementButton = null;
    [Export] private Button _intelligenceIncrementButton = null;
    [Export] private Button _intelligenceDecrementButton = null;
    [Export] private Button _wisdomIncrementButton = null;
    [Export] private Button _wisdomDecrementButton = null;
    [Export] private Button _charismaIncrementButton = null;
    [Export] private Button _charismaDecrementButton = null;

    [ExportSubgroup("Rolling UI")]
    [Export] private Button _rollButton = null;
    [Export] private VBoxContainer _rollResultsContainer = null;
    [Export] private OptionButton _strengthRollDropdown = null;
    [Export] private OptionButton _dexterityRollDropdown = null;
    [Export] private OptionButton _constitutionRollDropdown = null;
    [Export] private OptionButton _intelligenceRollDropdown = null;
    [Export] private OptionButton _wisdomRollDropdown = null;
    [Export] private OptionButton _charismaRollDropdown = null;

    [ExportSubgroup("Determine Details")]
    [Export] private LineEdit _characterNameInput = null;
    [Export] private OptionButton _alignmentDropdown = null;
    [Export] private TextureRect _portraitPreview = null;
    [Export] private Button _selectPortraitButton = null;
    [Export] private FileDialog _portraitFileDialog = null;
    [Export] private ItemList _languagesList = null;
    [Export] private ItemList _skillProficiencyList = null;
    [Export] private Button _finalizeCharacterButton = null;

    [ExportSubgroup("Character Preview")]
    [Export] private Label _previewClassLabel = null;
    [Export] private Label _previewSpeciesLabel = null;
    [Export] private Label _previewBackgroundLabel = null;
    [Export] private Label _previewStrengthLabel = null;
    [Export] private Label _previewDexterityLabel = null;
    [Export] private Label _previewConstitutionLabel = null;
    [Export] private Label _previewIntelligenceLabel = null;
    [Export] private Label _previewWisdomLabel = null;
    [Export] private Label _previewCharismaLabel = null;
    [Export] private Label _previewHitPointsLabel = null;
    [Export] private Label _previewArmorClassLabel = null;
    [Export] private Label _previewSpeedLabel = null;
    [Export] private Label _previewSizeLabel = null;

    [ExportGroup("Class Data")]
    [Export] private Array<ClassComponentData> _classes = null;

    [ExportGroup("Species Data")]
    [Export] private Array<SpeciesComponentData> _species = null;

    [ExportGroup("Background Data")]
    [Export] private Array<BackgroundComponentData> _backgrounds = null;

    private ClassComponentData _selectedClass = null;
    private SpeciesComponentData _selectedSpecies = null;
    private BackgroundComponentData _selectedBackground = null;
    private AbilityScoresComponentData _selectedAbilityScores = null;

    private string _selectedCharacterName = "";
    private EntityAlignment _selectedAlignment = EntityAlignment.TrueNeutral;
    private Texture2D _selectedPortrait = null;
    private Array<Language> _selectedLanguages = [];
    private Array<Skill> _selectedSkillProficiencies = [];

    private AbilityScoreGenerator _abilityScoreGenerator;
    private AbilityScoreGenerationMethod _selectedMethod;
    private System.Collections.Generic.Dictionary<AbilityScore, int> _abilityScores = new();

    public override void _Ready()
    {
        _stageCount = _characterCreationStageContainer.GetTabCount();

        InitSignals();

        InitClassSelect();
        InitSpeciesSelect();
        InitBackgroundSelect();
        InitAbilityScoreMethodSelect();
        InitDetails();
    }

    private void ProgessStage()
    {
        if(_characterCreationStageContainer.CurrentTab < _stageCount - 1)
        {
            _characterCreationStageContainer.CurrentTab++;
        }
        else
        {
            _characterCreationStageContainer.CurrentTab = 0;
        }
    }

    private void InitSignals()
    {
        _globalStageProgressButton.Pressed += ProgessStage;
        _selectClassButton.Pressed += SelectClass;
        _selectSpeciesButton.Pressed += SelectSpecies;
        _selectBackgroundButton.Pressed += SelectBackground;
        _confirmAbilityScoresButton.Pressed += ConfirmAbilityScores;
        _finalizeCharacterButton.Pressed += FinalizeDetails;
    }

    #region Determine Class

    private void InitClassSelect()
    {
        foreach (ClassComponentData classData in _classes)
        {
            _classItemList.AddItem(classData.ClassName, classData.Icon);
        }

        ClassComponentData firstClass = _classes[0];

        _classNameLabel.Text = firstClass.ClassName;
        _classDescriptionLabel.Text = firstClass.ClassDescription;
        _selectedClass = firstClass;

        _classItemList.ItemSelected += (idx) => UpdateClassInfoBlock((int)idx);
    }

    private void UpdateClassInfoBlock(int index)
    {
        ClassComponentData classData = _classes[index];

        _classNameLabel.Text = classData.ClassName;
        _classDescriptionLabel.Text = classData.ClassDescription;
    }

    private void SelectClass()
    {
        if(_classItemList.GetSelectedItems().Length <= 0) return;

        int classArrayIdx = _classItemList.GetSelectedItems()[0];
        _selectedClass = _classes[classArrayIdx];

        ProgessStage();

        if(enableDebug) GD.Print($"Selected Class Name: {_selectedClass.ClassName}");
    }

    #endregion

    #region Determine Species

    private void InitSpeciesSelect()
    {
        foreach (SpeciesComponentData speciesData in _species)
        {
            _speciesItemList.AddItem(speciesData.SpeciesName, speciesData.Icon);
        }

        SpeciesComponentData firstSpecies = _species[0];

        _speciesNameLabel.Text = firstSpecies.SpeciesName;
        _speciesDescriptionLabel.Text = firstSpecies.SpeciesDescription;
        _selectedSpecies = firstSpecies;

        _speciesItemList.ItemSelected += (idx) => UpdateSpeciesInfoBlock((int)idx);
    }

    private void UpdateSpeciesInfoBlock(int index)
    {
        SpeciesComponentData speciesData = _species[index];

        _speciesNameLabel.Text = speciesData.SpeciesName;
        _speciesDescriptionLabel.Text = speciesData.SpeciesDescription;
    }

    private void SelectSpecies()
    {
        if(_speciesItemList.GetSelectedItems().Length <= 0) return;

        int speciesArrayIdx = _speciesItemList.GetSelectedItems()[0];
        _selectedSpecies = _species[speciesArrayIdx];

        ProgessStage();

        if(enableDebug) GD.Print($"Selected Class Name: {_selectedSpecies.SpeciesName}");
    }

    #endregion

    #region Determine Background

    private void InitBackgroundSelect()
    {
        foreach (BackgroundComponentData backgroundData in _backgrounds)
        {
            _backgroundItemList.AddItem(backgroundData.BackgroundName);
        }

        BackgroundComponentData firstBackground = _backgrounds[0];

        _backgroundNameLabel.Text = firstBackground.BackgroundName;
        _backgroundDescriptionLabel.Text = firstBackground.BackgroundDescription;
        _selectedBackground = firstBackground;

        _backgroundItemList.ItemSelected += (idx) => UpdateBackgroundInfoBlock((int)idx);
    }

    private void UpdateBackgroundInfoBlock(int index)
    {
        BackgroundComponentData backgroundData = _backgrounds[index];

        _backgroundNameLabel.Text = backgroundData.BackgroundName;
        _backgroundDescriptionLabel.Text = backgroundData.BackgroundDescription;
    }

    private void SelectBackground()
    {
        if(_backgroundItemList.GetSelectedItems().Length <= 0) return;

        int backgroundArrayIdx = _backgroundItemList.GetSelectedItems()[0];
        _selectedBackground = _backgrounds[backgroundArrayIdx];

        ProgessStage();

        if(enableDebug) GD.Print($"Selected Background Name: {_selectedBackground.BackgroundName}");
    }

    #endregion

    #region Ability Score Generation

    private void InitAbilityScoreMethodSelect()
    {
        foreach (AbilityScoreGenerationMethod method in Enum.GetValues<AbilityScoreGenerationMethod>())
        {
            string displayName = AbilityScoreGenerator.GetDisplayName(method);
            _methodDropdown.AddItem(displayName);
        }

        _methodDropdown.ItemSelected += OnMethodSelected;

        // Select Standard Array by default and initialize its UI
        _methodDropdown.Selected = 0;
        OnMethodSelected(0);
    }

    private void HideAllAbilityScorePanels()
    {
        if (_standardArrayPanel != null) _standardArrayPanel.Visible = false;
        if (_pointBuyPanel != null) _pointBuyPanel.Visible = false;
        if (_rollingPanel != null) _rollingPanel.Visible = false;
    }

    private void OnMethodSelected(long index)
    {
        _selectedMethod = (AbilityScoreGenerationMethod)index;

        _abilityScoreGenerator = AbilityScoreGenerator.Create(_selectedMethod);
        _abilityScoreGenerator.AssignmentsChanged += UpdateAbilityScoreUI;

        HideAllAbilityScorePanels();

        switch (_selectedMethod)
        {
            case AbilityScoreGenerationMethod.StandardArray:
                ShowStandardArrayUI();
                break;
            case AbilityScoreGenerationMethod.PointBuy:
                ShowPointBuyUI();
                break;
            case AbilityScoreGenerationMethod.Rolling:
                ShowRollingUI();
                break;
        }
    }

    private void UpdateAbilityScoreUI()
    {
        switch (_selectedMethod)
        {
            case AbilityScoreGenerationMethod.StandardArray:
                UpdateStandardArrayDropdowns();
                break;
            case AbilityScoreGenerationMethod.PointBuy:
                UpdatePointBuyUI();
                break;
            case AbilityScoreGenerationMethod.Rolling:
                UpdateRollingDropdowns();
                break;
        }
    }

    #endregion

    #region Determine Details

    private void InitDetails()
    {
        InitAlignmentDropdown();
        InitPortraitSelection();
        InitLanguagesList();
        InitSkillProficiencyList();

        _characterNameInput.TextChanged += OnCharacterNameChanged;
        _alignmentDropdown.ItemSelected += OnAlignmentSelected;
    }

    private void InitAlignmentDropdown()
    {
        foreach (string entityAlignment in Enum.GetNames<EntityAlignment>())
        {
            var alignmentName = string.Concat(entityAlignment.Select(c => char.IsUpper(c) ? " " + c : c.ToString())).TrimStart(' ');

            _alignmentDropdown.AddItem(alignmentName);
        }

        _alignmentDropdown.Selected = (int)EntityAlignment.TrueNeutral;
    }

    private void InitPortraitSelection()
    {
        if (_selectPortraitButton == null || _portraitFileDialog == null) return;

        _selectPortraitButton.Pressed += OnSelectPortraitPressed;
        _portraitFileDialog.FileSelected += OnPortraitFileSelected;

        _portraitFileDialog.Filters = ["*.png", "*.jpg", "*.jpeg", "*.webp"];
        _portraitFileDialog.Access = FileDialog.AccessEnum.Filesystem;
    }

    private void InitLanguagesList()
    {
        if (_languagesList == null) return;

        _languagesList.SelectMode = ItemList.SelectModeEnum.Multi;

        foreach (Language language in Enum.GetValues<Language>())
        {
            var languageName = string.Concat(language.ToString().Select(c => char.IsUpper(c) ? " " + c : c.ToString())).TrimStart(' ');
            _languagesList.AddItem(languageName);
        }

        _languagesList.MultiSelected += OnLanguageMultiSelected;
    }

    private void InitSkillProficiencyList()
    {
        if (_skillProficiencyList == null) return;

        _skillProficiencyList.SelectMode = ItemList.SelectModeEnum.Multi;

        foreach (Skill skill in Enum.GetValues<Skill>())
        {
            var skillName = string.Concat(skill.ToString().Select(c => char.IsUpper(c) ? " " + c : c.ToString())).TrimStart(' ');
            _skillProficiencyList.AddItem(skillName);
        }

        _skillProficiencyList.MultiSelected += OnSkillProficiencyMultiSelected;
    }

    private void UpdateCharacterPreview()
    {
        // Class, Species, Background
        if (_previewClassLabel != null)
            _previewClassLabel.Text = _selectedClass?.ClassName ?? "Not Selected";

        if (_previewSpeciesLabel != null)
            _previewSpeciesLabel.Text = _selectedSpecies?.SpeciesName ?? "Not Selected";

        if (_previewBackgroundLabel != null)
            _previewBackgroundLabel.Text = _selectedBackground?.BackgroundName ?? "Not Selected";

        // Ability Scores
        if (_selectedAbilityScores != null)
        {
            if (_previewStrengthLabel != null)
                _previewStrengthLabel.Text = FormatAbilityScore(_selectedAbilityScores.Strength);

            if (_previewDexterityLabel != null)
                _previewDexterityLabel.Text = FormatAbilityScore(_selectedAbilityScores.Dexterity);

            if (_previewConstitutionLabel != null)
                _previewConstitutionLabel.Text = FormatAbilityScore(_selectedAbilityScores.Constitution);

            if (_previewIntelligenceLabel != null)
                _previewIntelligenceLabel.Text = FormatAbilityScore(_selectedAbilityScores.Intelligence);

            if (_previewWisdomLabel != null)
                _previewWisdomLabel.Text = FormatAbilityScore(_selectedAbilityScores.Wisdom);

            if (_previewCharismaLabel != null)
                _previewCharismaLabel.Text = FormatAbilityScore(_selectedAbilityScores.Charisma);

            // Derived Stats
            UpdateDerivedStatsPreview();
        }
    }

    private string FormatAbilityScore(int score)
    {
        int modifier = (score - 10) / 2;
        string sign = modifier >= 0 ? "+" : "";
        return $"{score} ({sign}{modifier})";
    }

    private void UpdateDerivedStatsPreview()
    {
        if (_selectedClass == null || _selectedSpecies == null || _selectedAbilityScores == null)
            return;

        // Hit Points
        int hitDieMax = _selectedClass.HitPointDie switch
        {
            DieType.D6 => 6,
            DieType.D8 => 8,
            DieType.D10 => 10,
            DieType.D12 => 12,
            _ => 8
        };
        int constitutionModifier = (_selectedAbilityScores.Constitution - 10) / 2;
        int hitPoints = hitDieMax + constitutionModifier;

        if (_previewHitPointsLabel != null)
            _previewHitPointsLabel.Text = hitPoints.ToString();

        // Armor Class (base 10 + DEX modifier)
        int dexterityModifier = (_selectedAbilityScores.Dexterity - 10) / 2;
        int armorClass = 10 + dexterityModifier;

        if (_previewArmorClassLabel != null)
            _previewArmorClassLabel.Text = armorClass.ToString();

        // Speed
        if (_previewSpeedLabel != null)
            _previewSpeedLabel.Text = $"{_selectedSpecies.Speed} ft.";

        // Size
        if (_previewSizeLabel != null)
            _previewSizeLabel.Text = _selectedSpecies.Size.ToString();
    }

    private void OnCharacterNameChanged(string newName)
    {
        _selectedCharacterName = newName;

        if (enableDebug) GD.Print($"Character Name Changed: {_selectedCharacterName}");
    }

    private void OnAlignmentSelected(long index)
    {
        _selectedAlignment = (EntityAlignment)index;

        if (enableDebug) GD.Print($"Alignment Selected: {_selectedAlignment}");
    }

    private void OnSelectPortraitPressed()
    {
        _portraitFileDialog.Popup();
    }

    private void OnPortraitFileSelected(string path)
    {
        _selectedPortrait = GD.Load<Texture2D>(path);

        if (_portraitPreview != null && _selectedPortrait != null)
        {
            _portraitPreview.Texture = _selectedPortrait;
        }

        if (enableDebug) GD.Print($"Portrait Selected: {path}");
    }

    private void OnLanguageMultiSelected(long index, bool selected)
    {
        var language = (Language)index;

        if (selected && !_selectedLanguages.Contains(language))
        {
            _selectedLanguages.Add(language);
        }
        else if (!selected && _selectedLanguages.Contains(language))
        {
            _selectedLanguages.Remove(language);
        }

        if (enableDebug) GD.Print($"Language {language}: {(selected ? "Added" : "Removed")}");
    }

    private void OnSkillProficiencyMultiSelected(long index, bool selected)
    {
        var skill = (Skill)index;

        if (selected && !_selectedSkillProficiencies.Contains(skill))
        {
            _selectedSkillProficiencies.Add(skill);
        }
        else if (!selected && _selectedSkillProficiencies.Contains(skill))
        {
            _selectedSkillProficiencies.Remove(skill);
        }

        if (enableDebug) GD.Print($"Skill Proficiency {skill}: {(selected ? "Added" : "Removed")}");
    }

    private bool ValidateDetails()
    {
        if (string.IsNullOrWhiteSpace(_selectedCharacterName))
        {
            GD.PrintErr("Character name is required!");
            return false;
        }

        if (_selectedClass == null)
        {
            GD.PrintErr("No class selected!");
            return false;
        }

        if (_selectedSpecies == null)
        {
            GD.PrintErr("No species selected!");
            return false;
        }

        if (_selectedBackground == null)
        {
            GD.PrintErr("No background selected!");
            return false;
        }

        if (_selectedAbilityScores == null)
        {
            GD.PrintErr("Ability scores not confirmed!");
            return false;
        }

        return true;
    }

    private void FinalizeDetails()
    {
        if (!ValidateDetails()) return;

        var characterData = CreateCharacterData();

        if (enableDebug)
        {
            GD.Print("=== Character Created ===");
            GD.Print($"  Name: {characterData.EntityName}");
            GD.Print($"  Alignment: {characterData.Alignment}");
            GD.Print($"  Size: {characterData.Size}");
            GD.Print($"  Type: {characterData.Type}");
            GD.Print($"  Languages: {string.Join(", ", _selectedLanguages)}");
            GD.Print($"  Skill Proficiencies: {string.Join(", ", _selectedSkillProficiencies)}");
        }

        SaveCharacter(characterData);
    }

    private void SaveCharacter(CharacterData characterData)
    {
        string sanitizedName = SanitizeFileName(characterData.EntityName);
        string savePath = $"res://resources/testing/characters/{sanitizedName}.tres";

        // Ensure the directory exists
        var dir = DirAccess.Open("res://resources/testing");
        if (dir == null)
        {
            DirAccess.MakeDirRecursiveAbsolute("res://resources/testing/characters");
        }
        else if (!dir.DirExists("characters"))
        {
            dir.MakeDir("characters");
        }

        var error = ResourceSaver.Save(characterData, savePath);

        if (error == Error.Ok)
        {
            GD.Print($"Character saved successfully to: {savePath}");
        }
        else
        {
            GD.PrintErr($"Failed to save character: {error}");
        }
    }

    private static string SanitizeFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "unnamed_character";

        // Replace spaces with underscores and remove invalid characters
        var sanitized = name.ToLower().Replace(" ", "_");
        var invalidChars = System.IO.Path.GetInvalidFileNameChars();

        foreach (var c in invalidChars)
        {
            sanitized = sanitized.Replace(c.ToString(), "");
        }

        return sanitized;
    }

    private CharacterData CreateCharacterData()
    {
        var characterData = new CharacterData
        {
            EntityName = _selectedCharacterName,
            Alignment = _selectedAlignment,
            Controller = ControllerType.Player,
            Size = _selectedSpecies.Size,
            Type = _selectedSpecies.CreatureType,
            Icon = _selectedPortrait,

            // Character origin
            ClassComponentData = _selectedClass,
            SpeciesComponentData = _selectedSpecies,
            BackgroundComponentData = _selectedBackground,

            // Required components
            AbilityScoresComponentData = _selectedAbilityScores,
            HealthComponentData = CreateHealthComponentData(),
            MovementComponentData = CreateMovementComponentData(),
            AttackComponentData = new AttackComponentData(),
            EquipmentComponentData = new EquipmentComponentData(),
            LanguagesComponentData = CreateLanguagesComponentData(),
            CharacterSkillsComponentData = CreateSkillsComponentData()
        };

        return characterData;
    }

    private HealthComponentData CreateHealthComponentData()
    {
        int hitDieMax = _selectedClass.HitPointDie switch
        {
            DieType.D6 => 6,
            DieType.D8 => 8,
            DieType.D10 => 10,
            DieType.D12 => 12,
            _ => 8
        };

        int constitutionModifier = (_selectedAbilityScores.Constitution - 10) / 2;

        return new HealthComponentData
        {
            MaxHP = hitDieMax + constitutionModifier,
            ArmorClass = 10 + ((_selectedAbilityScores.Dexterity - 10) / 2)
        };
    }

    private MovementComponentData CreateMovementComponentData()
    {
        return new MovementComponentData
        {
            WalkSpeed = _selectedSpecies.Speed
        };
    }

    private LanguagesComponentData CreateLanguagesComponentData()
    {
        var languages = new Array<Language>();

        foreach (var language in _selectedLanguages)
        {
            languages.Add(language);
        }

        if (!languages.Contains(Language.Common))
        {
            languages.Add(Language.Common);
        }

        return new LanguagesComponentData
        {
            KnownLanguages = languages
        };
    }

    private CharacterSkillsComponentData CreateSkillsComponentData()
    {
        var proficiencies = new Array<Skill>();

        foreach (var skill in _selectedBackground.Proficiencies)
        {
            if (!proficiencies.Contains(skill))
            {
                proficiencies.Add(skill);
            }
        }

        foreach (var skill in _selectedSkillProficiencies)
        {
            if (!proficiencies.Contains(skill))
            {
                proficiencies.Add(skill);
            }
        }

        return new CharacterSkillsComponentData
        {
            Proficiencies = proficiencies
        };
    }

    #endregion

    #region Standard Array

    private void ShowStandardArrayUI()
    {
        if (_standardArrayPanel != null) _standardArrayPanel.Visible = true;

        _strengthArrayDropdown.ItemSelected += (idx) => OnArrayDropdownSelected(AbilityScore.Strength, idx);
        _dexterityArrayDropdown.ItemSelected += (idx) => OnArrayDropdownSelected(AbilityScore.Dexterity, idx);
        _constitutionArrayDropdown.ItemSelected += (idx) => OnArrayDropdownSelected(AbilityScore.Constitution, idx);
        _intelligenceArrayDropdown.ItemSelected += (idx) => OnArrayDropdownSelected(AbilityScore.Intelligence, idx);
        _wisdomArrayDropdown.ItemSelected += (idx) => OnArrayDropdownSelected(AbilityScore.Wisdom, idx);
        _charismaArrayDropdown.ItemSelected += (idx) => OnArrayDropdownSelected(AbilityScore.Charisma, idx);

        UpdateStandardArrayDropdowns();
    }

    private void UpdateStandardArrayDropdowns()
    {
        var generator = (StandardArrayGenerator)_abilityScoreGenerator;
        UpdateSingleArrayDropdown(_strengthArrayDropdown, AbilityScore.Strength, generator);
        UpdateSingleArrayDropdown(_dexterityArrayDropdown, AbilityScore.Dexterity, generator);
        UpdateSingleArrayDropdown(_constitutionArrayDropdown, AbilityScore.Constitution, generator);
        UpdateSingleArrayDropdown(_intelligenceArrayDropdown, AbilityScore.Intelligence, generator);
        UpdateSingleArrayDropdown(_wisdomArrayDropdown, AbilityScore.Wisdom, generator);
        UpdateSingleArrayDropdown(_charismaArrayDropdown, AbilityScore.Charisma, generator);
    }

    private void UpdateSingleArrayDropdown(OptionButton dropdown, AbilityScore ability, StandardArrayGenerator generator)
    {
        var available = generator.GetAvailableScores();
        var currentScore = generator.GetScore(ability);

        dropdown.Clear();
        dropdown.AddItem("--", 0);

        int selectedIndex = 0;
        for (int i = 0; i < available.Count; i++)
        {
            dropdown.AddItem(available[i].ToString(), i + 1);
        }

        if (currentScore.HasValue)
        {
            dropdown.AddItem(currentScore.Value.ToString(), available.Count + 1);
            selectedIndex = available.Count + 1;
        }

        dropdown.Selected = selectedIndex;
    }

    private void OnArrayDropdownSelected(AbilityScore ability, long index)
    {
        if (index == 0)
        {
            ((StandardArrayGenerator)_abilityScoreGenerator).UnassignScore(ability);
            return;
        }

        var generator = (StandardArrayGenerator)_abilityScoreGenerator;
        var available = generator.GetAvailableScores();

        if (index - 1 < available.Count)
        {
            int selectedScore = available[(int)(index - 1)];
            generator.AssignScore(ability, selectedScore);
        }
    }

    #endregion

    #region Point Buy

    private void ShowPointBuyUI()
    {
        if (_pointBuyPanel != null) _pointBuyPanel.Visible = true;

        var generator = (PointBuyGenerator)_abilityScoreGenerator;
        generator.PointsRemainingChanged += UpdatePointsRemainingLabel;

        _strengthIncrementButton.Pressed += () => OnIncrementAbility(AbilityScore.Strength);
        _strengthDecrementButton.Pressed += () => OnDecrementAbility(AbilityScore.Strength);
        _dexterityIncrementButton.Pressed += () => OnIncrementAbility(AbilityScore.Dexterity);
        _dexterityDecrementButton.Pressed += () => OnDecrementAbility(AbilityScore.Dexterity);
        _constitutionIncrementButton.Pressed += () => OnIncrementAbility(AbilityScore.Constitution);
        _constitutionDecrementButton.Pressed += () => OnDecrementAbility(AbilityScore.Constitution);
        _intelligenceIncrementButton.Pressed += () => OnIncrementAbility(AbilityScore.Intelligence);
        _intelligenceDecrementButton.Pressed += () => OnDecrementAbility(AbilityScore.Intelligence);
        _wisdomIncrementButton.Pressed += () => OnIncrementAbility(AbilityScore.Wisdom);
        _wisdomDecrementButton.Pressed += () => OnDecrementAbility(AbilityScore.Wisdom);
        _charismaIncrementButton.Pressed += () => OnIncrementAbility(AbilityScore.Charisma);
        _charismaDecrementButton.Pressed += () => OnDecrementAbility(AbilityScore.Charisma);

        UpdatePointBuyUI();
    }

    private void OnIncrementAbility(AbilityScore ability)
    {
        ((PointBuyGenerator)_abilityScoreGenerator).IncrementScore(ability);
    }

    private void OnDecrementAbility(AbilityScore ability)
    {
        ((PointBuyGenerator)_abilityScoreGenerator).DecrementScore(ability);
    }

    private void UpdatePointBuyUI()
    {
        var generator = (PointBuyGenerator)_abilityScoreGenerator;
        var assignments = generator.GetAssignments();

        _strengthPointBuyLabel.Text = assignments[AbilityScore.Strength].ToString();
        _dexterityPointBuyLabel.Text = assignments[AbilityScore.Dexterity].ToString();
        _constitutionPointBuyLabel.Text = assignments[AbilityScore.Constitution].ToString();
        _intelligencePointBuyLabel.Text = assignments[AbilityScore.Intelligence].ToString();
        _wisdomPointBuyLabel.Text = assignments[AbilityScore.Wisdom].ToString();
        _charismaPointBuyLabel.Text = assignments[AbilityScore.Charisma].ToString();

        UpdatePointsRemainingLabel(generator.GetPointsRemaining());
    }

    private void UpdatePointsRemainingLabel(int pointsRemaining)
    {
        _pointsRemainingLabel.Text = $"Points Remaining: {pointsRemaining}";
    }

    #endregion

    #region Rolling

    private void ShowRollingUI()
    {
        if (_rollingPanel != null) _rollingPanel.Visible = true;

        _rollButton.Pressed += OnRollButtonPressed;

        _strengthRollDropdown.ItemSelected += (idx) => OnRollDropdownSelected(AbilityScore.Strength, idx);
        _dexterityRollDropdown.ItemSelected += (idx) => OnRollDropdownSelected(AbilityScore.Dexterity, idx);
        _constitutionRollDropdown.ItemSelected += (idx) => OnRollDropdownSelected(AbilityScore.Constitution, idx);
        _intelligenceRollDropdown.ItemSelected += (idx) => OnRollDropdownSelected(AbilityScore.Intelligence, idx);
        _wisdomRollDropdown.ItemSelected += (idx) => OnRollDropdownSelected(AbilityScore.Wisdom, idx);
        _charismaRollDropdown.ItemSelected += (idx) => OnRollDropdownSelected(AbilityScore.Charisma, idx);
    }

    private void OnRollButtonPressed()
    {
        var generator = (RollingGenerator)_abilityScoreGenerator;
        var results = generator.RollCompleteSet();

        var scores = results.Select(r => r.score).ToList();
        generator.SetRolledScores(scores);

        foreach (var child in _rollResultsContainer.GetChildren())
        {
            child.QueueFree();
        }

        foreach (var result in results)
        {
            var label = new Label();
            var diceString = string.Join(", ", result.dice);
            label.Text = $"Rolled: [{diceString}] = {result.score}";
            _rollResultsContainer.AddChild(label);
        }

        UpdateRollingDropdowns();
    }

    private void UpdateRollingDropdowns()
    {
        var generator = (RollingGenerator)_abilityScoreGenerator;
        UpdateSingleRollDropdown(_strengthRollDropdown, AbilityScore.Strength, generator);
        UpdateSingleRollDropdown(_dexterityRollDropdown, AbilityScore.Dexterity, generator);
        UpdateSingleRollDropdown(_constitutionRollDropdown, AbilityScore.Constitution, generator);
        UpdateSingleRollDropdown(_intelligenceRollDropdown, AbilityScore.Intelligence, generator);
        UpdateSingleRollDropdown(_wisdomRollDropdown, AbilityScore.Wisdom, generator);
        UpdateSingleRollDropdown(_charismaRollDropdown, AbilityScore.Charisma, generator);
    }

    private void UpdateSingleRollDropdown(OptionButton dropdown, AbilityScore ability, RollingGenerator generator)
    {
        var available = generator.GetAvailableScores();
        var currentScore = generator.GetScore(ability);

        dropdown.Clear();
        dropdown.AddItem("--", 0);

        int selectedIndex = 0;
        for (int i = 0; i < available.Count; i++)
        {
            dropdown.AddItem(available[i].ToString(), i + 1);
        }

        if (currentScore.HasValue)
        {
            dropdown.AddItem(currentScore.Value.ToString(), available.Count + 1);
            selectedIndex = available.Count + 1;
        }

        dropdown.Selected = selectedIndex;
    }

    private void OnRollDropdownSelected(AbilityScore ability, long index)
    {
        if (index == 0)
        {
            ((RollingGenerator)_abilityScoreGenerator).UnassignScore(ability);
            return;
        }

        var generator = (RollingGenerator)_abilityScoreGenerator;
        var available = generator.GetAvailableScores();

        if (index - 1 < available.Count)
        {
            int selectedScore = available[(int)(index - 1)];
            generator.AssignScore(ability, selectedScore);
        }
    }

    #endregion

    #region Finalize Ability Scores

    private void ConfirmAbilityScores()
    {
        if (_abilityScoreGenerator == null || !_abilityScoreGenerator.IsComplete())
        {
            GD.PrintErr("Not all ability scores have been assigned!");
            return;
        }

        _abilityScores = _abilityScoreGenerator.GetAssignments();

        _selectedAbilityScores = new AbilityScoresComponentData
        {
            Strength = _abilityScores[AbilityScore.Strength],
            Dexterity = _abilityScores[AbilityScore.Dexterity],
            Constitution = _abilityScores[AbilityScore.Constitution],
            Intelligence = _abilityScores[AbilityScore.Intelligence],
            Wisdom = _abilityScores[AbilityScore.Wisdom],
            Charisma = _abilityScores[AbilityScore.Charisma]
        };

        ProgessStage();
        UpdateCharacterPreview();

        if (enableDebug)
        {
            GD.Print("Ability Scores:");
            foreach (var kvp in _abilityScores)
            {
                GD.Print($"  {kvp.Key}: {kvp.Value}");
            }
        }
    }

    #endregion
}
