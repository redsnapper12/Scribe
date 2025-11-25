using Godot;
using Scribe.Scripts.Core.Interfaces;
using Scribe.Scripts.Combat;
using Scribe.Scripts.Core.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Scribe.Scripts.Core;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }
    
    [Export] public GridManager GridManager { get; set; }
    
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
        
        // Initialize MovementService with required references
        MovementService.Initialize(this, GridManager);
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
    
    public bool IsCellOccupied(Vector2I gridPosition, Entity excludeEntity = null)
    {
        foreach (var entity in AllEntities)
        {
            if (entity == excludeEntity)
                continue;
            if (!entity.IsAlive)
                continue;
            if (entity.GridPosition == gridPosition)
                return true;
        }
        return false;
    }
}