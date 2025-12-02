using Godot;
using Godot.Collections;
using Scribe.Scripts.Entities;

namespace Scribe.Scripts.Data.ComponentData.Traits;

/// <summary>
/// Nimble Escape: Can take Disengage or Hide as a bonus action.
/// </summary>
public partial class NimbleEscapeTrait : BaseTrait
{
    public NimbleEscapeTrait(TraitInfo info) : base(info)
    {
    }

    public override string[] GetBonusActions()
    {
        return new[] { "Disengage", "Hide" };
    }

    public override void ExecuteBonusAction(string actionName, Entity entity,
        Dictionary context = null)
    {
        switch (actionName)
        {
            case "Disengage":
                // TODO: Set "Disengaged" status on entity (prevents opportunity attacks)
                GD.Print($"{entity.EntityName} uses Nimble Escape to Disengage!");
                break;
            case "Hide":
                // TODO: Make stealth check, set "Hidden" status
                GD.Print($"{entity.EntityName} uses Nimble Escape to Hide!");
                break;
        }
    }
}
