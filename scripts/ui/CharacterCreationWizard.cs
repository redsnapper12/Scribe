using Godot;
using System;

public partial class CharacterCreationWizard : Control
{
    [ExportGroup("Buttons")]
    [Export] Button startFromScratchButton = null;
    [Export] Button useATemplateButton = null;
    [Export] Button importCharacterButton = null;

    [ExportGroup("Scenes")]
    [Export] PackedScene startFromScratchScene = null;
    [Export] PackedScene useATemplateScene = null;
    [Export] PackedScene importCharacterScene = null;

    public override void _Ready()
    {
        // Init signals
        startFromScratchButton.Pressed += () => LoadCharacterCreationWizardScene(startFromScratchScene);
        useATemplateButton.Pressed += () => LoadCharacterCreationWizardScene(useATemplateScene);
        importCharacterButton.Pressed += () => LoadCharacterCreationWizardScene(importCharacterScene);
    }

    private void LoadCharacterCreationWizardScene(PackedScene scene)
    {
        GetTree().ChangeSceneToPacked(scene);
    }
}
