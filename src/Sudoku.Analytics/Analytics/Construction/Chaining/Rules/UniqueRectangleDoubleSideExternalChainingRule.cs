namespace Sudoku.Analytics.Construction.Chaining.Rules;

/// <summary>
/// Represents a chaining rule on AUR rule (i.e. <see cref="LinkType.UniqueRectangle_DoubleSideExternal"/>).
/// </summary>
/// <seealso cref="LinkType.UniqueRectangle_DoubleSideExternal"/>
public sealed class UniqueRectangleDoubleSideExternalChainingRule : UniqueRectangleChainingRule
{
	/// <inheritdoc/>
	public override void GetLinks(in Grid grid, LinkDictionary strongLinks, LinkDictionary weakLinks, StepGathererOptions options)
	{
		if (options.GetLinkOption(LinkType.UniqueRectangle_DoubleSideExternal) is not (var linkOption and not LinkOption.None))
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
				var otherDigitsMask = (Mask)(allDigitsMask & ~urDigitsMask);
				if (BitOperations.PopCount(otherDigitsMask) < 2)
				{
					continue;
				}

				var ur = new UniqueRectanglePattern(urCells, urDigitsMask, otherDigitsMask);

				foreach (var otherDigit in otherDigitsMask)
				{
					var cells = __CandidatesMap[otherDigit] & urCells;
					if (!cells.IsInIntersection)
					{
						continue;
					}

					// Determine whether such cells only contain UR digits and the selected digit.
					var unrelatedDigitsMask = (Mask)0;
					foreach (var cell in cells)
					{
						unrelatedDigitsMask |= (Mask)(grid.GetCandidates(cell) & ~urDigitsMask & ~(1 << otherDigit));
					}
					if (unrelatedDigitsMask != 0)
					{
						continue;
					}

					// Determine whether the other cells are in an intersection.
					var urOtherSideCells = urCells & ~cells;
					if (linkOption == LinkOption.Intersection && !urOtherSideCells.IsInIntersection)
					{
						continue;
					}

					var node1 = new Node(cells * otherDigit, false);
					foreach (var lockedHouse in urOtherSideCells.SharedHouses)
					{
						var lockedUrDigitsMask = (Mask)0;
						foreach (var digit in urDigitsMask)
						{
							var lockedCells = HousesMap[lockedHouse] & __CandidatesMap[digit];
							if (lockedCells == urOtherSideCells)
							{
								lockedUrDigitsMask |= (Mask)(1 << digit);
							}
						}

						if (lockedUrDigitsMask == 0 || urDigitsMask == lockedUrDigitsMask)
						{
							// No enough digits can be used.
							continue;
						}

						var lastUrDigit = BitOperations.Log2((Mask)(urDigitsMask & ~lockedUrDigitsMask));
						var otherCellsContainingLastUrDigit = HousesMap[lockedHouse] & __CandidatesMap[lastUrDigit] & ~urCells;
						if (linkOption == LinkOption.Intersection && !otherCellsContainingLastUrDigit.IsInIntersection)
						{
							continue;
						}

						var node2 = new Node(otherCellsContainingLastUrDigit * lastUrDigit, true);
						strongLinks.AddEntry(node1, node2, true, ur);
					}
				}
			}
		}
	}
}
