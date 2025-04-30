namespace Sudoku.Analytics.Construction.Chaining.Rules;

/// <summary>
/// Represents a chaining rule on AUR rule (i.e. <see cref="LinkType.UniqueRectangle_DifferentDigitWeak"/>).
/// </summary>
/// <seealso cref="LinkType.UniqueRectangle_DifferentDigitWeak"/>
public sealed class UniqueRectangleDifferentDigitWeakChainingRule : UniqueRectangleChainingRule
{
	/// <inheritdoc/>
	public override void GetLinks(in Grid grid, LinkDictionary strongLinks, LinkDictionary weakLinks, StepGathererOptions options)
	{
		if (options.GetLinkOption(LinkType.UniqueRectangle_DifferentDigitWeak) is not (var linkOption and not LinkOption.None))
		{
			return;
		}

		if (grid.Uniqueness != Uniqueness.Unique)
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

				var urDigitsMask = (Mask)(1 << digitPair[0] | 1 << digitPair[1]);
				var ur = new UniqueRectanglePattern(urCells, urDigitsMask, (Mask)(allDigitsMask & ~urDigitsMask));

				var urCellsContainingOtherDigits = CellMap.Empty;
				foreach (var cell in urCells)
				{
					if ((grid.GetCandidates(cell) & ~urDigitsMask) != 0)
					{
						urCellsContainingOtherDigits.Add(cell);
					}
				}
				if (linkOption == LinkOption.Intersection && !urCellsContainingOtherDigits.IsInIntersection)
				{
					continue;
				}

				foreach (var lockedHouse in urCellsContainingOtherDigits.SharedHouses)
				{
					var cells1 = HousesMap[lockedHouse] & __CandidatesMap[d1] & urCells;
					var cells2 = HousesMap[lockedHouse] & __CandidatesMap[d2] & urCells;
					if (linkOption == LinkOption.Intersection && (cells1.IsInIntersection || cells2.IsInIntersection)
						|| linkOption != LinkOption.Intersection)
					{
						var node1 = new Node(cells1 * d1, true);
						var node2 = new Node(cells2 * d2, false);
						weakLinks.AddEntry(node1, node2, false, ur);
					}
				}
			}
		}
	}
}
