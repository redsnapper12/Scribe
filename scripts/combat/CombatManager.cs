using Godot;
using System.Collections.Generic;
using System.Linq;
using Scribe.Scripts.Core;
using Scribe.Scripts.Core.Entities;
using Scribe.Scripts.Core.Interfaces;
using Scribe.Scripts.AI;
using Scribe.Scripts.Data.EntityDataTypes;

namespace Scribe.Scripts.Combat;

public partial class CombatManager : Node
{
    private List<Entity> _combatants = new();
    private Dictionary<Entity, int> _initiativeRolls = new();
    private int _currentTurnIndex = 0;
    private bool _combatActive = false;
    private BattleContext _battleContext;
    private bool _setupComplete = false;
    
    [Export] public TileMapLayer BattleGrid { get; set; }
    [Export] public GridManager GridManager { get; set; }
    [Export] public GameManager GameManager { get; set; }
    [Export] public EntityNode PlayerEntityNode { get; set; }
    [Export] public EntityNode GoblinEntityNode { get; set; }
    [Export] public CharacterData PlayerData { get; set; }
    [Export] public MonsterData GoblinData { get; set; }
    
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
        _battleContext = new BattleContext
        {
            BattleGrid = BattleGrid,
            GridManager = GridManager // Pass GridManager to BattleContext
        };
        
        if (!ValidateSetup())
        {
            GD.PrintErr("CombatManager: Failed validation");
            return;
        }
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
        
        if (PlayerEntityNode == null)
        {
            GD.PrintErr("CombatManager: PlayerEntityNode is not assigned!");
            valid = false;
        }
        
        if (GoblinEntityNode == null)
        {
            GD.PrintErr("CombatManager: GoblinEntityNode is not assigned!");
            valid = false;
        }
        
        if (GridManager == null)
        {
            GD.PrintErr("CombatManager: GridManager is not assigned!");
            valid = false;
        }
        
        return valid;
    }
    
    private void SetupCombat()
    {
        var player = EntityFactory.CreateEntity(PlayerData);
        var goblin = EntityFactory.CreateEntity(GoblinData);
        
        player.GridPosition = new Vector2I(3, 3);
        goblin.GridPosition = new Vector2I(25, 10);
        
        GameManager?.RegisterEntity(player);
        GameManager?.RegisterEntity(goblin);

        PlayerEntityNode.Entity = player;
        GoblinEntityNode.Entity = goblin;
        
        _combatants = new List<Entity> { player, goblin };
        
        CallDeferred(nameof(StartCombatDeferred));
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
            GD.Print($"{player.Name} ends their turn");
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
        GD.Print($"=== {entity.Name}'s Turn ===");
        entity.ResetMovement();
        entity.Act(_battleContext);
    }
    
    private void ProcessPlayerTurn(Entity entity)
    {
        GD.Print($"=== {entity.Name}'s Turn ===");
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
            GD.Print($"{dead.Name} has been defeated!");
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
                GD.Print($"{livingCombatants[0].Name} is victorious!");
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
            GD.Print($"{combatant.Name} rolled {roll} for initiative");
        }
        
        // Sort by initiative (highest first), with random tiebreaker for now
        // TODO: Use DEX modifier as tiebreaker when ability scores are wired up
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
            GD.Print($"  {i + 1}. {_combatants[i].Name}");
        }
    }

    public void PlayerAttack(Entity target)
    {
        var player = GetCurrentTurnEntity();
        if (player == null) return;

        var result = player.MeleeAttack(target);

        GD.Print($"{player.Name} attacks {target.Name}!");
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