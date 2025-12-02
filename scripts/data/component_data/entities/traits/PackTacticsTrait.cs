using Godot;
using Godot.Collections;
using Scribe.Scripts.Entities;

namespace Scribe.Scripts.Data.ComponentData.Traits;

/// <summary>
/// Pack Tactics: Advantage on attack rolls against a creature if at least one
/// ally is within 5 feet of the target and isn't incapacitated.
/// </summary>
public partial class PackTacticsTrait : BaseTrait
{
    private int _range = 5;  // 5 feet = 1 grid square

    public PackTacticsTrait(TraitInfo info) : base(info)
    {
        // Load range from parameters if specified
        if (info.Parameters.ContainsKey("RangeInFeet"))
        {
            _range = info.Parameters["RangeInFeet"].AsInt32();
        }
    }

    public override int ModifyAttackRoll(Entity target, Entity entity,
        Dictionary context = null)
    {
        if (context == null) return 0;

        // Check if any ally is within range of target
        if (context.ContainsKey("AllEntities"))
        {
            var allEntities = context["AllEntities"].AsGodotArray();

            foreach (var otherObj in allEntities)
            {
                if (otherObj.Obj is not Entity other) continue;
                if (other == entity) continue;  // Not self
                if (!other.IsAlive) continue;    // Must be alive
                if (other.OwnerPlayerId == target.OwnerPlayerId) continue; // Not enemy

                // Check distance to target (using grid positions)
                var distance = entity.GridPosition.DistanceTo(target.GridPosition);
                if (distance <= _range / 5)  // Convert feet to grid squares
                {
                    return 1;  // Advantage!
                }
            }
        }

        return 0;  // No advantage
    }
}
