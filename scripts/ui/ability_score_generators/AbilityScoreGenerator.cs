using Godot;
using Godot.Collections;
using Scribe.Scripts.Data.ComponentData;
using System.Linq;

namespace Scribe.Scripts.UI.AbilityScoreGenerators;

/// <summary>
/// Ability score generation method types.
/// </summary>
public enum AbilityScoreGenerationMethod
{
    StandardArray = 0,
    PointBuy = 1,
    Rolling = 2
}

/// <summary>
/// Abstract base class for ability score generation methods.
/// </summary>
public abstract partial class AbilityScoreGenerator : RefCounted
{
    [Signal]
    public delegate void AssignmentsChangedEventHandler();

    protected readonly System.Collections.Generic.Dictionary<AbilityScore, int> _assignments = new();

    /// <summary>
    /// Gets the current ability score assignments.
    /// </summary>
    public System.Collections.Generic.Dictionary<AbilityScore, int> GetAssignments()
        => new(_assignments);

    /// <summary>
    /// Gets the score assigned to a specific ability, or null if not assigned.
    /// </summary>
    public int? GetScore(AbilityScore ability)
        => _assignments.TryGetValue(ability, out int score) ? score : null;

    /// <summary>
    /// Checks if all ability scores have been assigned.
    /// </summary>
    public bool IsComplete()
        => _assignments.Count == 6;

    /// <summary>
    /// Resets all assignments.
    /// </summary>
    public virtual void Reset()
    {
        _assignments.Clear();
        EmitSignal(SignalName.AssignmentsChanged);
    }

    /// <summary>
    /// Gets the name of this generation method.
    /// </summary>
    public abstract string GetMethodName();

    /// <summary>
    /// Gets the method type enum for this generator.
    /// </summary>
    public abstract AbilityScoreGenerationMethod GetMethodType();

    /// <summary>
    /// Factory method to create a generator based on the method type.
    /// </summary>
    public static AbilityScoreGenerator Create(AbilityScoreGenerationMethod method)
    {
        return method switch
        {
            AbilityScoreGenerationMethod.StandardArray => new StandardArrayGenerator(),
            AbilityScoreGenerationMethod.PointBuy => new PointBuyGenerator(),
            AbilityScoreGenerationMethod.Rolling => new RollingGenerator(),
            _ => new StandardArrayGenerator()
        };
    }

    /// <summary>
    /// Gets the display name for a method type.
    /// </summary>
    public static string GetDisplayName(AbilityScoreGenerationMethod method)
    {
        return method switch
        {
            AbilityScoreGenerationMethod.StandardArray => "Standard Array",
            AbilityScoreGenerationMethod.PointBuy => "Point Buy",
            AbilityScoreGenerationMethod.Rolling => "Rolling (4d6 Drop Lowest)",
            _ => "Unknown"
        };
    }
}
