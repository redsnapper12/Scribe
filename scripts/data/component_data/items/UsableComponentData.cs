using Godot;
using Scribe.Scripts.AI;
using Scribe.Scripts.Core.Entities;
using Scribe.Scripts.Items.Components;

namespace Scribe.Scripts.Items.Data;

/// <summary>
/// Resource data for usable item components.
/// </summary>
[GlobalClass]
public partial class UsableComponentData : ItemComponentData
{
    [Export] public int MaxCharges { get; set; } = 1;
    [Export] public bool ConsumedOnUse { get; set; } = true;
    [Export] public UsableEffectType EffectType { get; set; } = UsableEffectType.Healing;
    [Export] public int EffectAmount { get; set; } = 10;

    public override IItemComponent CreateComponent()
    {
        return new UsableComponent
        {
            MaxCharges = this.MaxCharges,
            ConsumedOnUse = this.ConsumedOnUse,
            EffectType = this.EffectType,
            EffectAmount = this.EffectAmount
        };
    }
}

/// <summary>
/// Types of effects usable items can have.
/// </summary>
public enum UsableEffectType
{
    Healing,
    Damage,
    Buff,
    Debuff,
    Teleport,
    Summon
}

/// <summary>
/// Implementation of IUsable component.
/// Basic implementation - can be extended for specific item types.
/// </summary>
public class UsableComponent : IUsable
{
    public int Charges { get; set; }
    public int MaxCharges { get; set; }
    public bool ConsumedOnUse { get; set; }
    public UsableEffectType EffectType { get; set; }
    public int EffectAmount { get; set; }

    public UsableComponent()
    {
        // Start with max charges
        Charges = MaxCharges;
    }

    public virtual bool OnUse(Entity user, BattleContext context)
    {
        if (!CanUse(user))
            return false;

        // Apply effect based on type
        switch (EffectType)
        {
            case UsableEffectType.Healing:
                user.Heal(EffectAmount);
                GD.Print($"{user.Name} healed for {EffectAmount} HP");
                break;

            case UsableEffectType.Damage:
                // Could target an enemy, but for now just log
                GD.Print($"Used damage item: {EffectAmount} damage");
                break;

            case UsableEffectType.Buff:
                GD.Print($"Applied buff to {user.Name}");
                // Future: Apply modifier/buff
                break;

            default:
                GD.Print($"Used item with effect: {EffectType}");
                break;
        }

        // Consume charge
        if (Charges > 0)
        {
            Charges--;
        }

        return true;
    }

    public virtual bool CanUse(Entity user)
    {
        if (user == null || !user.IsAlive)
            return false;

        // Check if has charges (or infinite charges)
        if (Charges == 0 && MaxCharges != -1)
            return false;

        return true;
    }
}
