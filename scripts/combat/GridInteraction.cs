using Godot;
using Scribe.Scripts.Core;

namespace Scribe.Scripts.Combat;

public partial class GridInteraction : Node2D
{
    [Export] public bool Enabled { get; set; } = true;
    
    public Vector2I? CurrentHoveredCell { get; private set; }
    
    [Signal]
    public delegate void CellHoveredEventHandler(Vector2I cell);
    
    [Signal]
    public delegate void CellClickedEventHandler(Vector2I cell);
    
    [Signal]
    public delegate void CellExitedEventHandler();
    
    private Vector2I? _lastHoveredCell;
    
    public override void _Input(InputEvent @event)
    {
        if (!Enabled)
            return;
        
        if (@event is InputEventMouseMotion mouseMotion)
        {
            HandleMouseMove(mouseMotion.GlobalPosition);
        }
        else if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
            {
                HandleMouseClick(mouseButton.GlobalPosition);
            }
        }
    }
    
    private void HandleMouseMove(Vector2 globalMousePos)
    {
        var localPos = GetCanvasTransform().AffineInverse() * globalMousePos;
        var gridCell = GridManager.WorldToGrid(localPos);
        
        if (gridCell != _lastHoveredCell)
        {
            _lastHoveredCell = gridCell;
            CurrentHoveredCell = gridCell;
            EmitSignal(SignalName.CellHovered, gridCell);
        }
    }
    
    private void HandleMouseClick(Vector2 globalMousePos)
    {
        var localPos = GetCanvasTransform().AffineInverse() * globalMousePos;
        var gridCell = GridManager.WorldToGrid(localPos);
        
        EmitSignal(SignalName.CellClicked, gridCell);
    }
    
    public void Enable()
    {
        Enabled = true;
    }
    
    public void Disable()
    {
        Enabled = false;
        CurrentHoveredCell = null;
        _lastHoveredCell = null;
        EmitSignal(SignalName.CellExited);
    }
}