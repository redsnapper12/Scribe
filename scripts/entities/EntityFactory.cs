using Godot;
using System;
using Scribe.Scripts.Core.Components;
using Scribe.Scripts.Data.ComponentData;
using Scribe.Scripts.Data;
using Scribe.Scripts.Data.EntityDataTypes;

namespace Scribe.Scripts.Core.Entities;

public static class EntityFactory
{
    public static Entity CreateEntity(EntityData data)
    {
        if (data == null)
        {
            GD.PrintErr("Cannot create entity from null data");
            return null;
        }
        
        var entity = new Entity
        {
            Name = data.EntityName,
            GridPosition = Vector2I.Zero
        };
        
        switch (data)
        {
            case CharacterData characterData:
                BuildCharacter(entity, characterData);
                break;
            case MonsterData monsterData:
                BuildMonster(entity, monsterData);
                break;
            default:
                GD.PrintErr($"Unknown entity data type: {data.GetType().Name}");
                return null;
        }
        
        entity.InitializeComponents();
        
        return entity;
    }
    
    private static void BuildCharacter(Entity entity, CharacterData data)
    {
        // Characters are player-controlled by default
        entity.Controller = ControllerType.Player;
        
        if (data.HealthData != null)
        {
            var healthComponent = new HealthComponent();
            healthComponent.Initialize(data.HealthData);
            entity.AddComponent(healthComponent);
        }
        
        if (data.MeleeAttackData != null)
        {
            var attackComponent = new MeleeAttackComponent();
            attackComponent.Initialize(data.MeleeAttackData);
            entity.AddComponent(attackComponent);
        }
        
        if (data.MovementData != null)
        {
            var movementComponent = new MovementComponent();
            movementComponent.Initialize(data.MovementData);
            entity.AddComponent(movementComponent);
        }
        else
        {
            // Default movement if not specified
            var movementComponent = new MovementComponent();
            movementComponent.Initialize(new MovementComponentData());
            entity.AddComponent(movementComponent);
        }
    }
    
    private static void BuildMonster(Entity entity, MonsterData data)
    {
        // Monsters are AI-controlled by default
        entity.Controller = ControllerType.AI;
        
        if (data.HealthData != null)
        {
            var healthComponent = new HealthComponent();
            healthComponent.Initialize(data.HealthData);
            entity.AddComponent(healthComponent);
        }
        
        if (data.MeleeAttackData != null)
        {
            var attackComponent = new MeleeAttackComponent();
            attackComponent.Initialize(data.MeleeAttackData);
            entity.AddComponent(attackComponent);
        }
        
        if (data.AIData != null)
        {
            var aiComponent = new AIComponent();
            aiComponent.Initialize(data.AIData);
            entity.AddComponent(aiComponent);
        }
        
        if (data.MovementData != null)
        {
            var movementComponent = new MovementComponent();
            movementComponent.Initialize(data.MovementData);
            entity.AddComponent(movementComponent);
        }
        else
        {
            // Default movement if not specified
            var movementComponent = new MovementComponent();
            movementComponent.Initialize(new MovementComponentData());
            entity.AddComponent(movementComponent);
        }
    }
}