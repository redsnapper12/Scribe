using Godot;
using Scribe.Scripts.Core;
using Scribe.Scripts.Data.ComponentData;

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
                if (_entity.TryGetComponent<HealthComponent>(out var oldHealthComponent))
                {
                    oldHealthComponent.Damaged -= OnEntityDamaged;
                    oldHealthComponent.Death -= OnEntityDeath;
                }
            }
            
            _entity = value;
            UpdateVisuals();
            
            if (_entity != null)
            {

                if (_entity.TryGetComponent<HealthComponent>(out var healthComponent))
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
            if (_entity.TryGetComponent<HealthComponent>(out var health))
            {
                _healthBar.MaxValue = health.MaxHP;
                _healthBar.Value = health.CurrentHP;
            }
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