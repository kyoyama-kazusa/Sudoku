namespace Sudoku.Drawing.Drawing2D;

public partial class GridCanvas
{
	/// <summary>
	/// Draw eliminations onto the canvas, with conflict check on nodes.
	/// If a node covers (uses) one candidate, the elimination will be treated as a cannibalism one, drawing with different color.
	/// </summary>
	/// <param name="conclusions">The conclusions to be drawn.</param>
	/// <param name="nodes">The nodes to be checked.</param>
	public partial void DrawEliminations(ReadOnlySpan<Conclusion> conclusions, ReadOnlySpan<CandidateViewNode> nodes)
	{
		if (Settings is not { EliminationColor: var eColor, CannibalismColor: var cColor })
		{
			return;
		}

		using var elimBrush = new SolidBrush(eColor);
		using var cannibalBrush = new SolidBrush(cColor);
		using var elimBrushLighter = new SolidBrush(eColor.QuarterAlpha);
		using var cannibalismBrushLighter = new SolidBrush(cColor.QuarterAlpha);
		foreach (var (t, c, d) in conclusions)
		{
			if (t != Elimination)
			{
				continue;
			}

			var cannibalism = false;
			if (nodes.Length == 0)
			{
				goto Drawing;
			}

			foreach (var node in nodes)
			{
				if (node.Candidate == c * 9 + d)
				{
					cannibalism = true;
					break;
				}
			}

		Drawing:
			_g.FillEllipse(
				(cannibalism, nodes.Any(node => node.Cell == c)) switch
				{
					(true, true) => cannibalismBrushLighter,
					(true, false) => cannibalBrush,
					(false, true) => elimBrushLighter,
					_ => elimBrush
				},
				_calculator.GetMouseRectangle(c, d)
			);
		}
	}
}
