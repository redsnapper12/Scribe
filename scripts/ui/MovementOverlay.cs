using Godot;
using System.Collections.Generic;
using Scribe.Scripts.Core;

namespace Scribe.Scripts.UI;

public partial class MovementOverlay : Node2D
{
    [Export] public Color ReachableColor { get; set; } = new Color(0, 0.5f, 1, 0.3f);
    [Export] public Color PathColor { get; set; } = new Color(0, 0.7f, 1, 0.6f);
    [Export] public Color InvalidColor { get; set; } = new Color(1, 0, 0, 0.3f);

    private List<Vector2I> _reachableCells = new();
    private List<Vector2I> _pathPreview = new();
    private HashSet<Vector2I> _reachableSet = new();  // For fast lookup

    public void ShowReachableCells(List<Vector2I> cells)
    {
        _reachableCells = cells ?? new List<Vector2I>();
        _reachableSet = new HashSet<Vector2I>(_reachableCells);
        QueueRedraw();
    }

    public void ShowPathPreview(List<Vector2I> path)
    {
        _pathPreview = path ?? new List<Vector2I>();
        QueueRedraw();
    }

    public void ClearPathPreview()
    {
        _pathPreview.Clear();
        QueueRedraw();
    }

    public void Clear()
    {
        _reachableCells.Clear();
        _reachableSet.Clear();
        _pathPreview.Clear();
        QueueRedraw();
    }

    public bool IsCellReachable(Vector2I cell)
    {
        return _reachableSet.Contains(cell);
    }

    public override void _Draw()
    {
        // Draw reachable cells
        foreach (var cell in _reachableCells)
        {
            DrawCellHighlight(cell, ReachableColor);
        }

        // Draw path preview on top
        foreach (var cell in _pathPreview)
        {
            DrawCellHighlight(cell, PathColor);
        }
    }

    private void DrawCellHighlight(Vector2I cell, Color color)
    {
        var worldPos = GridManager.GridToWorld(cell);
        var cellSize = GridManager.CELL_SIZE;

        // GridToWorld returns center, so offset to top-left
        var topLeft = new Vector2(
            worldPos.X - cellSize / 2f,
            worldPos.Y - cellSize / 2f
        );

        var rect = new Rect2(topLeft, new Vector2(cellSize, cellSize));
        DrawRect(rect, color);
    }
}