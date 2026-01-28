using Godot;
using Scribe.Scripts.Combat;
using Scribe.Scripts.Core;
using Scribe.Scripts.Entities;
using Scribe.Scripts.Data.ComponentData;

namespace Scribe.Scripts.UI;

public partial class PlayerTurnUI : Control
{
    [Export] public CombatManager CombatManager { get; set; }
    [Export] public BattleGridView BattleGridView { get; set; }
    
    [ExportGroup("UI Elements")]
    [Export] public Label TurnLabel { get; set; }
    [Export] public Label MovementLabel { get; set; }
    [Export] public Button AttackButton { get; set; }
    [Export] public Button EndTurnButton { get; set; }
    [Export] public VBoxContainer TargetContainer { get; set; }
    
    private bool _selectingTarget = false;
    private Entity _currentEntity;
    
    public override void _Ready()
    {
        Visible = false;
        
        if (AttackButton != null)
            AttackButton.Pressed += OnAttackPressed;
        
        if (EndTurnButton != null)
            EndTurnButton.Pressed += OnEndTurnPressed;
        
        if (CombatManager != null)
        {
            CombatManager.TurnChanged += OnTurnChanged;
        }
        
        if (BattleGridView != null)
        {
            BattleGridView.MovementCompleted += OnMovementCompleted;
        }
    }
    
    private void OnTurnChanged(Entity entity)
    {
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
    
    private void OnMovementCompleted(int movementRemaining)
    {
        UpdateMovementLabel();
    }
    
    private void OnAttackPressed()
    {
        _selectingTarget = true;
        ShowTargetSelection();
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
        
        // Update movement display after attack (in case we add attack-of-opportunity movement cost later)
        UpdateMovementLabel();
    }
}