namespace Sudoku.Analytics.PatternSearchers;

/// <summary>
/// Represents a pattern searcher on XYZ-Wings.
/// </summary>
public sealed class XyzWingPatternSearcher : PatternSearcher<XyzWingPattern>
{
	/// <inheritdoc/>
	/// <remarks><i>This method cannot be used as caching because most of callers have already been cached.</i></remarks>
	public override ReadOnlySpan<XyzWingPattern> Search(in Grid grid)
	{
		_ = grid is { BivalueCells: var bivalueCells, EmptyCells: var emptyCells };

		// Collect for tri-value cells.
		var trivalueCells = CellMap.Empty;
		foreach (var cell in emptyCells & ~bivalueCells)
		{
			if (BitOperations.PopCount(grid.GetCandidates(cell)) == 3)
			{
				trivalueCells.Add(cell);
			}
		}

		// Iterate on each pivot cell to get all possible results.
		var result = new List<XyzWingPattern>();
		foreach (var pivot in trivalueCells)
		{
			var digitsMaskPivot = grid.GetCandidates(pivot);

			// Fetch for two cells from two different houses.
			foreach (var housePair in HouseTypes.AsReadOnlySpan().GetSubsets(2))
			{
				var house1 = pivot.ToHouse(housePair[0]);
				var house2 = pivot.ToHouse(housePair[1]);
				var bivalueCellsFromHouse1 = bivalueCells & HousesMap[house1];
				var bivalueCellsFromHouse2 = bivalueCells & HousesMap[house2];
				if (!bivalueCellsFromHouse1 || !bivalueCellsFromHouse2)
				{
					continue;
				}

				foreach (var leafCell1 in bivalueCellsFromHouse1)
				{
					var digitsMask1 = grid.GetCandidates(leafCell1);
					foreach (var leafCell2 in bivalueCellsFromHouse2)
					{
						var pattern = pivot.AsCellMap() + leafCell1 + leafCell2;
						if (pattern.FirstSharedHouse != 32)
						{
							continue;
						}

						var digitsMask2 = grid.GetCandidates(leafCell2);

						// Check whether 3 cells intersected by one common digit, and contains 3 different digits.
						var unionedDigitsMask = (Mask)((Mask)(digitsMaskPivot | digitsMask1) | digitsMask2);
						if (BitOperations.PopCount(unionedDigitsMask) != 3
							|| unionedDigitsMask != digitsMaskPivot
							|| !BitOperations.IsPow2((Mask)(digitsMaskPivot & digitsMask1 & digitsMask2)))
						{
							continue;
						}

						var intersectedDigit = BitOperations.Log2((Mask)(digitsMaskPivot & digitsMask1 & digitsMask2));
						result.Add(new(pivot, leafCell1, leafCell2, house1, house2, unionedDigitsMask, intersectedDigit));
					}
				}
			}
		}
		return result.AsSpan();
	}
}
