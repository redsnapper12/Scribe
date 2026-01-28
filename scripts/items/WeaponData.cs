using Godot;
using Scribe.Scripts.Data;
using Scribe.Scripts.Data.ComponentData;
using Scribe.Scripts.Items;
using System;

public partial class WeaponData : ItemData
{
    [Export] WeaponComponentData WeaponComponentData { get; set; } = new WeaponComponentData();
    [Export] EquippableComponentData EquippableComponentData { get; set; } = new EquippableComponentData();
    [Export] ValuableComponentData ValuableComponentData { get; set; } = new ValuableComponentData();

    public override Item CreateItem()
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

        AppendRequiredComponents();
        FilterDuplicates();

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

    public override void AppendRequiredComponents()
    {
        ComponentData.Add(WeaponComponentData);
        ComponentData.Add(EquippableComponentData);
        ComponentData.Add(ValuableComponentData);
    }
}
