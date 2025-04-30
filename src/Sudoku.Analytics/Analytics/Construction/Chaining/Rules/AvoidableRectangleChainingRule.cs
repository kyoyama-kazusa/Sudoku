namespace Sudoku.Analytics.Construction.Chaining.Rules;

/// <summary>
/// Represents a chaining rule on AAR rule (i.e. <see cref="LinkType.AvoidableRectangle"/>).
/// </summary>
/// <seealso cref="LinkType.AvoidableRectangle"/>
public sealed class AvoidableRectangleChainingRule : ChainingRule
{
	/// <inheritdoc/>
	public override void GetLinks(in Grid grid, LinkDictionary strongLinks, LinkDictionary weakLinks, StepGathererOptions options)
	{
		if (options.GetLinkOption(LinkType.AvoidableRectangle) == LinkOption.None)
		{
			return;
		}

		if (grid.Uniqueness != Uniqueness.Unique)
		{
			return;
		}

		// VARIABLE_DECLARATION_BEGIN
		_ = grid is { CandidatesMap: var __CandidatesMap };
		// VARIABLE_DECLARATION_END

		// Weak.
		foreach (var pattern in UniqueRectanglePattern.AllPatterns)
		{
			var urCells = pattern.AsCellMap();
			var (modifiableCellsInPattern, emptyCellsInPattern, isValid) = (CellMap.Empty, CellMap.Empty, true);
			foreach (var cell in urCells)
			{
				switch (grid.GetState(cell))
				{
					case CellState.Modifiable:
					{
						modifiableCellsInPattern.Add(cell);
						break;
					}
					case CellState.Given:
					{
						isValid = false;
						goto OutsideValidityCheck;
					}
					default:
					{
						emptyCellsInPattern.Add(cell);
						break;
					}
				}
			}
		OutsideValidityCheck:
			if (!isValid || modifiableCellsInPattern.Count != 2 || emptyCellsInPattern.Count != 2)
			{
				continue;
			}

			var digit1 = grid.GetDigit(modifiableCellsInPattern[0]);
			var digit2 = grid.GetDigit(modifiableCellsInPattern[1]);
			var digitsMask = (Mask)(1 << digit1 | 1 << digit2);
			if (modifiableCellsInPattern.CanSeeEachOther)
			{
				var cells1 = emptyCellsInPattern & __CandidatesMap[digit1];
				var cells2 = emptyCellsInPattern & __CandidatesMap[digit2];
				if (!cells1 || !cells2)
				{
					continue;
				}

				var node1 = new Node(cells1 * digit1, true);
				var node2 = new Node(cells2 * digit2, false);
				var ar = new AvoidableRectanglePattern(urCells, digitsMask, modifiableCellsInPattern);
				weakLinks.AddEntry(node1, node2, false, ar);
			}
			else if (digit1 == digit2)
			{
				var digitsOtherCellsContained = (Mask)0;
				foreach (var digit in grid[emptyCellsInPattern])
				{
					if ((grid.GetCandidates(emptyCellsInPattern[0]) >> digit & 1) != 0
						&& (grid.GetCandidates(emptyCellsInPattern[1]) >> digit & 1) != 0)
					{
						digitsOtherCellsContained |= (Mask)(1 << digit);
					}
				}
				if (digitsOtherCellsContained == 0)
				{
					continue;
				}

				foreach (var digit in digitsOtherCellsContained)
				{
					var node1 = new Node((emptyCellsInPattern[0] * 9 + digit).AsCandidateMap(), true);
					var node2 = new Node((emptyCellsInPattern[1] * 9 + digit).AsCandidateMap(), false);
					var ar = new AvoidableRectanglePattern(urCells, (Mask)(1 << digit1 | 1 << digit), urCells & ~emptyCellsInPattern);
					weakLinks.AddEntry(node1, node2, false, ar);
				}
			}
		}
	}

	/// <inheritdoc/>
	public override void GetViewNodes(
		in Grid grid,
		Chain pattern,
		View view,
		ProcessedViewNodeMap processedViewNodesMap,
		out ReadOnlySpan<ViewNode> producedViewNodes
	)
	{
		var urIndex = processedViewNodesMap.MaxKeyInRectangle is var key and not WellKnownColorIdentifierKind.Normal
			? (key - WellKnownColorIdentifierKind.Rectangle1 + 1) % 3
			: 0;
		var result = new List<ViewNode>();
		foreach (var link in pattern.Links)
		{
			if (link.GroupedLinkPattern is not AvoidableRectanglePattern { Cells: var urCells })
			{
				continue;
			}

			var id = urIndex + WellKnownColorIdentifierKind.Rectangle1;
			foreach (var cell in urCells)
			{
				if (view.FindCell(cell) is { Identifier: var originalIdentifier } cellViewNode)
				{
					if (originalIdentifier is WellKnownColorIdentifier
						{
							Kind: >= WellKnownColorIdentifierKind.Rectangle1 and <= WellKnownColorIdentifierKind.Rectangle3
						})
					{
						// Skip for drawing the current cell if the cell has already been drawn
						// with the same-categorized color (also an AUR color).
						continue;
					}

					// Almost avoidable rectangles have higher priority to show.
					view.Remove(cellViewNode);
				}

				var existsCell = processedViewNodesMap.ContainsCell(cell, out var identifierKind);
				if (!existsCell && processedViewNodesMap.TryAdd(id, (cell.AsCellMap(), CandidateMap.Empty)))
				{
					var pair = processedViewNodesMap[id];
					pair.Cells.Add(cell);
					processedViewNodesMap[id] = pair;
				}

				var node = new CellViewNode(existsCell ? identifierKind : id, cell);
				view.Add(node);
				result.Add(node);
			}
			urIndex = (urIndex + 1) % 3;
		}
		producedViewNodes = result.AsSpan();
	}
}
