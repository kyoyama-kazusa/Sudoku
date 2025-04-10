namespace Sudoku.Analytics.Construction.Chaining.Rules;

/// <summary>
/// Represents a chaining rule on AUR rule.
/// </summary>
public abstract class UniqueRectangleChainingRule : ChainingRule
{
	/// <inheritdoc/>
	public sealed override void GetViewNodes(
		in Grid grid,
		Chain pattern,
		View view,
		ref int currentAlsIndex,
		ref int currentUrIndex,
		out ReadOnlySpan<ViewNode> producedViewNodes
	)
	{
		var urIndex = currentUrIndex;
		var result = new List<ViewNode>();
		foreach (var link in pattern.Links)
		{
			if (link.GroupedLinkPattern is not UniqueRectanglePattern { Cells: var cells, DigitsMask: var digitsMask })
			{
				continue;
			}

			// If the cell has already been colorized, we should change the color into UR-categorized one.
			var id = (ColorIdentifier)(urIndex + WellKnownColorIdentifierKind.Rectangle1);
			foreach (var cell in cells)
			{
				foreach (var digit in (Mask)(grid.GetCandidates(cell) & digitsMask))
				{
					var candidate = cell * 9 + digit;
					if (view.FindCandidate(candidate) is { } candidateViewNode)
					{
						view.Remove(candidateViewNode);
					}
					var node = new CandidateViewNode(id, candidate);
					view.Add(node);
					result.Add(node);
				}
			}
			foreach (var cell in cells)
			{
				if (view.FindCell(cell) is { } cellViewNode)
				{
					view.Remove(cellViewNode);
				}
				var node = new CellViewNode(id, cell);
				view.Add(node);
				result.Add(node);
			}
			urIndex = (urIndex + 1) % 3;
		}

		currentUrIndex = urIndex;
		producedViewNodes = result.AsSpan();
	}
}
