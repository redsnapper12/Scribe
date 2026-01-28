using Godot;
using Scribe.Scripts.Data.ComponentData;
using System.Collections.Generic;

namespace Scribe.Scripts.UI.AbilityScoreGenerators;

/// <summary>
/// Manages ability score assignment using the Point Buy method (27 points).
/// Base score of 8, can buy up to 15. Costs follow D&D 5e 2024 rules.
/// </summary>
public partial class PointBuyGenerator : AbilityScoreGenerator
{
    private const int TOTAL_POINTS = 27;
    private const int MIN_SCORE = 8;
    private const int MAX_SCORE = 15;

    // Point costs for each score (index 0 = score 8, index 7 = score 15)
    private static readonly int[] POINT_COSTS = { 0, 1, 2, 3, 4, 5, 7, 9 };

    private int _pointsRemaining = TOTAL_POINTS;

    [Signal]
    public delegate void PointsRemainingChangedEventHandler(int pointsRemaining);

    public PointBuyGenerator()
    {
        // Initialize all abilities to minimum score (8)
        foreach (AbilityScore ability in System.Enum.GetValues<AbilityScore>())
        {
            _assignments[ability] = MIN_SCORE;
        }
    }

    /// <summary>
    /// Gets the number of points remaining to spend.
    /// </summary>
    public int GetPointsRemaining()
        => _pointsRemaining;

    /// <summary>
    /// Sets a score for an ability. Returns true if successful (enough points).
    /// </summary>
    public bool SetScore(AbilityScore ability, int score)
    {
        if (score < MIN_SCORE || score > MAX_SCORE)
            return false;

        int currentScore = _assignments[ability];
        int currentCost = GetCostForScore(currentScore);
        int newCost = GetCostForScore(score);
        int pointDifference = newCost - currentCost;

        // Check if we have enough points
        if (_pointsRemaining - pointDifference < 0)
            return false;

        _assignments[ability] = score;
        _pointsRemaining -= pointDifference;

        EmitSignal(SignalName.AssignmentsChanged);
        EmitSignal(SignalName.PointsRemainingChanged, _pointsRemaining);
        return true;
    }

    /// <summary>
    /// Increases an ability score by 1 if possible.
    /// </summary>
    public bool IncrementScore(AbilityScore ability)
    {
        int currentScore = _assignments[ability];
        if (currentScore >= MAX_SCORE)
            return false;

        return SetScore(ability, currentScore + 1);
    }

    /// <summary>
    /// Decreases an ability score by 1 if possible.
    /// </summary>
    public bool DecrementScore(AbilityScore ability)
    {
        int currentScore = _assignments[ability];
        if (currentScore <= MIN_SCORE)
            return false;

        return SetScore(ability, currentScore - 1);
    }

    /// <summary>
    /// Gets the point cost for a specific score.
    /// </summary>
    public static int GetCostForScore(int score)
    {
        if (score < MIN_SCORE || score > MAX_SCORE)
            return 0;

        return POINT_COSTS[score - MIN_SCORE];
    }

    /// <summary>
    /// Gets the valid score range.
    /// </summary>
    public (int min, int max) GetScoreRange()
        => (MIN_SCORE, MAX_SCORE);

    public override void Reset()
    {
        _assignments.Clear();
        foreach (AbilityScore ability in System.Enum.GetValues<AbilityScore>())
        {
            _assignments[ability] = MIN_SCORE;
        }
        _pointsRemaining = TOTAL_POINTS;

        EmitSignal(SignalName.AssignmentsChanged);
        EmitSignal(SignalName.PointsRemainingChanged, _pointsRemaining);
    }

    public override string GetMethodName()
        => "Point Buy";

    public override AbilityScoreGenerationMethod GetMethodType()
        => AbilityScoreGenerationMethod.PointBuy;

    /// <summary>
    /// Point buy is complete when all points are spent or user chooses to finish.
    /// Override to always return true since all abilities are assigned from the start.
    /// </summary>
    public new bool IsComplete()
        => true; // Point buy is always "complete" since all scores start at 8
}
