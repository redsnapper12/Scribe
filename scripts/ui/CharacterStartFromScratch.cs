using Godot;
using Godot.Collections;
using Scribe.Scripts.Data.ComponentData;
using System;
using System.Diagnostics;
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

    [ExportGroup("Class Data")]
    [Export] private Array<ClassComponentData> _classes = null;

    [ExportGroup("Species Data")]
    [Export] private Array<SpeciesComponentData> _species = null;

    [ExportGroup("Background Data")]
    [Export] private Array<BackgroundComponentData> _backgrounds = null;

    private ClassComponentData _selectedClass = null;
    private SpeciesComponentData _selectedSpecies = null;
    private BackgroundComponentData _selectedBackground = null;


    // Determine Ability Scores
    private Dictionary<AbilityScore, int> abilityScores = [];

    public override void _Ready()
    {
        _stageCount = _characterCreationStageContainer.GetTabCount();

        InitSignals();

        InitClassSelect();
        InitSpeciesSelect();
        InitBackgroundSelect();
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
        // Debug
        _globalStageProgressButton.Pressed += ProgessStage;

        // Determine Class
        _selectClassButton.Pressed += SelectClass;

        // Determine Species
        _selectSpeciesButton.Pressed += SelectSpecies;

        // Determine Background
        _selectBackgroundButton.Pressed += SelectBackground;
    }

    #region Determine Class

    private void InitClassSelect()
    {
        // Init class items
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

        // Using a magic number here is fine because our class list doesn't allow multi-select yet.
        int classArrayIdx = _classItemList.GetSelectedItems()[0];
        _selectedClass = _classes[classArrayIdx];

        ProgessStage();

        if(enableDebug) GD.Print($"Selected Class Name: {_selectedClass.ClassName}");
    }

    #endregion

    #region Determine Species

    private void InitSpeciesSelect()
    {
        // Init class items
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
        // Using a magic number here is fine because our class list doesn't allow multi-select yet.
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
        // Init class items
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
        // Using a magic number here is fine because our class list doesn't allow multi-select yet.
        if(_backgroundItemList.GetSelectedItems().Length <= 0) return;

        int backgroundArrayIdx = _backgroundItemList.GetSelectedItems()[0];
        _selectedBackground = _backgrounds[backgroundArrayIdx];

        ProgessStage();

        if(enableDebug) GD.Print($"Selected Background Name: {_selectedBackground.BackgroundName}");
    }

    #endregion
}
