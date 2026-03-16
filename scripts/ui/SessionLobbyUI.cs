using Godot;
using System.Collections.Generic;
using Scribe.Scripts.Core;

namespace Scribe.Scripts.UI;

public partial class SessionLobbyUI : Control
{
    [Export] public Label SlotLabel1 { get; set; }
    [Export] public Label SlotLabel2 { get; set; }
    [Export] public Label SlotLabel3 { get; set; }
    [Export] public Label SlotLabel4 { get; set; }

    [Export] public Button PickButton1 { get; set; }
    [Export] public Button PickButton2 { get; set; }
    [Export] public Button PickButton3 { get; set; }
    [Export] public Button PickButton4 { get; set; }

    [Export] public Button StartSessionButton { get; set; }

    private readonly CharacterData[] _slots = new CharacterData[4];
    private FileDialog _fileDialog;
    private int _activeSlot = -1;

    public override void _Ready()
    {
        PickButton1.Pressed += () => OpenPicker(0);
        PickButton2.Pressed += () => OpenPicker(1);
        PickButton3.Pressed += () => OpenPicker(2);
        PickButton4.Pressed += () => OpenPicker(3);
        StartSessionButton.Pressed += OnStartSessionPressed;
        StartSessionButton.Disabled = true;

        _fileDialog = new FileDialog
        {
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Access = FileDialog.AccessEnum.Resources,
            Filters = ["*.tres ; Character Resources"],
            Title = "Pick Character",
            CurrentDir = "res://resources/entities/characters/",
            Size = new Vector2I(900, 600)
        };
        AddChild(_fileDialog);
        _fileDialog.FileSelected += OnFileSelected;
    }

    private void OpenPicker(int slot)
    {
        _activeSlot = slot;
        _fileDialog.PopupCentered();
    }

    private void OnFileSelected(string path)
    {
        var charData = ResourceLoader.Load<CharacterData>(path);
        if (charData == null)
        {
            GD.PrintErr($"SessionLobbyUI: '{path}' is not a valid CharacterData resource.");
            return;
        }

        _slots[_activeSlot] = charData;

        var className = charData.ClassComponentData?.ClassName ?? "Unknown";
        GetSlotLabel(_activeSlot).Text = $"{charData.EntityName}\n{className}";

        RefreshStartButton();
    }

    private void RefreshStartButton()
    {
        foreach (var slot in _slots)
        {
            if (slot != null)
            {
                StartSessionButton.Disabled = false;
                return;
            }
        }
        StartSessionButton.Disabled = true;
    }

    private void OnStartSessionPressed()
    {
        var characters = new List<CharacterData>();
        foreach (var slot in _slots)
            if (slot != null) characters.Add(slot);

        GameManager.StageSessionCharacters(characters);
        GetTree().ChangeSceneToPacked(GD.Load<PackedScene>("res://scenes/session/session_view.tscn"));
    }

    private Label GetSlotLabel(int index) => index switch
    {
        0 => SlotLabel1,
        1 => SlotLabel2,
        2 => SlotLabel3,
        3 => SlotLabel4,
        _ => null
    };
}
