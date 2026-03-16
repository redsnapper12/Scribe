using Godot;
using Scribe.Scripts.Combat;
using Scribe.Scripts.Combat.Actions;
using Scribe.Scripts.Core;
using Scribe.Scripts.Core.Services;
using Scribe.Scripts.Entities;
using Scribe.Scripts.Data.ComponentData;
using System.Collections.Generic;

namespace Scribe.Scripts.UI;

public partial class PlayerTurnUI : Control
{
    [Export] public CombatManager CombatManager { get; set; }
    [Export] public BattleGridView BattleGridView { get; set; }
    
    [ExportGroup("UI Elements")]
    [Export] public Label TurnLabel { get; set; }
    [Export] public Label MovementLabel { get; set; }
    [Export] public Label ActionLabel { get; set; }
    [Export] public Label BonusActionLabel { get; set; }
    [Export] public Label ReactionLabel { get; set; }
    [Export] public Button AttackButton { get; set; }
    [Export] public Button DashButton { get; set; }
    [Export] public Button EndTurnButton { get; set; }
    [Export] public VBoxContainer TargetContainer { get; set; }

    // Action instances
    private readonly DashAction _dashAction = new();
    
    private bool _selectingTarget = false;
    private Entity _currentEntity;
    
    public override void _Ready()
    {
        Visible = false;
        
        if (AttackButton != null)
            AttackButton.Pressed += OnAttackPressed;

        if (DashButton != null)
            DashButton.Pressed += OnDashPressed;

        if (EndTurnButton != null)
            EndTurnButton.Pressed += OnEndTurnPressed;
        
        if (CombatManager != null)
        {
            CombatManager.TurnChanged += OnTurnChanged;
        }
        
        if (BattleGridView != null)
        {
            BattleGridView.MovementCompleted += OnMovementCompleted;
            BattleGridView.TargetSelected += OnGridTargetSelected;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (_selectingTarget && @event is InputEventKey keyEvent)
        {
            if (keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
            {
                ExitTargetingMode();
                GetViewport().SetInputAsHandled();
            }
        }
    }
    
    private void OnTurnChanged(Entity entity)
    {
        // Disconnect from previous entity's signals
        if (_currentEntity != null && _currentEntity.TryGetComponent<ActionEconomyComponent>(out var prevActionEconomy))
        {
            prevActionEconomy.ActionSpent -= OnActionSpent;
            prevActionEconomy.TurnReset -= OnTurnReset;
        }

        if (entity.Controller == Core.Services.ControllerType.AI)
        {
            Visible = false;
            BattleGridView?.HideMovementRange();
            BattleGridView?.DisableInteraction();
            _currentEntity = null;
        }
        else
        {
            ShowPlayerTurn(entity);
        }
    }
    
    private void ShowPlayerTurn(Entity player)
    {
        _currentEntity = player;
        Visible = true;

        if (TurnLabel != null)
            TurnLabel.Text = $"{player.EntityName}'s Turn";

        UpdateMovementLabel();
        UpdateActionDisplay();

        // Subscribe to action economy changes
        if (player.TryGetComponent<ActionEconomyComponent>(out var actionEconomy))
        {
            actionEconomy.ActionSpent += OnActionSpent;
            actionEconomy.TurnReset += OnTurnReset;
        }

        _selectingTarget = false;

        if (TargetContainer != null)
            TargetContainer.Visible = false;

        // Enable movement on the grid
        if (BattleGridView != null)
        {
            BattleGridView.SelectEntity(player);
            BattleGridView.ShowMovementRange(player);
            BattleGridView.EnableInteraction();
        }
    }
    
    private void UpdateMovementLabel()
    {
        if (MovementLabel != null && _currentEntity != null)
        {
            if (_currentEntity.TryGetComponent<MovementComponent>(out var movement))
            {
                MovementLabel.Text = $"Movement: {movement.MovementRemaining}/{movement.WalkSpeed} ft";
            }
        }
    }

    private void UpdateActionDisplay()
    {
        if (_currentEntity == null) return;

        if (_currentEntity.TryGetComponent<ActionEconomyComponent>(out var actionEconomy))
        {
            if (ActionLabel != null)
                ActionLabel.Text = $"Action: {actionEconomy.ActionsRemaining}/{actionEconomy.TotalActions}";

            if (BonusActionLabel != null)
                BonusActionLabel.Text = $"Bonus Action: {actionEconomy.BonusActionsRemaining}/{actionEconomy.TotalBonusActions}";

            if (ReactionLabel != null)
                ReactionLabel.Text = $"Reaction: {actionEconomy.ReactionsRemaining}/{actionEconomy.TotalReactions}";

            // Disable Attack button if no action available
            if (AttackButton != null)
            {
                AttackButton.Disabled = !actionEconomy.CanAttack();
            }

            // Disable Dash button if no action available
            if (DashButton != null)
            {
                DashButton.Disabled = !actionEconomy.CanAfford(ActionCost.Action);
            }
        }
    }

    private void OnActionSpent(ActionCost actionType)
    {
        UpdateActionDisplay();
    }

    private void OnTurnReset()
    {
        UpdateActionDisplay();
    }
    
    private void OnMovementCompleted(int movementRemaining)
    {
        UpdateMovementLabel();
    }
    
    private void OnAttackPressed()
    {
        // Check action economy before entering targeting mode
        if (_currentEntity != null && _currentEntity.TryGetComponent<ActionEconomyComponent>(out var actionEconomy))
        {
            if (!actionEconomy.CanAttack())
            {
                GD.Print("No action available to attack");
                return;
            }
        }

        var validTargets = GetAttackableTargets();

        if (validTargets.Count == 0)
        {
            GD.Print("No valid targets in range");
            return;
        }

        _selectingTarget = true;

        if (TargetContainer != null)
            TargetContainer.Visible = false;

        BattleGridView?.EnterTargetingMode(validTargets);
    }

    private void OnDashPressed()
    {
        if (_currentEntity == null) return;

        var battleContext = CombatManager?.BattleContext;
        if (battleContext == null) return;

        // Execute the Dash action
        _dashAction.Execute(_currentEntity, battleContext);

        // Update UI
        UpdateMovementLabel();
        UpdateActionDisplay();

        // Refresh movement range display
        if (BattleGridView != null)
        {
            BattleGridView.ShowMovementRange(_currentEntity);
        }
    }

    private void OnEndTurnPressed()
    {
        BattleGridView?.HideMovementRange();
        BattleGridView?.DisableInteraction();
        CombatManager?.PlayerEndTurn();
    }
    
    private void ShowTargetSelection()
    {
        if (TargetContainer == null) return;

        foreach (var child in TargetContainer.GetChildren())
        {
            child.QueueFree();
        }

        var currentEntity = CombatManager?.GetCurrentTurnEntity();
        if (currentEntity == null) return;

        var allCombatants = CombatManager.GetAllCombatants();

        // Access BattleContext to check attack validity (range + wall blocking)
        var battleContext = CombatManager?.BattleContext;

        foreach (var entity in allCombatants)
        {
            if (entity == currentEntity)
                continue;

            if (!entity.TryGetComponent<HealthComponent>(out var health) || !health.IsAlive)
                continue;

            // Only show targets that can be attacked (range + no wall blocking)
            if (battleContext != null && !battleContext.CanMeleeAttack(currentEntity, entity))
                continue;

            var targetButton = new Button
            {
                Text = $"{entity.EntityName} (HP: {health.CurrentHP}/{health.MaxHP})"
            };

            var capturedEntity = entity;
            targetButton.Pressed += () => OnTargetSelected(capturedEntity);

            TargetContainer.AddChild(targetButton);
        }

        TargetContainer.Visible = true;
    }
    
    private void OnTargetSelected(Entity target)
    {
        CombatManager?.PlayerAttack(target);

        if (TargetContainer != null)
            TargetContainer.Visible = false;

        _selectingTarget = false;

        UpdateMovementLabel();
        UpdateActionDisplay();

        if (BattleGridView != null && _currentEntity != null)
        {
            BattleGridView.ShowMovementRange(_currentEntity);
        }
    }

    private void OnGridTargetSelected(Entity target)
    {
        if (!_selectingTarget) return;
        if (_currentEntity == null) return;

        var battleContext = CombatManager?.BattleContext;

        // If already in melee range, attack directly
        if (battleContext != null && battleContext.CanMeleeAttack(_currentEntity, target))
        {
            CombatManager?.PlayerAttack(target);
            ExitTargetingMode();
            return;
        }

        // Need to move first - find adjacent cell
        var adjacentCell = FindAdjacentCell(_currentEntity, target);
        if (adjacentCell.HasValue)
        {
            var request = new MovementRequest(adjacentCell.Value, _currentEntity.Controller);
            var result = MovementService.RequestMove(_currentEntity, request);

            if (result.Success)
            {
                UpdateMovementLabel();

                // Check if we're now in range to attack
                if (battleContext != null && battleContext.CanMeleeAttack(_currentEntity, target))
                {
                    CombatManager?.PlayerAttack(target);
                }
            }
        }

        ExitTargetingMode();
    }

    private void ExitTargetingMode()
    {
        _selectingTarget = false;
        BattleGridView?.ExitTargetingMode();

        if (BattleGridView != null && _currentEntity != null)
        {
            BattleGridView.ShowMovementRange(_currentEntity);
        }

        UpdateMovementLabel();
        UpdateActionDisplay();
    }

    private List<Entity> GetAttackableTargets()
    {
        var validTargets = new List<Entity>();
        var currentEntity = CombatManager?.GetCurrentTurnEntity();
        if (currentEntity == null) return validTargets;

        var allCombatants = CombatManager.GetAllCombatants();
        var battleContext = CombatManager?.BattleContext;
        var reachableCells = MovementService.GetReachableCells(currentEntity);
        var reachableSet = new HashSet<Vector2I>(reachableCells);

        foreach (var entity in allCombatants)
        {
            if (entity == currentEntity)
                continue;

            if (!entity.TryGetComponent<HealthComponent>(out var health) || !health.IsAlive)
                continue;

            // Already in melee range
            if (battleContext != null && battleContext.CanMeleeAttack(currentEntity, entity))
            {
                validTargets.Add(entity);
                continue;
            }

            // Check if any adjacent cell to the enemy is reachable
            if (CanReachAdjacentCell(currentEntity, entity, reachableSet))
            {
                validTargets.Add(entity);
            }
        }

        return validTargets;
    }

    private bool CanReachAdjacentCell(Entity self, Entity target, HashSet<Vector2I> reachableCells)
    {
        var gridManager = BattleGridView?.GridManager;
        if (gridManager == null) return false;

        var targetPos = target.GridPosition;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                var candidate = new Vector2I(targetPos.X + dx, targetPos.Y + dy);

                if (!reachableCells.Contains(candidate))
                    continue;

                if (!gridManager.IsValidGridPosition(candidate))
                    continue;

                if (!gridManager.IsWalkable(candidate))
                    continue;

                if (gridManager.IsCellOccupied(candidate, GameManager.Instance.AllEntities, self))
                    continue;

                // Can we attack the target from this cell?
                if (!gridManager.CanAttackAcross(candidate, targetPos))
                    continue;

                return true;
            }
        }

        return false;
    }

    private Vector2I? FindAdjacentCell(Entity self, Entity target)
    {
        var gridManager = BattleGridView?.GridManager;
        if (gridManager == null) return null;

        var targetPos = target.GridPosition;
        var selfPos = self.GridPosition;
        Vector2I? bestCell = null;
        int shortestPathLength = int.MaxValue;
        float bestDirectionScore = float.MaxValue;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                var candidate = new Vector2I(targetPos.X + dx, targetPos.Y + dy);

                if (!gridManager.IsValidGridPosition(candidate))
                    continue;

                if (!gridManager.IsWalkable(candidate))
                    continue;

                if (gridManager.IsCellOccupied(candidate, GameManager.Instance.AllEntities, self))
                    continue;

                // Can we attack the target from this cell?
                if (!gridManager.CanAttackAcross(candidate, targetPos))
                    continue;

                var path = gridManager.FindPath(
                    selfPos,
                    candidate,
                    pos => gridManager.IsCellOccupied(pos, GameManager.Instance.AllEntities, self)
                );

                if (path != null && path.Count > 0)
                {
                    float directionScore = Mathf.Sqrt(
                        Mathf.Pow(candidate.X - selfPos.X, 2) +
                        Mathf.Pow(candidate.Y - selfPos.Y, 2)
                    );

                    if (path.Count < shortestPathLength ||
                        (path.Count == shortestPathLength && directionScore < bestDirectionScore))
                    {
                        shortestPathLength = path.Count;
                        bestDirectionScore = directionScore;
                        bestCell = candidate;
                    }
                }
            }
        }

        return bestCell;
    }
}