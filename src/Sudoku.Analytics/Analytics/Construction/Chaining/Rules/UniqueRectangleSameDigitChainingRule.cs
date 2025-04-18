namespace Sudoku.Analytics.Construction.Chaining.Rules;

/// <summary>
/// Represents a chaining rule on AUR rule (i.e. <see cref="LinkType.UniqueRectangle_SameDigit"/>).
/// </summary>
/// <seealso cref="LinkType.UniqueRectangle_SameDigit"/>
public sealed class UniqueRectangleSameDigitChainingRule : UniqueRectangleChainingRule
{
	/// <inheritdoc/>
	public override void GetLinks(in Grid grid, LinkDictionary strongLinks, LinkDictionary weakLinks, StepGathererOptions options)
	{
		if (options.GetLinkOption(LinkType.UniqueRectangle_SameDigit) is not (var linkOption and not LinkOption.None))
		{
			return;
		}

		if (grid.GetUniqueness() != Uniqueness.Unique)
		{
			return;
		}

		// VARIABLE_DECLARATION_BEGIN
		_ = grid is { EmptyCells: var __EmptyCells, CandidatesMap: var __CandidatesMap };
		// VARIABLE_DECLARATION_END

		foreach (var pattern in UniqueRectanglePattern.AllPatterns)
		{
			var urCells = pattern.AsCellMap();
			if ((__EmptyCells & urCells) != urCells)
			{
				// Four cells must be empty.
				continue;
			}

			var allDigitsMask = grid[urCells];
			foreach (var digitPair in grid[urCells].AllSets.GetSubsets(2))
			{
				var (d1, d2) = (digitPair[0], digitPair[1]);
				if (!UniqueRectanglePattern.CanMakeDeadlyPattern(grid, d1, d2, pattern))
				{
					continue;
				}

				var urDigitsMask = (Mask)(1 << d1 | 1 << d2);
				var otherDigitsMask = (Mask)(allDigitsMask & ~urDigitsMask);
				if (!Mask.IsPow2(otherDigitsMask))
				{
					continue;
				}

				var ur = new UniqueRectanglePattern(urCells, urDigitsMask, otherDigitsMask);
				var otherOnlyDigit = Mask.Log2(otherDigitsMask);
				var cellsContainingThisDigit = __CandidatesMap[otherOnlyDigit] & urCells;
				var rowsSpanned = cellsContainingThisDigit.RowMask << 9;
				if (BitOperations.PopCount(rowsSpanned) == 2)
				{
					var row1 = HouseMask.TrailingZeroCount(rowsSpanned);
					var row2 = rowsSpanned.GetNextSet(row1);
					var cells1 = cellsContainingThisDigit & HousesMap[row1];
					var cells2 = cellsContainingThisDigit & HousesMap[row2];
					if (linkOption == LinkOption.Intersection && (cells1.IsInIntersection || cells2.IsInIntersection)
						|| linkOption != LinkOption.Intersection)
					{
						var node1 = new Node(cells1 * otherOnlyDigit, false);
						var node2 = new Node(cells2 * otherOnlyDigit, true);
						strongLinks.AddEntry(node1, node2, true, ur);
					}
				}

				var columnsSpanned = cellsContainingThisDigit.ColumnMask << 18;
				if (BitOperations.PopCount(columnsSpanned) == 2)
				{
					var column1 = HouseMask.TrailingZeroCount(columnsSpanned);
					var column2 = columnsSpanned.GetNextSet(column1);
					var cells3 = cellsContainingThisDigit & HousesMap[column1];
					var cells4 = cellsContainingThisDigit & HousesMap[column2];
					if (linkOption == LinkOption.Intersection && (cells3.IsInIntersection || cells4.IsInIntersection)
						|| linkOption != LinkOption.Intersection)
					{
						var node3 = new Node(cells3 * otherOnlyDigit, false);
						var node4 = new Node(cells4 * otherOnlyDigit, true);
						strongLinks.AddEntry(node3, node4, false, ur);
					}
				}
			}
		}
	}

	/// <inheritdoc/>
	public override void GetLoopConclusions(in Grid grid, ReadOnlySpan<Link> links, ref ConclusionSet conclusions)
	{
		// VARIABLE_DECLARATION_BEGIN
		_ = 42;
		// VARIABLE_DECLARATION_END

		var result = ConclusionSet.Empty;
		foreach (var element in links)
		{
			if (element is (_, _, true, UniqueRectanglePattern(var cells, var digitsMask)))
			{
				result.AddRange(EliminationCalculator.UniqueRectangle.GetConclusions(cells, digitsMask, grid));
			}
		}
		conclusions |= result;
	}
}
