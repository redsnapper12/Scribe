using Godot;
using Godot.Collections;
using Scribe.Scripts.Data.ComponentData;
using Scribe.Scripts.Items;

namespace Scribe.Scripts.Data;

/// <summary>
/// Base resource class for all item definitions.
/// Item data is defined in Godot .tres files and used to instantiate Item instances at runtime.
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

    /// <summary>
    /// Adds required components to the ComponentData array
    /// </summary>
    public virtual void AppendRequiredComponents()
    {
        
    }

    /// <summary>
    /// Removes duplicate component data from the ComponentData array.
    /// Keeps only the last occurrence of each component type.
    /// </summary>
    public void FilterDuplicates()
    {
        var seen = new System.Collections.Generic.Dictionary<System.Type, ItemComponentData>();
        var filtered = new Array<ItemComponentData>();

        // Iterate through all components
        foreach (var componentData in ComponentData)
        {
            if (componentData != null)
            {
                var type = componentData.GetType();
                // Store the component, overwriting any previous one of the same type
                seen[type] = componentData;
            }
        }

        // Build the filtered array with unique components
        foreach (var kvp in seen)
        {
            filtered.Add(kvp.Value);
        }

        ComponentData = filtered;
    }
}
