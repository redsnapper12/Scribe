using Godot;
using Scribe.Scripts.Core;
using Scribe.Scripts.Entities;

namespace Scribe.Scripts.Items;

/// <summary>
/// Visual representation of an item on the ground/in the world.
/// Similar to EntityNode pattern.
/// </summary>
public partial class ItemNode : Node2D
{
    public Item Item { get; private set; }
    public Vector2I GridPosition { get; private set; }

    [Export] private Sprite2D _sprite;
    [Export] private AnimationPlayer _animator;
    [Export] private Label _nameLabel;

    public override void _Ready()
    {
        if (_sprite == null)
        {
            GD.PrintErr("ItemNode: Missing Sprite2D child node");
        }

        if (Item != null)
        {
            UpdateVisual();
        }
    }

    public override void _Process(double delta)
    {
        if (Item != null)
        {
            GlobalPosition = GridManager.GridToWorld(GridPosition);
        }
    }

    /// <summary>
    /// Initializes this ItemNode with an item and grid position.
    /// </summary>
    public void Initialize(Item item, Vector2I gridPosition)
    {
        Item = item;
        GridPosition = gridPosition;

        // Position in world space
        GlobalPosition = GridManager.GridToWorld(gridPosition);

        if (IsNodeReady())
        {
            UpdateVisual();
        }
    }

    /// <summary>
    /// Updates the visual representation based on the item's icon.
    /// </summary>
    public void UpdateVisual()
    {
        if (Item == null)
            return;

        if (_sprite != null && Item.Icon != null)
        {
            _sprite.Texture = Item.Icon;
        }

        if (_nameLabel != null)
        {
            _nameLabel.Text = Item.DisplayName;
        }

        // Optional: Add a subtle idle animation (bobbing, etc.)
        if (_animator != null && _animator.HasAnimation("idle"))
        {
            _animator.Play("idle");
        }
    }

    /// <summary>
    /// Called when an entity picks up this item.
    /// </summary>
    public void OnPickup(Entity picker)
    {
        Item?.OnPickup(picker);

        // Play pickup animation/effect
        if (_animator != null && _animator.HasAnimation("pickup"))
        {
            _animator.Play("pickup");
            // Wait for animation to finish before removing
            _animator.AnimationFinished += (animName) =>
            {
                if (animName == "pickup")
                {
                    QueueFree();
                }
            };
        }
        else
        {
            // No animation, remove immediately
            QueueFree();
        }
    }

    /// <summary>
    /// Called when an entity examines this item.
    /// </summary>
    public void OnExamine(Entity examiner)
    {
        Item?.OnExamine(examiner);

        // Optional: Play examine visual effect
        if (_animator != null && _animator.HasAnimation("examine"))
        {
            _animator.Play("examine");
        }
    }

    /// <summary>
    /// Updates the grid position and world position.
    /// </summary>
    public void SetGridPosition(Vector2I newPosition)
    {
        GridPosition = newPosition;
        GlobalPosition = GridManager.GridToWorld(newPosition);
    }
}
