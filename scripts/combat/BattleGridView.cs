using Godot;
using Scribe.Scripts.Core;
using Scribe.Scripts.Core.Services;
using Scribe.Scripts.Entities;
using Scribe.Scripts.UI;
using System.Collections.Generic;
using System.Linq;

namespace Scribe.Scripts.Combat;

public partial class BattleGridView : Node2D
{
    #region Exports

    [ExportGroup("External References")]
    [Export] public TileMapLayer GroundLayer { get; set; }
    [Export] public TileMapLayer OverlayLayer { get; set; }

    [ExportGroup("Child References")]
    [Export] public GridManager GridManager { get; set; }
    [Export] public Node2D EntityContainer { get; set; }
    [Export] public MovementOverlay MovementOverlay { get; set; }
    [Export] public GridInteraction GridInteraction { get; set; }
    [Export] public Camera2D Camera { get; set; }

    #endregion
    
    #region Signals
    
    [Signal]
    public delegate void CellClickedEventHandler(Vector2I cell);
    
    [Signal]
    public delegate void CellHoveredEventHandler(Vector2I cell);
    
    [Signal]
    public delegate void EntityClickedEventHandler(string entityName);
    
    [Signal]
    public delegate void MovementCompletedEventHandler(int movementRemaining);

    [Signal]
    public delegate void TargetSelectedEventHandler(Entity target);

    [Signal]
    public delegate void TargetingCancelledEventHandler();

    #endregion
    
    #region State
    
    public Entity SelectedEntity { get; private set; }
    public bool InteractionEnabled => GridInteraction?.Enabled ?? false;
    public bool IsInTargetingMode => _targetingModeActive;

    private Entity _movingEntity;
    private bool _movementModeActive = false;
    private bool _targetingModeActive = false;
    private List<Entity> _validTargets = new();

    #endregion
    
    public override void _Ready()
    {
        if (GridManager != null)
        {
            // Assign layers to GridManager
            GridManager.GroundLayer = GroundLayer;
            GridManager.OverlayLayer = OverlayLayer;

        }

        if (GridInteraction != null)
        {
            GridInteraction.CellHovered += OnCellHovered;
            GridInteraction.CellClicked += OnCellClicked;
            GridInteraction.CellExited += OnCellExited;
        }
    }
    
    #region Public API
    
    public void ShowMovementRange(Entity entity)
    {
        if (entity == null || MovementOverlay == null)
            return;
        
        _movingEntity = entity;
        _movementModeActive = true;
        
        var reachableCells = MovementService.GetReachableCells(entity);
        MovementOverlay.ShowReachableCells(reachableCells);
    }
    
    public void HideMovementRange()
    {
        _movingEntity = null;
        _movementModeActive = false;
        MovementOverlay?.Clear();
    }
    
    public void EnableInteraction()
    {
        GridInteraction?.Enable();
    }
    
    public void DisableInteraction()
    {
        GridInteraction?.Disable();
        MovementOverlay?.ClearPathPreview();
    }
    
    public void SelectEntity(Entity entity)
    {
        SelectedEntity = entity;
    }
    
    public void ClearSelection()
    {
        SelectedEntity = null;
        HideMovementRange();
    }
    
    public void CenterOnCell(Vector2I cell)
    {
        if (Camera == null)
            return;
        
        var worldPos = GridManager.GridToWorld(cell);
        Camera.GlobalPosition = worldPos;
    }
    
    public void CenterOnEntity(Entity entity)
    {
        if (entity == null)
            return;

        CenterOnCell(entity.GridPosition);
    }

    public void EnterTargetingMode(List<Entity> validTargets)
    {
        _targetingModeActive = true;
        _validTargets = validTargets ?? new List<Entity>();
        _movementModeActive = false;

        var targetCells = _validTargets.Select(e => e.GridPosition).ToList();
        MovementOverlay?.ShowTargetableCells(targetCells);
    }

    public void ExitTargetingMode()
    {
        _targetingModeActive = false;
        _validTargets.Clear();
        MovementOverlay?.ClearTargetHighlights();
    }

    #endregion
    
    #region Event Handlers
    
    private void OnCellHovered(Vector2I cell)
    {
        EmitSignal(SignalName.CellHovered, cell);

        // Handle target hover feedback
        if (_targetingModeActive && MovementOverlay != null)
        {
            if (_validTargets.Any(e => e.GridPosition == cell))
            {
                MovementOverlay.SetHoveredTarget(cell);
            }
            else
            {
                MovementOverlay.SetHoveredTarget(null);
            }
            return;
        }

        if (_movementModeActive && _movingEntity != null && MovementOverlay != null)
        {
            if (MovementOverlay.IsCellReachable(cell))
            {
                // Show path preview to this cell
                var path = GridManager.FindPath(
                    _movingEntity.GridPosition,
                    cell,
                    pos => GridManager.IsCellOccupied(pos, GameManager.Instance.AllEntities, _movingEntity)
                );

                MovementOverlay.ShowPathPreview(path);
            }
            else
            {
                MovementOverlay.ClearPathPreview();
            }
        }
    }
    
    private void OnCellClicked(Vector2I cell)
    {
        EmitSignal(SignalName.CellClicked, cell);

        // Handle targeting mode first
        if (_targetingModeActive)
        {
            var targetEntity = _validTargets.FirstOrDefault(e => e.GridPosition == cell);
            if (targetEntity != null)
            {
                EmitSignal(SignalName.TargetSelected, targetEntity);
                EmitSignal(SignalName.EntityClicked, targetEntity.EntityName);
            }
            return;
        }

        if (_movementModeActive && _movingEntity != null && MovementOverlay != null)
        {
            if (MovementOverlay.IsCellReachable(cell))
            {
                ExecuteMovement(cell);
            }
        }
    }
    
    private void OnCellExited()
    {
        MovementOverlay?.ClearPathPreview();
    }
    
    #endregion
    
    #region Movement Execution
    
    private void ExecuteMovement(Vector2I targetCell)
    {
        if (_movingEntity == null)
            return;
        
        var request = new MovementRequest(targetCell, _movingEntity.Controller);
        var result = MovementService.RequestMove(_movingEntity, request);
        
        if (result.Success)
        {
            // Recalculate reachable cells for split movement
            var newReachable = MovementService.GetReachableCells(_movingEntity);
            MovementOverlay.ShowReachableCells(newReachable);
            MovementOverlay.ClearPathPreview();
            
            EmitSignal(SignalName.MovementCompleted, result.MovementRemaining);
        }
    }
    
    #endregion
}