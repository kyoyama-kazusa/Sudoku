namespace Sudoku.Analytics.StepSearchers.LockedSets;

/// <summary>
/// Provides with a <b>Multi-sector Locked Sets</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Multi-sector Locked Sets</item>
/// </list>
/// </summary>
[StepSearcher("StepSearcherName_MultisectorLockedSetsStepSearcher", Technique.MultisectorLockedSets)]
public sealed partial class MultisectorLockedSetsStepSearcher : StepSearcher
{
	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		var linkForEachHouse = (stackalloc Mask[27]);
		linkForEachHouse.Clear();

		var linkForEachDigit = (stackalloc CellMap[9]);
		var canL = (stackalloc CellMap[9]);
		ref readonly var grid = ref context.Grid;
		foreach (var (pattern, rows, columns) in MultisectorLockedSetPattern.Patterns)
		{
			var map = EmptyCells & pattern;
			if (pattern.Count < 12 && pattern.Count - map.Count > 1 || pattern.Count - map.Count > 2)
			{
				continue;
			}

			var n = 0;
			var count = map.Count;
			for (var digit = 0; digit < 9; digit++)
			{
				ref var tempMap = ref linkForEachDigit[digit];
				tempMap = CandidatesMap[digit] & map;
				n += Math.Min(BitOperations.PopCount(tempMap.RowMask), BitOperations.PopCount(tempMap.ColumnMask), BitOperations.PopCount(tempMap.BlockMask));
			}

			if (n == count)
			{
				canL.Clear();

				var conclusions = new List<Conclusion>();
				var candidateOffsets = new List<CandidateViewNode>();
				for (var digit = 0; digit < 9; digit++)
				{
					var q = (Mask)(1 << digit);
					var currentMap = linkForEachDigit[digit];
					var rMask = currentMap.RowMask;
					var cMask = currentMap.ColumnMask;
					var bMask = currentMap.BlockMask;
					var temp = Math.Min(BitOperations.PopCount(rMask), BitOperations.PopCount(cMask), BitOperations.PopCount(bMask));
					var elimMap = CellMap.Empty;
					var check = 0;
					if (BitOperations.PopCount(rMask) == temp)
					{
						check++;
						foreach (var i in rMask)
						{
							var house = i + 9;
							linkForEachHouse[house] |= q;
							elimMap |= (CandidatesMap[digit] & HousesMap[house] & map).PeerIntersection;
						}
					}
					if (BitOperations.PopCount(cMask) == temp)
					{
						check++;
						foreach (var i in cMask)
						{
							var house = i + 18;
							linkForEachHouse[house] |= q;
							elimMap |= (CandidatesMap[digit] & HousesMap[house] & map).PeerIntersection;
						}
					}
					if (BitOperations.PopCount(bMask) == temp)
					{
						check++;
						foreach (var i in bMask)
						{
							linkForEachHouse[i] |= q;
							elimMap |= (CandidatesMap[digit] & HousesMap[i] & map).PeerIntersection;
						}
					}

					elimMap &= CandidatesMap[digit];
					if (!elimMap)
					{
						continue;
					}

					foreach (var cell in elimMap)
					{
						if (map.Contains(cell))
						{
							canL[digit].Add(cell);
						}

						conclusions.Add(new(Elimination, cell, digit));
					}
				}

				if (conclusions.Count == 0)
				{
					continue;
				}

				for (var house = 0; house < 27; house++)
				{
					var linkMask = linkForEachHouse[house];
					if (linkMask == 0)
					{
						continue;
					}

					foreach (var cell in map & HousesMap[house])
					{
						var cands = (Mask)(grid.GetCandidates(cell) & linkMask);
						if (cands == 0)
						{
							continue;
						}

						foreach (var cand in cands)
						{
							if (!canL[cand].Contains(cell))
							{
								candidateOffsets.Add(
									new(
										house switch
										{
											< 9 => ColorIdentifier.Auxiliary2,
											< 18 => ColorIdentifier.Normal,
											_ => ColorIdentifier.Auxiliary1
										},
										cell * 9 + cand
									)
								);
							}
						}
					}
				}

				var step = new MultisectorLockedSetsStep(
					conclusions.AsMemory(),
					[[.. candidateOffsets]],
					context.Options,
					map,
					rows,
					columns,
					grid[map]
				);
				if (context.OnlyFindOne)
				{
					return step;
				}

				context.Accumulator.Add(step);
			}
		}

		return null;
	}
}
