namespace Sudoku.Analytics.Construction.Chaining.Rules;

/// <summary>
/// Represents a chaining rule on AUR rule (i.e. <see cref="LinkType.UniqueRectangle_SingleSideExternal"/>).
/// </summary>
/// <seealso cref="LinkType.UniqueRectangle_SingleSideExternal"/>
public sealed class UniqueRectangleSingleSideExternalChainingRule : UniqueRectangleChainingRule
{
	/// <inheritdoc/>
	public override void GetLinks(in Grid grid, LinkDictionary strongLinks, LinkDictionary weakLinks, StepGathererOptions options)
	{
		if (options.GetLinkOption(LinkType.UniqueRectangle_SingleSideExternal) is not (var linkOption and not LinkOption.None))
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

				var urDigitsMask = (Mask)(1 << digitPair[0] | 1 << digitPair[1]);
				var otherDigitsMask = (Mask)(allDigitsMask & ~urDigitsMask);
				if (Mask.PopCount(otherDigitsMask) < 2)
				{
					continue;
				}

				var ur = new UniqueRectanglePattern(urCells, urDigitsMask, otherDigitsMask);

				var urCellsContainingOtherDigits = CellMap.Empty;
				foreach (var cell in urCells)
				{
					if ((grid.GetCandidates(cell) & otherDigitsMask) != 0)
					{
						urCellsContainingOtherDigits.Add(cell);
					}
				}
				if (linkOption == LinkOption.Intersection && !urCellsContainingOtherDigits.IsInIntersection)
				{
					return;
				}

				foreach (var lockedHouse in urCellsContainingOtherDigits.SharedHouses)
				{
					var cells1 = HousesMap[lockedHouse] & __CandidatesMap[d1] & ~urCells;
					var cells2 = HousesMap[lockedHouse] & __CandidatesMap[d2] & ~urCells;
					if (linkOption == LinkOption.Intersection && (cells1.IsInIntersection || cells2.IsInIntersection)
						|| linkOption != LinkOption.Intersection)
					{
						var node1 = new Node(cells1 * d1, false);
						var node2 = new Node(cells2 * d2, true);
						strongLinks.AddEntry(node1, node2, true, ur);
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

		// This example will contain extra eliminations:
		//   4.........2..5...6..8..23...9..8.7....59.+7.6.7+8.3.4..2.7..4.6....3..1..91......8.:548 549 167 168 695
		//
		// Extra eliminations should be r7c8(25):
		//   .-----------------------.-----------------------.------------------------.
		//   | 4      156(3)  679-1  | 1678   1679(3)  689-3 | 12589  12579    1578   |
		//   | 9(3)   2       79-1   | 1478   5        389   | 1489   1479     6      |
		//   | 569    156     8      | 1467   1679     2     | 3      14579    1457   |
		//   |-----------------------+-----------------------+------------------------|
		//   | (236)  9       (1246) | 1256   8        56    | 7      (134)    (134)  |
		//   | (23)   34-1    5      | 9      12       7     | 148    6        1348   |
		//   | 7      8       (16)   | 3      16       4     | 59     59       2      |
		//   |-----------------------+-----------------------+------------------------|
		//   | 2589   7       29     | 258    4        3589  | 6      -25(13)  5(13)  |
		//   | 2568   456     3      | 25678  267      1     | 245    2457     9      |
		//   | 1      456     2469   | 2567   279(3)   569-3 | 245    8        457(3) |
		//   '-----------------------'-----------------------'------------------------'
		//
		// However I don't have any unified rules to calculate such eliminations
		// because AUR eliminations should be treated as "forced" ones, meaning we should check them by supposing it with "true"
		// to make a contradiction.
		// This will be implemented in the future.
		//
		// Binds with issue #680: https://github.com/KyouyamaKazusa0805/Sudoku/issues/680
		base.GetLoopConclusions(grid, links, ref conclusions);
	}
}
