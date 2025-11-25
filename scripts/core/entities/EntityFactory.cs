using Godot;
using System;
using Scribe.Scripts.Core.Components;
using Scribe.Scripts.Core.Interfaces;
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
    }
    
    private static void BuildMonster(Entity entity, MonsterData data)
    {
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
    }
    
    public static Entity CreateFromPath(string resourcePath)
    {
        var data = GD.Load<EntityData>(resourcePath);
        if (data == null)
        {
            GD.PrintErr($"Failed to load entity data from: {resourcePath}");
            return null;
        }
        
        return CreateEntity(data);
    }
}