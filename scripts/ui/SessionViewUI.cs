using Godot;
using Scribe.Scripts.Combat;
using Scribe.Scripts.Core;
using Scribe.Scripts.IO;

namespace Scribe.Scripts.UI;

public partial class SessionViewUI : Node2D
{
    [Export] public BattleGridView BattleGridView { get; set; }
    [Export] public CombatManager CombatManager { get; set; }
    [Export] public Control PlayerTurnUI { get; set; }
    [Export] public Button LoadMapButton { get; set; }
    [Export] public Button StartCombatButton { get; set; }
    [Export] public Button EndSessionButton { get; set; }
    [Export] public Button DebugWalkabilityButton { get; set; }

    private FileDialog _loadDialog;

    public override void _Ready()
    {
        PlayerTurnUI.Visible = false;
        StartCombatButton.Disabled = true;

        LoadMapButton.Pressed += OnLoadMapPressed;
        StartCombatButton.Pressed += OnStartCombatPressed;
        EndSessionButton.Pressed += OnEndSessionPressed;
        DebugWalkabilityButton.Pressed += OnDebugWalkabilityToggled;

        GameManager.Instance.CombatStarted += OnCombatStarted;

        DirAccess.MakeDirRecursiveAbsolute("user://maps");

        _loadDialog = new FileDialog
        {
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Access = FileDialog.AccessEnum.Userdata,
            Filters = ["*.tres ; Scribe Maps"],
            Title = "Load Map",
            CurrentDir = "user://maps",
            Size = new Vector2I(900, 600)
        };
        AddChild(_loadDialog);
        _loadDialog.FileSelected += OnMapFileSelected;
    }

    private void OnLoadMapPressed()
    {
        _loadDialog.PopupCentered();
    }

    private void OnMapFileSelected(string path)
    {
        var mapData = ResourceLoader.Load<DungeonScrawlMapData>(path);
        if (mapData == null)
        {
            GD.PrintErr($"SessionViewUI: '{path}' is not a valid map resource.");
            return;
        }

        MapLoader.Load(mapData, BattleGridView.GridManager, this);
        GameManager.Instance.ActiveMapData = mapData;
        StartCombatButton.Disabled = false;
        GD.Print($"SessionViewUI: Map loaded — {mapData.GridSize.X}×{mapData.GridSize.Y}");
    }

    private void OnStartCombatPressed()
    {
        CombatManager.InitiateSetupAndStart();
    }

    private void OnEndSessionPressed()
    {
        GameManager.Instance.ClearSession();
        GetTree().ChangeSceneToPacked(GD.Load<PackedScene>("res://scenes/main_menu.tscn"));
    }

    private void OnDebugWalkabilityToggled()
    {
        var gm = BattleGridView.GridManager;
        gm.DrawWalkability = !gm.DrawWalkability;
        gm.QueueRedraw();
    }

    private void OnCombatStarted()
    {
        PlayerTurnUI.Visible = true;
        StartCombatButton.Disabled = true;
        LoadMapButton.Disabled = true;
    }
}
