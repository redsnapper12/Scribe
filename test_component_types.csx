using System;
using Godot;
using Godot.Collections;

// Simulate the component data classes
public abstract class EntityComponentData 
{
    public abstract object CreateComponent();
}

public class HealthComponentData : EntityComponentData
{
    public override object CreateComponent() => new object();
}

public class MovementComponentData : EntityComponentData
{
    public override object CreateComponent() => new object();
}

public class AIComponentData : EntityComponentData
{
    public override object CreateComponent() => new object();
}

// Test
var components = new object[]
{
    new HealthComponentData(),
    new MovementComponentData(),
    new AIComponentData(),
    new HealthComponentData(), // Duplicate
};

var seen = new System.Collections.Generic.Dictionary<Type, object>();

foreach (var componentData in components)
{
    if (componentData != null)
    {
        var type = componentData.GetType();
        Console.WriteLine($"Type: {type.Name}");
        seen[type] = componentData;
    }
}

Console.WriteLine($"\nUnique types in dictionary: {seen.Count}");
foreach (var kvp in seen)
{
    Console.WriteLine($"  - {kvp.Key.Name}");
}
