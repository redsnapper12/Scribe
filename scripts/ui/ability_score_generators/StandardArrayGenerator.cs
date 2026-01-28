using Godot;
using Scribe.Scripts.Data.ComponentData;
using System.Collections.Generic;
using System.Linq;

namespace Scribe.Scripts.UI.AbilityScoreGenerators;

/// <summary>
/// Manages ability score assignment using the Standard Array method (15, 14, 13, 12, 10, 8).
/// </summary>
public partial class StandardArrayGenerator : AbilityScoreGenerator
{
    private static readonly int[] STANDARD_ARRAY = { 15, 14, 13, 12, 10, 8 };
    private readonly List<int> _availableScores;

    public StandardArrayGenerator()
    {
        _availableScores = new List<int>(STANDARD_ARRAY);
    }

    /// <summary>
    /// Gets the list of scores that haven't been assigned yet.
    /// </summary>
    public List<int> GetAvailableScores()
        => new(_availableScores);

    /// <summary>
    /// Assigns a score to an ability. Returns true if successful.
    /// </summary>
    public bool AssignScore(AbilityScore ability, int score)
    {
        // Check if score is available
        if (!_availableScores.Contains(score))
            return false;

        // If this ability already has a score assigned, return it to the pool
        if (_assignments.TryGetValue(ability, out int oldScore))
        {
            _availableScores.Add(oldScore);
            _availableScores.Sort((a, b) => b.CompareTo(a)); // Keep sorted descending
        }

        // Assign the new score
        _assignments[ability] = score;
        _availableScores.Remove(score);

        EmitSignal(SignalName.AssignmentsChanged);
        return true;
    }

    /// <summary>
    /// Unassigns a score from an ability, returning it to the available pool.
    /// </summary>
    public bool UnassignScore(AbilityScore ability)
    {
        if (!_assignments.TryGetValue(ability, out int score))
            return false;

        _assignments.Remove(ability);
        _availableScores.Add(score);
        _availableScores.Sort((a, b) => b.CompareTo(a)); // Keep sorted descending

        EmitSignal(SignalName.AssignmentsChanged);
        return true;
    }

    public override void Reset()
    {
        base.Reset();
        _availableScores.Clear();
        _availableScores.AddRange(STANDARD_ARRAY);
    }

    public override string GetMethodName()
        => "Standard Array";

    public override AbilityScoreGenerationMethod GetMethodType()
        => AbilityScoreGenerationMethod.StandardArray;
}
