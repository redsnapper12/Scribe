using Godot;
using Scribe.Scripts.Core.Entities;
using Scribe.Scripts.Combat;
using System.Collections.Generic;

namespace Scribe.Scripts.UI;

public partial class PlayerTurnUI : Control
{
    [Export] public CombatManager CombatManager { get; set; }
    [Export] public Label TurnLabel { get; set; }
    [Export] public Button AttackButton { get; set; }
    [Export] public Button EndTurnButton { get; set; }
    [Export] public VBoxContainer TargetContainer { get; set; }
    
    private bool _selectingTarget = false;
    
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
    }
    
    private void OnTurnChanged(Entity entity)
    {
        if (entity.IsAI)
        {
            Visible = false;
        }
        else
        {
            ShowPlayerTurn(entity);
        }
    }
    
    private void ShowPlayerTurn(Entity player)
    {
        Visible = true;
        
        if (TurnLabel != null)
            TurnLabel.Text = $"{player.Name}'s Turn";
        
        _selectingTarget = false;
        
        if (TargetContainer != null)
            TargetContainer.Visible = false;
    }
    
    private void OnAttackPressed()
    {
        _selectingTarget = true;
        ShowTargetSelection();
    }
    
    private void OnEndTurnPressed()
    {
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
        
        foreach (var entity in allCombatants)
        {
            if (entity != currentEntity && entity.IsAlive)
            {
                var targetButton = new Button
                {
                    Text = $"{entity.Name} (HP: {entity.CurrentHP}/{entity.MaxHP})"
                };
                
                var capturedEntity = entity;
                targetButton.Pressed += () => OnTargetSelected(capturedEntity);
                
                TargetContainer.AddChild(targetButton);
            }
        }
        
        TargetContainer.Visible = true;
    }
    
    private void OnTargetSelected(Entity target)
    {
        CombatManager?.PlayerAttack(target);
        
        if (TargetContainer != null)
            TargetContainer.Visible = false;
        
        _selectingTarget = false;
    }
}