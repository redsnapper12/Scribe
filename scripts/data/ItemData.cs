using Godot;
using Godot.Collections;
using Scribe.Scripts.Data.ComponentData;
using Scribe.Scripts.Items;

namespace Scribe.Scripts.Data;

/// <summary>
/// Base resource class for all item definitions.
/// Item data is defined in Godot .tres files and used to instantiate Item instances at runtime.
/// Similar to EntityData/MonsterData pattern.
/// </summary>
[GlobalClass]
public partial class ItemData : Resource
{
    [ExportGroup("Identity")]
    [Export] public string ItemId { get; set; } = "";
    [Export] public string DisplayName { get; set; } = "Unnamed Item";
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
    [Export] public Texture2D Icon { get; set; }

    [ExportGroup("Storage Properties")]
    [Export] public bool IsStackable { get; set; } = false;
    [Export] public int MaxStackSize { get; set; } = 1;
    [Export] public float Weight { get; set; } = 0.0f;

    [ExportGroup("Components")]
    [Export] public Array<ItemComponentData> ComponentData { get; set; } = new();

    /// <summary>
    /// Factory method to create an Item instance from this data.
    /// Each subclass can override to create specialized item types.
    /// </summary>
    public virtual Item CreateItem()
    {
        var item = new Item
        {
            ItemId = this.ItemId,
            DisplayName = this.DisplayName,
            Description = this.Description,
            Icon = this.Icon,
            IsStackable = this.IsStackable,
            MaxStackSize = this.MaxStackSize,
            Weight = this.Weight
        };

        // Add all components from component data
        foreach (var componentData in ComponentData)
        {
            if (componentData != null)
            {
                var component = componentData.CreateComponent();
                item.AddComponent(component);
            }
        }

        return item;
    }
}
