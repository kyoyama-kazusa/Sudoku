namespace Sudoku.Analytics.StepSearcherHelpers.Chaining.Rules;

/// <summary>
/// Represents a chaining rule on AUR rule.
/// </summary>
public closed class UniqueRectangleChainingRule : ChainingRule
{
	/// <inheritdoc/>
	public sealed override void GetViewNodes(
		in Grid grid,
		Chain pattern,
		View view,
		ProcessedViewNodeMap processedViewNodesMap,
		out ReadOnlySpan<ViewNode> producedViewNodes
	)
	{
		var urIndex = processedViewNodesMap.MaxKeyInRectangle is var key and not ColorDescriptorAlias.Normal
			? (key - ColorDescriptorAlias.Rectangle1 + 1) % 3
			: 0;
		var result = new List<ViewNode>();
		foreach (var link in pattern.Links)
		{
			if (link.GroupedLinkPattern is not UniqueRectanglePattern { Cells: var cells, DigitsMask: var digitsMask })
			{
				continue;
			}

			// If the cell has already been colorized, we should change the color into UR-categorized one.
			var id = urIndex + ColorDescriptorAlias.Rectangle1;
			foreach (var cell in cells)
			{
				foreach (var digit in (Mask)(grid.GetCandidates(cell) & digitsMask))
				{
					var candidate = cell * 9 + digit;
					if (view.FindCandidate(candidate) is { Identifier: var originalIdentifier } candidateViewNode)
					{
						if (originalIdentifier is >= ColorDescriptorAlias.Rectangle1 and <= ColorDescriptorAlias.Rectangle3)
						{
							// Skip for drawing the current cell if the cell has already been drawn
							// with the same-categorized color (also an AUR color).
							continue;
						}

						// Almost unique rectangles have higher priority to show.
						view.Remove(candidateViewNode);
					}

					var existsCandidate = processedViewNodesMap.ContainsCandidate(candidate, out var identifierKind);
					if (!existsCandidate && !processedViewNodesMap.TryAdd(id, (CellMap.Empty, candidate.AsCandidateMap())))
					{
						var pair = processedViewNodesMap[id];
						pair.Candidates += candidate;
						processedViewNodesMap[id] = pair;
					}

					var node = new CandidateViewNode(id, candidate);
					view.Add(node);
					result.Add(node);
				}
			}
			foreach (var cell in cells)
			{
				if (view.FindCell(cell) is { Identifier: var originalIdentifier } cellViewNode)
				{
					if (originalIdentifier is >= ColorDescriptorAlias.Rectangle1 and <= ColorDescriptorAlias.Rectangle3)
					{
						// Skip for drawing the current cell if the cell has already been drawn
						// with the same-categorized color (also an AUR color).
						continue;
					}

					// Almost unique rectangles have higher priority to show.
					view.Remove(cellViewNode);
				}

				var existsCell = processedViewNodesMap.ContainsCell(cell, out var identifierKind);
				if (!existsCell && !processedViewNodesMap.TryAdd(id, (cell.AsCellMap(), CandidateMap.Empty)))
				{
					var pair = processedViewNodesMap[id];
					pair.Cells += cell;
					processedViewNodesMap[id] = pair;
				}

				var node = new CellViewNode(id, cell);
				view.Add(node);
				result.Add(node);
			}
			urIndex = (urIndex + 1) % 3;
		}

		producedViewNodes = result.AsSpan();
	}
}
