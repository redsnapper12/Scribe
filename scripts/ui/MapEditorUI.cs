using Godot;
using Scribe.Scripts.IO;
using System.IO;

namespace Scribe.Scripts.UI;

/// <summary>
/// Controller for the map editor scene.
/// Wires the Import Map menu button and runs the DS import pipeline when a .ds file is chosen.
/// Adds a background Sprite2D and MapEditorOverlay as dynamic children of the scene root.
/// </summary>
public partial class MapEditorUI : Node2D
{
	[Export] public MenuButton ImportButton { get; set; }
	[Export] public Button LoadMapButton { get; set; }
	[Export] public Button SaveButton { get; set; }
	[Export] public Button AlignButton { get; set; }
	[Export] public Button PaintButton { get; set; }
	[Export] public Button EraseButton { get; set; }
	[Export] public Button ZoomInButton { get; set; }
	[Export] public Button ZoomOutButton { get; set; }
	[Export] public Button ZoomResetButton { get; set; }
	[Export] public SpinBox ZoomSpinBox { get; set; }
	[Export] public Camera2D Camera { get; set; }

	private enum Tool { None, AlignGrid, Paint, Erase }
	private Tool _activeTool = Tool.None;

	// Drag state for AlignGrid tool
	private bool _isDragging = false;
	private Vector2 _dragStart = Vector2.Zero;
	private Vector2 _offsetAtDragStart = Vector2.Zero;

	private const string SessionPath = "user://editor_states/map_editor/.editor_session.tres";

	private FileDialog _fileDialog;
	private FileDialog _importScribeDialog;
	private FileDialog _loadDialog;
	private FileDialog _saveDialog;
	private Sprite2D _background;
	private MapEditorOverlay _overlay;
	private string _dsFilePath = "";
	private string _backgroundImagePath = "";

	public override void _Ready()
	{
		// Wire Import Map menu button popup
		ImportButton.GetPopup().IndexPressed += OnImportMenuIndexPressed;

		// Wire tool buttons — grouped so only one stays visually pressed
		var toolGroup = new ButtonGroup();
		foreach (var btn in new BaseButton[] { AlignButton, PaintButton, EraseButton })
		{
			btn.ToggleMode = true;
			btn.ButtonGroup = toolGroup;
		}
		AlignButton.Pressed += () => SetActiveTool(Tool.AlignGrid);
		PaintButton.Pressed += () => SetActiveTool(Tool.Paint);
		EraseButton.Pressed += () => SetActiveTool(Tool.Erase);

		// Wire zoom controls
		ZoomInButton.Pressed += () => SetZoom(Camera.Zoom.X + 0.1f);
		ZoomOutButton.Pressed += () => SetZoom(Camera.Zoom.X - 0.1f);
		ZoomResetButton.Pressed += () => SetZoom(1f);
		ZoomSpinBox.ValueChanged += v => SetZoom((float)v / 100f);

		// Import FileDialog
		_fileDialog = new FileDialog
		{
			FileMode = FileDialog.FileModeEnum.OpenFile,
			Access = FileDialog.AccessEnum.Filesystem,
			Filters = ["*.ds ; Dungeon Scrawl Maps"],
			Title = "Import Dungeon Scrawl Map",
			Size = new Vector2I(900, 600),
			UseNativeDialog = true
		};
		AddChild(_fileDialog);
		_fileDialog.FileSelected += OnDsFileSelected;

		// Import Scribe Map FileDialog — browse entire filesystem
		_importScribeDialog = new FileDialog
		{
			FileMode = FileDialog.FileModeEnum.OpenFile,
			Access = FileDialog.AccessEnum.Filesystem,
			Filters = ["*.tscn ; Scribe Maps"],
			Title = "Import Scribe Map",
			Size = new Vector2I(900, 600),
			UseNativeDialog = true
		};
		AddChild(_importScribeDialog);
		_importScribeDialog.FileSelected += OnScribeMapFileSelected;

		// Ensure required directories exist
		DirAccess.MakeDirRecursiveAbsolute("user://maps");
		DirAccess.MakeDirRecursiveAbsolute("user://editor_states/map_editor");

		// Load Map FileDialog — restricted to user://maps/
		_loadDialog = new FileDialog
		{
			FileMode = FileDialog.FileModeEnum.OpenFile,
			Access = FileDialog.AccessEnum.Userdata,
			Filters = ["*.tscn ; Scribe Maps"],
			Title = "Load Map",
			CurrentDir = "user://maps",
			Size = new Vector2I(900, 600),
		};
		AddChild(_loadDialog);
		_loadDialog.FileSelected += OnScribeMapFileSelected;

		LoadMapButton.Pressed += () => _loadDialog.PopupCentered();
		

		// Save FileDialog — restricted to user://maps/
		_saveDialog = new FileDialog
		{
			FileMode = FileDialog.FileModeEnum.SaveFile,
			Access = FileDialog.AccessEnum.Userdata,
			Filters = ["*.tres ; Godot Resources"],
			Title = "Save Map Resource",
			CurrentDir = "user://maps",
			Size = new Vector2I(900, 600),
			UseNativeDialog = true
		};
		AddChild(_saveDialog);
		_saveDialog.FileSelected += OnSaveFileSelected;

		// Save button — disabled until a map is loaded
		SaveButton.Pressed += () => _saveDialog.PopupCentered();

		// Persist session on close
		TreeExiting += SaveSession;

		RestoreSession();
	}

	// ── Signal handlers ────────────────────────────────────────────────────────

	private void OnImportMenuIndexPressed(long index)
	{
		switch (index)
		{
			case 0: _importScribeDialog.PopupCentered(); break;
			case 1: _fileDialog.PopupCentered();         break;
			default: return;
		}
	}

	private void OnScribeMapFileSelected(string path)
	{
		DungeonScrawlMapData mapData;
		try
		{
			mapData = ResourceLoader.Load<DungeonScrawlMapData>(path);
		}
		catch (System.Exception e)
		{
			GD.PrintErr($"[MapEditor] Failed to load Scribe map '{path}': {e.Message}");
			return;
		}

		if (mapData == null)
		{
			GD.PrintErr($"[MapEditor] '{path}' is not a valid Scribe map resource.");
			return;
		}

		GD.Print($"[MapEditor] Loaded Scribe map {mapData.GridSize.X}×{mapData.GridSize.Y} from: {path}");

		// Restore background PNG stored in the resource
		EnsureBackground();
		if (!string.IsNullOrEmpty(mapData.BackgroundImagePath) &&
		    File.Exists(mapData.BackgroundImagePath))
		{
			_backgroundImagePath = mapData.BackgroundImagePath;
			var img = Image.LoadFromFile(mapData.BackgroundImagePath);
			_background.Texture = ImageTexture.CreateFromImage(img);
			EnsureOverlay();
			_overlay.ImageSize = new Vector2(img.GetWidth(), img.GetHeight());
		}
		else
		{
			_background.Texture = null;
			EnsureOverlay();
			_overlay.ImageSize = Vector2.Zero;
		}

		_dsFilePath = "";
		EnsureOverlay();
		_overlay.MapData    = mapData;
		_overlay.GridOffset = mapData.GridOffset;

		SaveButton.Disabled = false;
	}

	private void OnDsFileSelected(string path)
	{
		// 1 — Parse
		DsMapData rawData;
		try
		{
			rawData = DungeonScrawlParser.Parse(path);
		}
		catch (System.Exception e)
		{
			GD.PrintErr($"[MapEditor] Failed to parse '{path}': {e.Message}");
			return;
		}

		_dsFilePath = path;

		// 2 — Convert
		var mapData = DungeonScrawlConverter.Convert(rawData);
		GD.Print($"[MapEditor] Loaded {mapData.GridSize.X}×{mapData.GridSize.Y} map from: {path}");

		// 3 — Background PNG (same base name, .png extension, same directory)
		string pngPath = Path.ChangeExtension(path, ".png");
		EnsureBackground();
		if (File.Exists(pngPath))
		{
			_backgroundImagePath = pngPath;
			var img = Image.LoadFromFile(pngPath);
			_background.Texture = ImageTexture.CreateFromImage(img);
			EnsureOverlay();
			_overlay.ImageSize = new Vector2(img.GetWidth(), img.GetHeight());
		}
		else
		{
			GD.PrintErr($"[MapEditor] No background PNG at '{pngPath}' — overlay only.");
			_background.Texture = null;
		}

		// 4 — Grid overlay
		EnsureOverlay();
		_overlay.MapData = mapData;
		_overlay.GridOffset = Vector2.Zero; // user can tune in a later session

		SaveButton.Disabled = false;
	}

	private void OnSaveFileSelected(string path)
	{
		if (_overlay?.MapData == null) return;

		_overlay.MapData.GridOffset = _overlay.GridOffset;
		_overlay.MapData.BackgroundImagePath = _backgroundImagePath;

		var err = ResourceSaver.Save(_overlay.MapData, path);
		if (err == Error.Ok)
			GD.Print($"[MapEditor] Saved map resource to '{path}'.");
		else
			GD.PrintErr($"[MapEditor] Failed to save map resource to '{path}': {err}");
	}

	// ── Input handling ─────────────────────────────────────────────────────────

	public override void _UnhandledInput(InputEvent @event)
	{
		if (_overlay?.MapData == null) return;

		switch (_activeTool)
		{
			case Tool.AlignGrid: HandleAlignGridInput(@event); break;
			case Tool.Paint:
			case Tool.Erase:     HandlePaintInput(@event);     break;
		}
	}

	private void HandleAlignGridInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
		{
			if (mb.Pressed)
			{
				_isDragging = true;
				_dragStart = GetGlobalMousePosition();
				_offsetAtDragStart = _overlay.GridOffset;
			}
			else
			{
				_isDragging = false;
			}
			GetViewport().SetInputAsHandled();
		}
		else if (@event is InputEventMouseMotion && _isDragging)
		{
			var raw = _offsetAtDragStart + (GetGlobalMousePosition() - _dragStart);
			var snapped = new Vector2(
				Mathf.Round(raw.X / 64f) * 64f,
				Mathf.Round(raw.Y / 64f) * 64f
			);
			_overlay.GridOffset = ClampGridOffset(snapped);
			GetViewport().SetInputAsHandled();
		}
	}

	private void HandlePaintInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
		{
			_isDragging = mb.Pressed;
			if (mb.Pressed) PaintCellAtMouse();
			GetViewport().SetInputAsHandled();
		}
		else if (@event is InputEventMouseMotion && _isDragging)
		{
			PaintCellAtMouse();
			GetViewport().SetInputAsHandled();
		}
	}

	private void PaintCellAtMouse()
	{
		var local = GetGlobalMousePosition() - _overlay.GridOffset;
		var cell = new Vector2I(Mathf.FloorToInt(local.X / 64f), Mathf.FloorToInt(local.Y / 64f));
		if (!_overlay.MapData.InBounds(cell)) return;
		_overlay.MapData.SetWalkable(cell, _activeTool == Tool.Paint);
		_overlay.QueueRedraw();
	}

	private void SetActiveTool(Tool tool)
	{
		_activeTool = tool;
		_isDragging = false;
	}

	private void SetZoom(float zoom)
	{
		float clamped = Mathf.Clamp(zoom, 0.5f, 1.5f);
		Camera.Zoom = new Vector2(clamped, clamped);
		ZoomSpinBox.SetValueNoSignal(Mathf.RoundToInt(clamped * 100f));
	}

	private Vector2 ClampGridOffset(Vector2 offset)
	{
		if (_overlay?.MapData == null || _overlay.ImageSize == Vector2.Zero)
			return offset;

		// GridPadding * 2 non-walkable cells are appended to the right/bottom of
		// the grid data. Allow those cells to extend beyond the image edge so the
		// walkable content can reach the PNG boundary.
		float trailingPx = DungeonScrawlConverter.GridPadding * 2 * 64f;
		float maxX = _overlay.ImageSize.X - _overlay.MapData.Width * 64f + trailingPx;
		float maxY = _overlay.ImageSize.Y - _overlay.MapData.Height * 64f + trailingPx;

		return new Vector2(
			maxX >= 0 ? Mathf.Clamp(offset.X, 0f, maxX) : offset.X,
			maxY >= 0 ? Mathf.Clamp(offset.Y, 0f, maxY) : offset.Y
		);
	}

	// ── Lazy node creation ─────────────────────────────────────────────────────

	private void EnsureBackground()
	{
		if (_background != null) return;
		_background = new Sprite2D { Name = "MapBackground", Centered = false };
		AddChild(_background);
	}

	private void EnsureOverlay()
	{
		if (_overlay != null) return;
		_overlay = new MapEditorOverlay { Name = "MapOverlay" };
		AddChild(_overlay);
	}

	// ── Session persistence ────────────────────────────────────────────────────

	private void SaveSession()
	{
		if (_overlay?.MapData == null) return;

		_overlay.MapData.GridOffset = _overlay.GridOffset;

		var session = new MapEditorSessionData
		{
			DsFilePath          = _dsFilePath,
			BackgroundImagePath = _backgroundImagePath,
			MapData             = _overlay.MapData,
			CameraPosition      = Camera.Position,
			CameraZoom          = Camera.Zoom.X,
		};

		var err = ResourceSaver.Save(session, SessionPath);
		if (err != Error.Ok)
			GD.PrintErr($"[MapEditor] Failed to save session: {err}");
	}

	private void RestoreSession()
	{
		if (!ResourceLoader.Exists(SessionPath)) return;

		var session = ResourceLoader.Load<MapEditorSessionData>(SessionPath);
		if (session?.MapData == null) return;

		// Restore background PNG
		if (!string.IsNullOrEmpty(session.BackgroundImagePath) &&
		    File.Exists(session.BackgroundImagePath))
		{
			var img = Image.LoadFromFile(session.BackgroundImagePath);
			EnsureBackground();
			_background.Texture = ImageTexture.CreateFromImage(img);
			EnsureOverlay();
			_overlay.ImageSize = new Vector2(img.GetWidth(), img.GetHeight());
		}
		else
		{
			EnsureOverlay();
		}

		// Restore map data and grid offset
		_dsFilePath          = session.DsFilePath;
		_backgroundImagePath = session.BackgroundImagePath;
		_overlay.MapData     = session.MapData;
		_overlay.GridOffset  = session.MapData.GridOffset;

		// Restore camera
		Camera.Position = session.CameraPosition;
		SetZoom(session.CameraZoom);

		SaveButton.Disabled = false;
	}
}
