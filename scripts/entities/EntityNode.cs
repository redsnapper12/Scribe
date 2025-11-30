using Godot;
using Scribe.Scripts.Core;
using Scribe.Scripts.Data.ComponentData;
using Scribe.Scripts.Core.Interfaces;
using Scribe.Scripts.Core.Interfaces.Entities;

namespace Scribe.Scripts.Entities;

public partial class EntityNode : Node2D
{
    private Entity _entity;
    private ProgressBar _healthBar;
    private Label _nameLabel;
    private Sprite2D _sprite;
    
    public Entity Entity
    {
        get => _entity;
        set
        {
            if (_entity != null)
            {
                var oldHealthComponent = _entity.GetComponent<HealthComponent>();
                if (oldHealthComponent != null)
                {
                    oldHealthComponent.Damaged -= OnEntityDamaged;
                    oldHealthComponent.Death -= OnEntityDeath;
                }
            }
            
            _entity = value;
            UpdateVisuals();
            
            if (_entity != null)
            {
                var healthComponent = _entity.GetComponent<HealthComponent>();
                if (healthComponent != null)
                {
                    healthComponent.Damaged += OnEntityDamaged;
                    healthComponent.Death += OnEntityDeath;
                }
            }
        }
    }
    
    public override void _Ready()
    {
        _sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        _healthBar = GetNodeOrNull<ProgressBar>("HealthBar");
        _nameLabel = GetNodeOrNull<Label>("NameLabel");
        
        if (_entity != null)
        {
            UpdateVisuals();
        }
    }
    
    public override void _Process(double delta)
    {
        if (_entity != null)
        {
            GlobalPosition = GridManager.GridToWorld(_entity.GridPosition);
        }
    }
    
    private void UpdateVisuals()
    {
        if (_nameLabel != null && _entity != null)
        {
            _nameLabel.Text = _entity.EntityName;
        }

        if (_sprite != null && _entity != null)
        {
            _sprite.Texture = _entity.Icon;
        }
        
        UpdateHealthBar();
    }
    
    private void UpdateHealthBar()
    {
        if (_healthBar != null && _entity != null)
        {
            _healthBar.MaxValue = _entity.MaxHP;
            _healthBar.Value = _entity.CurrentHP;
        }
    }
    
    private void OnEntityDamaged(int amount, int damageTypeInt)
    {
        DamageType damageType = (DamageType)damageTypeInt;
        UpdateHealthBar();
    }
    
    private void OnEntityDeath()
    {
        if (_sprite != null)
        {
            Modulate = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }
    }
}