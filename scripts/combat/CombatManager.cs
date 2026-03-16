using Godot;
using System.Collections.Generic;
using System.Linq;
using Scribe.Scripts.AI;
using Scribe.Scripts.Core;
using Scribe.Scripts.Entities;
using Scribe.Scripts.Data.ComponentData;

namespace Scribe.Scripts.Combat;

public partial class CombatManager : Node
{
    private List<Entity> _combatants = new();
    private Dictionary<Entity, int> _initiativeRolls = new();
    private int _currentTurnIndex = 0;
    private int _currentRound = 1;
    private bool _combatActive = false;
    private BattleContext _battleContext;

    public BattleContext BattleContext => _battleContext;
    
    [Export] public BattleGridView BattleGridView { get; set; }
    [Export] public GameManager GameManager { get; set; }
    [Export] public CharacterData PlayerData { get; set; }
    [Export] public CreatureData GoblinData { get; set; }
    
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

    [Signal]
    public delegate void RoundStartedEventHandler(int roundNumber);
    
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
    
    public void InitiateSetupAndStart()
    {
        if (!ValidateSetup())
        {
            GD.PrintErr("CombatManager: Cannot start — failed validation");
            return;
        }

        SetupCombat();
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
    
    private Vector2I GetRandomWalkableCell(HashSet<Vector2I> occupied)
    {
        if (GridManager == null)
            return Vector2I.Zero;

        var candidates = new List<Vector2I>();
        for (int x = 0; x < GridManager.GridWidth; x++)
            for (int y = 0; y < GridManager.GridHeight; y++)
            {
                var cell = new Vector2I(x, y);
                if (GridManager.IsWalkable(cell) && !occupied.Contains(cell))
                    candidates.Add(cell);
            }

        if (candidates.Count == 0)
        {
            GD.PrintErr("CombatManager: No walkable cells available for spawn!");
            return Vector2I.Zero;
        }

        return candidates[GD.RandRange(0, candidates.Count - 1)];
    }

    private void SetupCombat()
    {
        var occupied = new HashSet<Vector2I>();
        var allEntities = new List<Entity>();

        // Spawn player characters — use SessionCharacters if available, else fall back to PlayerData export
        var sessionChars = GameManager?.SessionCharacters;
        if (sessionChars != null && sessionChars.Count > 0)
        {
            foreach (var charData in sessionChars)
            {
                var cell = GetRandomWalkableCell(occupied);
                occupied.Add(cell);
                var entity = charData.CreateEntity(cell);
                GameManager?.RegisterEntity(entity);
                BattleGridView?.EntityContainer?.AddChild(CreateEntityNode(entity));
                allEntities.Add(entity);
            }
        }
        else if (PlayerData != null)
        {
            var playerCell = GetRandomWalkableCell(occupied);
            occupied.Add(playerCell);
            var player = PlayerData.CreateEntity(playerCell);
            GameManager?.RegisterEntity(player);
            BattleGridView?.EntityContainer?.AddChild(CreateEntityNode(player));
            allEntities.Add(player);
        }

        // Spawn goblin (placeholder enemy)
        if (GoblinData != null)
        {
            var goblinCell = GetRandomWalkableCell(occupied);
            occupied.Add(goblinCell);
            var goblin = GoblinData.CreateEntity(goblinCell);
            GameManager?.RegisterEntity(goblin);
            BattleGridView?.EntityContainer?.AddChild(CreateEntityNode(goblin));
            allEntities.Add(goblin);
        }

        _combatants = allEntities;

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
        _combatants = combatants
            .Where(e => e.TryGetComponent<HealthComponent>(out var health) && health.IsAlive)
            .ToList();
        _battleContext.AllEntities = _combatants;
        
        if (_combatants.Count == 0)
        {
            GD.PrintErr("Cannot start combat with no combatants");
            return;
        }
        
        GameManager?.EnterCombat(this);
        
        RollInitiative();
        
        _currentTurnIndex = 0;
        _currentRound = 1;
        _combatActive = true;

        // Reset reactions for all combatants at combat start (round 1)
        foreach (var combatant in _combatants)
        {
            if (combatant.TryGetComponent<ActionEconomyComponent>(out var actionEconomy))
            {
                actionEconomy.ResetRound();
            }
        }

        EmitSignal(SignalName.CombatStarted);
        MessagePanelUI.Instance?.EnqueueMessage("=== COMBAT START ===", Colors.White);
        MessagePanelUI.Instance?.EnqueueMessage($"=== ROUND {_currentRound} ===", Colors.Yellow);
        PrintInitiativeOrder();

        ProcessAllAITurnsUntilPlayer();
    }
    
    public void EndCombat()
    {
        _combatActive = false;
        _initiativeRolls.Clear();
        
        GameManager?.ExitCombat();
        
        EmitSignal(SignalName.CombatEnded);
        MessagePanelUI.Instance?.EnqueueMessage("=== COMBAT END ===", Colors.White);
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
            MessagePanelUI.Instance?.EnqueueMessage($"{player.EntityName} ends their turn.", Colors.White);
        
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

            if (currentEntity.TryGetComponent<MovementComponent>(out var movement))
                movement.ResetMovement();

            if (currentEntity.TryGetComponent<ActionEconomyComponent>(out var actionEconomy))
                actionEconomy.ResetTurn();

            EmitSignal(SignalName.TurnChanged, currentEntity);

            if (currentEntity.Controller == Core.Services.ControllerType.AI)
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
        MessagePanelUI.Instance?.EnqueueMessage($"=== {entity.EntityName}'s Turn ===", Colors.Yellow);

        if (entity.TryGetComponent<AIComponent>(out var ai))
            ai.Act(entity, _battleContext);
    }
    
    private void ProcessPlayerTurn(Entity entity)
    {
        MessagePanelUI.Instance?.EnqueueMessage($"=== {entity.EntityName}'s Turn ===", Colors.Cyan);

        EmitSignal(SignalName.PlayerTurnStarted);
    }
    
    private void AdvanceTurn()
    {
        _currentTurnIndex = (_currentTurnIndex + 1) % _combatants.Count;

        if (_currentTurnIndex == 0)
        {
            _currentRound++;
            MessagePanelUI.Instance?.EnqueueMessage($"=== ROUND {_currentRound} ===", Colors.Yellow);
            EmitSignal(SignalName.RoundStarted, _currentRound);

            // Reset reactions for all combatants at round start
            foreach (var combatant in _combatants)
            {
                if (combatant.TryGetComponent<ActionEconomyComponent>(out var actionEconomy))
                {
                    actionEconomy.ResetRound();
                }
            }
        }
    }
    
    private void RemoveDeadCombatants()
    {
        var deadEntities = _combatants
            .Where(e => !e.TryGetComponent<HealthComponent>(out var health) || !health.IsAlive)
            .ToList();
        
        foreach (var dead in deadEntities)
        {
            MessagePanelUI.Instance?.EnqueueMessage($"{dead.EntityName} has been defeated!", Colors.Red);
            _combatants.Remove(dead);
            
            if (_currentTurnIndex >= _combatants.Count)
            {
                _currentTurnIndex = 0;
            }
        }
    }
    
    private bool CheckCombatEnd()
    {
        var livingCombatants = _combatants
            .Where(e => e.TryGetComponent<HealthComponent>(out var health) && health.IsAlive)
            .ToList();
        
        if (livingCombatants.Count <= 1)
        {
            if (livingCombatants.Count == 1)
                MessagePanelUI.Instance?.EnqueueMessage($"{livingCombatants[0].EntityName} is victorious!", Colors.Gold);
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
            MessagePanelUI.Instance?.EnqueueMessage($"{combatant.EntityName} rolled {roll} for initiative.", Colors.White);
        }
        
        // Sort by initiative (highest first), with random tiebreaker for now
        _combatants = _combatants
            .OrderByDescending(e => _initiativeRolls[e])
            .ThenByDescending(_ => random.Next())
            .ToList();
    }
    
    private void PrintInitiativeOrder()
    {
        MessagePanelUI.Instance?.EnqueueMessage("Initiative Order:", Colors.White);
        for (int i = 0; i < _combatants.Count; i++)
            MessagePanelUI.Instance?.EnqueueMessage($"  {i + 1}. {_combatants[i].EntityName}", Colors.White);
    }

    public void PlayerAttack(Entity target)
    {
        var player = GetCurrentTurnEntity();
        if (player == null) return;

        // Action economy validation
        if (!player.TryGetComponent<ActionEconomyComponent>(out var actionEconomy))
        {
            GD.PrintErr($"{player.EntityName} has no action economy component");
            return;
        }

        // Check if we can attack (either already in attack action or have action available)
        if (!actionEconomy.CanAttack())
        {
            GD.PrintErr($"{player.EntityName} has no action available for attacking");
            return;
        }

        // Begin attack action if not already in one
        if (!actionEconomy.IsInAttackAction)
        {
            if (!actionEconomy.BeginAttackAction())
            {
                GD.PrintErr($"{player.EntityName} cannot begin attack action");
                return;
            }
        }

        if (!player.TryGetComponent<AttackComponent>(out var meleeAttack))
        {
            GD.PrintErr($"{player.EntityName} cannot perform melee attacks");
            return;
        }

        if (!player.TryGetComponent<EquipmentComponent>(out var equipment))
        {
            GD.PrintErr($"{player.EntityName} has no equipment component");
            return;
        }

        if (!target.TryGetComponent<HealthComponent>(out var targetHealth))
        {
            GD.PrintErr($"{target.EntityName} cannot take damage");
            return;
        }

        if (!_battleContext.CanMeleeAttack(player, target))
        {
            GD.PrintErr($"Invalid attack: {player.EntityName} cannot attack {target.EntityName} (out of range or blocked by walls)");
            return;
        }

        var meleeAttackData = equipment.GetMainHandWeaponMeleeData();
        var result = meleeAttack.MeleeAttack(player, targetHealth, meleeAttackData);

        MessagePanelUI.Instance?.EnqueueMessage($"{player.EntityName} attacks {target.EntityName}!", Colors.White);
        if (result.Hit)
        {
            var hitColor = result.IsCritical ? Colors.Gold : Colors.OrangeRed;
            var suffix = result.IsCritical ? " (CRITICAL!)" : "";
            MessagePanelUI.Instance?.EnqueueMessage($"  Hit! Rolled {result.AttackRoll}, dealt {result.Damage} damage{suffix}", hitColor);
        }
        else
        {
            MessagePanelUI.Instance?.EnqueueMessage($"  Miss! Rolled {result.AttackRoll}", Colors.Gray);
        }

        // Use one attack from the attack action
        actionEconomy.UseAttack();
    }
    
    public List<Entity> GetAllCombatants()
    {
        return _combatants;
    }
}