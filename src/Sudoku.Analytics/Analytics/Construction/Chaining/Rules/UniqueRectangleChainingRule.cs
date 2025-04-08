namespace Sudoku.Analytics.Construction.Chaining.Rules;

/// <summary>
/// Represents a chaining rule on AUR rule.
/// </summary>
public abstract class UniqueRectangleChainingRule : ChainingRule
{
	/// <inheritdoc/>
	public abstract override void GetLinks(in Grid grid, LinkDictionary strongLinks, LinkDictionary weakLinks, StepGathererOptions options);

	/// <inheritdoc/>
	public override void GetViewNodes(ref ChainingRuleViewNodeContext context)
	{
		ref readonly var grid = ref context.Grid;
		var pattern = context.Pattern;
		var view = context.View;

		var result = new List<ViewNode>();
		foreach (var link in pattern.Links)
		{
			if (link.GroupedLinkPattern is UniqueRectanglePattern { Cells: var cells, DigitsMask: var digitsMask })
			{
				// If the cell has already been colorized, we should change the color into UR-categorized one.
				foreach (var cell in cells)
				{
					foreach (var digit in (Mask)(grid.GetCandidates(cell) & digitsMask))
					{
						var candidate = cell * 9 + digit;
						if (view.FindCandidate(candidate) is { } candidateViewNode)
						{
							view.Remove(candidateViewNode);
						}
						var node = new CandidateViewNode(ColorIdentifier.Rectangle1, candidate);
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
					var node = new CellViewNode(ColorIdentifier.Rectangle1, cell);
					view.Add(node);
					result.Add(node);
				}
			}
		}
		context.ProducedViewNodes = result.AsSpan();
	}
}
