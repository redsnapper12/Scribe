using Godot;

namespace Scribe.Scripts.UI;

public partial class MainMenuUI : Control
{
    [Export] public Button MultiplayerButton { get; set; }
    [Export] public Button CharactersButton { get; set; }
    [Export] public Button SettingsButton { get; set; }
    [Export] public Button ExitButton { get; set; }

    public override void _Ready()
    {
        MultiplayerButton.Pressed += OnMultiplayerPressed;
        CharactersButton.Pressed += OnViewCharactersPressed;
        SettingsButton.Pressed += OnSettingsPressed;
        ExitButton.Pressed += OnExitPressed;
    }

    private void OnMultiplayerPressed()
    {
        GetTree().ChangeSceneToPacked(GD.Load<PackedScene>("res://scenes/session/session_lobby.tscn"));
    }

    private void OnViewCharactersPressed()
    {
        GetTree().ChangeSceneToPacked(GD.Load<PackedScene>("res://scenes/creation_menus/characters/character_creation_wizard.tscn"));
    }

    private void OnSettingsPressed()
    {
        GD.Print("MainMenuUI: Settings not yet implemented");
    }

    private void OnExitPressed()
    {
        GetTree().Quit();
    }
}
