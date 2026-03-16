using Godot;
using Godot.Collections;
using Scribe.Scripts.Core.Interfaces.Items;
using Scribe.Scripts.Items;

namespace Scribe.Scripts.Data.ComponentData;

/// <summary>
/// Resource data for equipment tracking component.
/// </summary>
[GlobalClass]
public partial class EquipmentComponentData : EntityComponentData
{
    /// <summary>
    /// Pre-equipped items for entity templates.
    /// Maps equipment slots to ItemData resources.
    /// </summary>
    [Export] public Dictionary<EquipSlot, ItemData> StartingEquipment { get; set; } = new();

    public override IEntityComponent CreateComponent()
    {
        var component = new EquipmentComponent();

        // Load starting equipment from ItemData resources
        foreach (var kvp in StartingEquipment)
        {
            if (kvp.Value != null)
            {
                var item = kvp.Value.CreateItem();
                component.Equip(item);
            }
        }

        return component;
    }
}
