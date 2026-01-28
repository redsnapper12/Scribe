using Godot;
using Scribe.Scripts.Data.ComponentData;
using System.Collections.Generic;
using System.Linq;

namespace Scribe.Scripts.UI.AbilityScoreGenerators;

/// <summary>
/// Manages ability score assignment using the Rolling method (4d6 drop lowest).
/// Uses the DiceRollingService for all dice rolls.
/// </summary>
public partial class RollingGenerator : AbilityScoreGenerator
{
    private readonly List<int> _rolledScores = new();

    /// <summary>
    /// Rolls 4d6, drops the lowest, and returns the sum. Returns all 4 dice for display.
    /// </summary>
    public (int total, int[] dice) Roll4d6DropLowest()
    {
        int[] dice = new int[4];

        for (int i = 0; i < 4; i++)
        {
            var request = new DiceRollRequest(DieType.D6, 1, 0);
            var result = DiceRollingService.RequestRoll(request);
            dice[i] = result.BaseRoll;
        }

        // Sort descending and drop the lowest (take top 3)
        var sorted = dice.OrderByDescending(x => x).ToArray();
        int total = sorted[0] + sorted[1] + sorted[2];

        return (total, dice);
    }

    /// <summary>
    /// Generates a full set of 6 ability scores using 4d6 drop lowest.
    /// </summary>
    public List<(int score, int[] dice)> RollCompleteSet()
    {
        var results = new List<(int score, int[] dice)>();

        for (int i = 0; i < 6; i++)
        {
            results.Add(Roll4d6DropLowest());
        }

        return results;
    }

    /// <summary>
    /// Sets the rolled scores that players can assign to abilities.
    /// </summary>
    public void SetRolledScores(List<int> scores)
    {
        _rolledScores.Clear();
        _rolledScores.AddRange(scores);
    }

    /// <summary>
    /// Gets the scores that were rolled.
    /// </summary>
    public List<int> GetRolledScores()
        => new(_rolledScores);

    /// <summary>
    /// Gets the scores that haven't been assigned yet.
    /// </summary>
    public List<int> GetAvailableScores()
    {
        var available = new List<int>(_rolledScores);
        foreach (var score in _assignments.Values)
        {
            available.Remove(score);
        }
        return available;
    }

    /// <summary>
    /// Assigns a rolled score to an ability. Returns true if successful.
    /// </summary>
    public bool AssignScore(AbilityScore ability, int score)
    {
        var available = GetAvailableScores();

        // Check if score is available
        if (!available.Contains(score))
            return false;

        // If this ability already has a score assigned, it will become available again
        if (_assignments.TryGetValue(ability, out int oldScore))
        {
            // Just replace it, the old score automatically becomes available
        }

        _assignments[ability] = score;
        EmitSignal(SignalName.AssignmentsChanged);
        return true;
    }

    /// <summary>
    /// Unassigns a score from an ability, returning it to the available pool.
    /// </summary>
    public bool UnassignScore(AbilityScore ability)
    {
        if (!_assignments.ContainsKey(ability))
            return false;

        _assignments.Remove(ability);
        EmitSignal(SignalName.AssignmentsChanged);
        return true;
    }

    public override void Reset()
    {
        base.Reset();
        _rolledScores.Clear();
    }

    public override string GetMethodName()
        => "Rolling (4d6 Drop Lowest)";

    public override AbilityScoreGenerationMethod GetMethodType()
        => AbilityScoreGenerationMethod.Rolling;
}
