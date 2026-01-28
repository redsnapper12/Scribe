using Godot;
using System.Collections.Generic;
using Scribe.Scripts.Combat;
using Scribe.Scripts.Core.Services;
using Scribe.Scripts.Entities;
using Scribe.Scripts.Data.ComponentData;

namespace Scribe.Scripts.Core;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }
    
    [Export] public BattleGridView BattleGridView { get; set; }
    
    public MovementMode CurrentMovementMode { get; private set; } = MovementMode.Exploration;
    public bool IsInCombat => CurrentMovementMode == MovementMode.Combat;
    
    public CombatManager ActiveCombat { get; private set; }
    
    [Signal]
    public delegate void MovementModeChangedEventHandler(int mode);
    
    [Signal]
    public delegate void CombatStartedEventHandler();

    [Signal]
    public delegate void CombatEndedEventHandler();

    public List<Entity> AllEntities { get; } = new();

    private GridManager GridManager => BattleGridView?.GridManager;
    
    public void RegisterEntity(Entity entity)
    {
        if (!AllEntities.Contains(entity))
            AllEntities.Add(entity);
    }

    public void UnregisterEntity(Entity entity)
    {
        AllEntities.Remove(entity);
    }

    public override void _Ready()
    {
        Instance = this;
        
        // Init services
        MovementService.Initialize(this, GridManager);
        DiceRollingService.Initialize();
    }
    
    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }
    
    public void EnterCombat(CombatManager combatManager)
    {
        ActiveCombat = combatManager;
        SetMovementMode(MovementMode.Combat);
        EmitSignal(SignalName.CombatStarted);
    }
    
    public void ExitCombat()
    {
        ActiveCombat = null;
        SetMovementMode(MovementMode.Exploration);
        EmitSignal(SignalName.CombatEnded);
    }
    
    public void SetMovementMode(MovementMode mode)
    {
        if (CurrentMovementMode != mode)
        {
            CurrentMovementMode = mode;
            EmitSignal(SignalName.MovementModeChanged, (int)mode);
        }
    }
    
    /// <summary>
    /// Checks if the given controller has authority to control the entity.
    /// </summary>
    public bool HasAuthority(Entity entity, ControllerType requestedBy)
    {
        // DM can control anything
        if (requestedBy == ControllerType.DM)
            return true;
        
        // Otherwise, controller must match
        return entity.Controller == requestedBy;
    }

    /// <summary>
    /// Checks if it's currently the given entity's turn.
    /// Always returns true if not in combat.
    /// </summary>
    public bool IsEntityTurn(Entity entity)
    {
        if (!IsInCombat || ActiveCombat == null)
            return true;

        return ActiveCombat.GetCurrentTurnEntity() == entity;
    }
}