using Godot;
using System.Collections.Generic;
using System.Linq;
using Scribe.Scripts.AI;
using Scribe.Scripts.Core;
using Scribe.Scripts.Core.Interfaces;
using Scribe.Scripts.Data;
using Scribe.Scripts.Entities;

namespace Scribe.Scripts.Combat;

public partial class CombatManager : Node
{
    private List<Entity> _combatants = new();
    private Dictionary<Entity, int> _initiativeRolls = new();
    private int _currentTurnIndex = 0;
    private bool _combatActive = false;
    private BattleContext _battleContext;
    private bool _setupComplete = false;

    public BattleContext BattleContext => _battleContext;
    
    [Export] public BattleGridView BattleGridView { get; set; }
    [Export] public GameManager GameManager { get; set; }
    [Export] public EntityData PlayerData { get; set; }
    [Export] public EntityData GoblinData { get; set; }
    
    // Convenience properties to access through BattleGridView
    private GridManager GridManager => BattleGridView?.GridManager;
    private TileMapLayer BattleGrid => BattleGridView?.GroundLayer;
    
    [Signal]
    public delegate void CombatStartedEventHandler();
    
    [Signal]
    public delegate void CombatEndedEventHandler();
    
    [Signal]
    public delegate void TurnChangedEventHandler(Entity entity);
    
    [Signal]
    public delegate void PlayerTurnStartedEventHandler();
    
    public override void _Ready()
    {
        if (!ValidateSetup())
        {
            GD.PrintErr("CombatManager: Failed validation");
            return;
        }
        
        _battleContext = new BattleContext
        {
            BattleGrid = BattleGrid,
            GridManager = GridManager
        };
    }
    
    public override void _Process(double delta)
    {
        if (!_setupComplete)
        {
            _setupComplete = true;
            SetProcessMode(ProcessModeEnum.Disabled);
            SetupCombat();
        }
    }
    
    private bool ValidateSetup()
    {
        bool valid = true;
        
        if (BattleGridView == null)
        {
            GD.PrintErr("CombatManager: BattleGridView is not assigned!");
            valid = false;
        }
        
        if (GameManager == null)
        {
            GD.PrintErr("CombatManager: GameManager is not assigned!");
            valid = false;
        }
    
        
        return valid;
    }
    
    private void SetupCombat()
    {
        var player = PlayerData.CreateEntity(new Vector2I(3, 3));
        var goblin = GoblinData.CreateEntity(new Vector2I(9, 3));
        
        // Register entities with GameManager
        GameManager?.RegisterEntity(player);
        GameManager?.RegisterEntity(goblin);
        
        // Create EntityNodes and add to BattleGridView
        var playerNode = CreateEntityNode(player);
        var goblinNode = CreateEntityNode(goblin);
        
        if (BattleGridView?.EntityContainer != null)
        {
            BattleGridView.EntityContainer.AddChild(playerNode);
            BattleGridView.EntityContainer.AddChild(goblinNode);
        }
        
        _combatants = new List<Entity> { player, goblin };
        
        CallDeferred(nameof(StartCombatDeferred));
    }
    
    private EntityNode CreateEntityNode(Entity entity)
    {
        var scene = GD.Load<PackedScene>("res://scenes/entities/entity_node.tscn");
        var node = scene.Instantiate<EntityNode>();
        node.Entity = entity;
        return node;
    }
    
    private void StartCombatDeferred()
    {
        if (_combatants == null || _combatants.Count == 0)
        {
            GD.PrintErr("No combatants to start combat!");
            return;
        }
        
        StartCombat(_combatants);
    }
    
    private void StartCombat(List<Entity> combatants)
    {
        _combatants = combatants.Where(e => e.IsAlive).ToList();
        _battleContext.AllEntities = _combatants;
        
        if (_combatants.Count == 0)
        {
            GD.PrintErr("Cannot start combat with no combatants");
            return;
        }
        
        // Notify GameManager we're in combat
        GameManager?.EnterCombat(this);
        
        RollInitiative();
        
        _currentTurnIndex = 0;
        _combatActive = true;
        
        EmitSignal(SignalName.CombatStarted);
        GD.Print("=== COMBAT START ===");
        PrintInitiativeOrder();
        
        ProcessAllAITurnsUntilPlayer();
    }
    
    public void EndCombat()
    {
        _combatActive = false;
        _initiativeRolls.Clear();
        
        // Notify GameManager combat ended
        GameManager?.ExitCombat();
        
        EmitSignal(SignalName.CombatEnded);
        GD.Print("=== COMBAT END ===");
    }
    
    public Entity GetCurrentTurnEntity()
    {
        if (_combatants.Count == 0) return null;
        return _combatants[_currentTurnIndex];
    }
    
    public void PlayerEndTurn()
    {
        var player = GetCurrentTurnEntity();
        if (player != null)
        {
            GD.Print($"{player.EntityName} ends their turn");
        }
        
        AdvanceTurn();
        ProcessAllAITurnsUntilPlayer();
    }
    
    private void ProcessAllAITurnsUntilPlayer()
    {
        int turnCount = 0;
        
        while (_combatActive)
        {
            turnCount++;
            
            if (turnCount > 100)
            {
                GD.PrintErr("Safety break - too many consecutive AI turns!");
                break;
            }
            
            RemoveDeadCombatants();
            
            if (CheckCombatEnd())
            {
                EndCombat();
                return;
            }
            
            var currentEntity = GetCurrentTurnEntity();
            if (currentEntity == null) return;
            
            EmitSignal(SignalName.TurnChanged, currentEntity);
            
            if (currentEntity.IsAI)
            {
                ProcessAITurn(currentEntity);
                AdvanceTurn();
            }
            else
            {
                ProcessPlayerTurn(currentEntity);
                return;
            }
        }
    }
    
    private void ProcessAITurn(Entity entity)
    {
        GD.Print($"=== {entity.EntityName}'s Turn ===");
        entity.ResetMovement();
        entity.Act(_battleContext);
    }
    
    private void ProcessPlayerTurn(Entity entity)
    {
        GD.Print($"=== {entity.EntityName}'s Turn ===");
        entity.ResetMovement();
        EmitSignal(SignalName.PlayerTurnStarted);
    }
    
    private void AdvanceTurn()
    {
        _currentTurnIndex = (_currentTurnIndex + 1) % _combatants.Count;
        
        if (_currentTurnIndex == 0)
        {
            GD.Print("=== NEW ROUND ===");
        }
    }
    
    private void RemoveDeadCombatants()
    {
        var deadEntities = _combatants.Where(e => !e.IsAlive).ToList();
        
        foreach (var dead in deadEntities)
        {
            GD.Print($"{dead.EntityName} has been defeated!");
            _combatants.Remove(dead);
            
            if (_currentTurnIndex >= _combatants.Count)
            {
                _currentTurnIndex = 0;
            }
        }
    }
    
    private bool CheckCombatEnd()
    {
        var livingCombatants = _combatants.Where(e => e.IsAlive).ToList();
        
        if (livingCombatants.Count <= 1)
        {
            if (livingCombatants.Count == 1)
            {
                GD.Print($"{livingCombatants[0].EntityName} is victorious!");
            }
            return true;
        }
        
        return false;
    }
    
    private void RollInitiative()
    {
        var random = new System.Random();
        _initiativeRolls.Clear();
        
        foreach (var combatant in _combatants)
        {
            int roll = random.Next(1, 21);
            _initiativeRolls[combatant] = roll;
            GD.Print($"{combatant.EntityName} rolled {roll} for initiative");
        }
        
        // Sort by initiative (highest first), with random tiebreaker for now
        _combatants = _combatants
            .OrderByDescending(e => _initiativeRolls[e])
            .ThenByDescending(_ => random.Next())
            .ToList();
    }
    
    private void PrintInitiativeOrder()
    {
        GD.Print("Initiative Order:");
        for (int i = 0; i < _combatants.Count; i++)
        {
            GD.Print($"  {i + 1}. {_combatants[i].EntityName}");
        }
    }

    public void PlayerAttack(Entity target)
    {
        var player = GetCurrentTurnEntity();
        if (player == null) return;

        if (!_battleContext.CanMeleeAttack(player, target))
        {
            GD.PrintErr($"Invalid attack: {player.EntityName} cannot attack {target.EntityName} (out of range or blocked by walls)");
            return;
        }

        var result = player.MeleeAttack(target);

        GD.Print($"{player.EntityName} attacks {target.EntityName}!");
        if (result.Hit)
        {
            GD.Print($"  Hit! Rolled {result.AttackRoll}, dealt {result.Damage} damage{(result.CriticalHit ? " (CRITICAL!)" : "")}");
        }
        else
        {
            GD.Print($"  Miss! Rolled {result.AttackRoll}");
        }
    }
    
    public List<Entity> GetAllCombatants()
    {
        return _combatants;
    }
}