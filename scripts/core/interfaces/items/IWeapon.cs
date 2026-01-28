using Godot.Collections;
using Scribe.Scripts.Core.Interfaces;
using Scribe.Scripts.Data;

namespace Scribe.Scripts.Core.Interfaces.Items;

/// <summary>
/// Component for items that can be used as weapons.
/// </summary>
public interface IWeapon
{
    MeleeAttackData Attack { get; set; }
}
