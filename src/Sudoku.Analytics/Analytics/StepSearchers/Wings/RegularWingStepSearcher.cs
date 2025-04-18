namespace Sudoku.Analytics.StepSearchers.Wings;

/// <summary>
/// Provides with a <b>Regular Wing</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>XY-Wing</item>
/// <item>XYZ-Wing</item>
/// <item>WXYZ-Wing</item>
/// <item>VWXYZ-Wing</item>
/// <item>UVWXYZ-Wing</item>
/// <item>TUVWXYZ-Wing</item>
/// <item>STUVWXYZ-Wing</item>
/// <item>RSTUVWXYZ-Wing</item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_RegularWingStepSearcher",
	Technique.XyWing, Technique.XyzWing, Technique.WxyzWing, Technique.VwxyzWing,
	Technique.UvwxyzWing, Technique.TuvwxyzWing, Technique.StuvwxyzWing, Technique.RstuvwxyzWing,
	Technique.IncompleteWxyzWing, Technique.IncompleteVwxyzWing, Technique.IncompleteUvwxyzWing,
	Technique.IncompleteTuvwxyzWing, Technique.IncompleteStuvwxyzWing, Technique.IncompleteRstuvwxyzWing)]
public sealed partial class RegularWingStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates the maximum number of pivots to be searched for. The default value is 3.
	/// </summary>
	[SettingItemName(SettingItemNames.MaxSizeOfRegularWing)]
	public int MaxSearchingPivotsCount { get; set; } = 3;


	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		// Test examples:
		// VWXYZ-Wing
		// 9....2.+1+7.+12..7.9+6..6..+1.28+6+8+5+71+9+2+434..258..12.+1.4....16....7...5.8..1...+2.1....4:413 953 362 567 667 668 474 574 975 383 397 597
		// 8..+6154..1+6.9+4...5.+4+5.7.6+1.+6..+4...31.+51.6.2+4.49...+1.....4+53....5....6..4..618+4..7:327 334 754 356 979 987 988

		// Iterate on the size.
		// Note that the greatest size is determined by two factors: the size that you specified
		// and the number of bi-value cells in the grid.
		ref readonly var grid = ref context.Grid;
		for (var size = 3; size <= Math.Min(MaxSearchingPivotsCount, BivalueCells.Count); size++)
		{
			// Iterate on each pivot cell.
			foreach (var pivot in EmptyCells)
			{
				var mask = grid.GetCandidates(pivot);
				var candsCount = BitOperations.PopCount(mask);
				if (candsCount != size && candsCount != size - 1)
				{
					// Candidates are not enough.
					continue;
				}

				var map = PeersMap[pivot] & BivalueCells;
				if (map.Count < size - 1)
				{
					// Bi-value cells are not enough.
					continue;
				}

				// Iterate on each cell combination.
				foreach (ref readonly var cells in map & size - 1)
				{
					// Check duplicate.
					// If two cells contain same candidates, the wing can't be formed.
					var flag = false;
					for (var (i, length) = (0, cells.Count); i < length - 1; i++)
					{
						for (var j = i + 1; j < length; j++)
						{
							if (grid[cells[i]] == grid[cells[j]])
							{
								flag = true;
								goto CheckWhetherTwoCellsContainSameCandidateKind;
							}
						}
					}

				CheckWhetherTwoCellsContainSameCandidateKind:
					if (flag)
					{
						continue;
					}

					var union = mask;
					var inter = (Mask)(Grid.MaxCandidatesMask & mask);
					foreach (var cell in cells)
					{
						var m = grid.GetCandidates(cell);
						union |= m;
						inter &= m;
					}

					if (BitOperations.PopCount(union) != size || inter != 0 && !BitOperations.IsPow2(inter))
					{
						continue;
					}

					// Get the Z digit (The digit to be removed).
					var isIncomplete = inter == 0;
					var interWithoutPivot = (Mask)(union & ~grid.GetCandidates(pivot));
					var maskToCheck = isIncomplete ? interWithoutPivot : inter;
					if (!BitOperations.IsPow2(maskToCheck))
					{
						continue;
					}

					// The pattern should be "az, bz, cz, dz, ... , abcd(z)".
					var zDigit = BitOperations.TrailingZeroCount(maskToCheck);
					var petals = cells;
					if ((petals + pivot & CandidatesMap[zDigit]).Count != (isIncomplete ? size - 1 : size))
					{
						continue;
					}

					// Check elimination map.
					var elimMap = petals.PeerIntersection;
					if (!isIncomplete)
					{
						elimMap &= PeersMap[pivot];
					}
					elimMap &= CandidatesMap[zDigit];
					if (!elimMap)
					{
						continue;
					}

					// Collect for highlight candidates.
					var candidateOffsets = new List<CandidateViewNode>(6);
					foreach (var cell in cells)
					{
						foreach (var digit in grid.GetCandidates(cell))
						{
							candidateOffsets.Add(new(digit == zDigit ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal, cell * 9 + digit));
						}
					}
					foreach (var digit in grid.GetCandidates(pivot))
					{
						candidateOffsets.Add(new(digit == zDigit ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal, pivot * 9 + digit));
					}

					var step = new RegularWingStep(
						(from cell in elimMap select new Conclusion(Elimination, cell, zDigit)).ToArray(),
						[[.. candidateOffsets]],
						context.Options,
						pivot,
						BitOperations.PopCount(mask),
						union,
						in petals
					);
					if (context.OnlyFindOne)
					{
						return step;
					}

					context.Accumulator.Add(step);
				}
			}
		}

		return null;
	}
}
