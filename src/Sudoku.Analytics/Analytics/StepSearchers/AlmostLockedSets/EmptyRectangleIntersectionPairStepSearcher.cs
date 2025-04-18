namespace Sudoku.Analytics.StepSearchers.AlmostLockedSets;

/// <summary>
/// Provides with an <b>Empty Rectangle Intersection Pair</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Empty Rectangle Intersection Pair</item>
/// </list>
/// </summary>
[StepSearcher("StepSearcherName_EmptyRectangleIntersectionPairStepSearcher", Technique.EmptyRectangleIntersectionPair)]
public sealed partial class EmptyRectangleIntersectionPairStepSearcher : StepSearcher
{
	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		ref readonly var grid = ref context.Grid;
		for (var (i, length) = (0, BivalueCells.Count); i < length - 1; i++)
		{
			var c1 = BivalueCells[i];
			var mask = grid.GetCandidates(c1);
			var d1 = BitOperations.TrailingZeroCount(mask);
			var d2 = mask.GetNextSet(d1);
			for (var j = i + 1; j < length; j++)
			{
				var c2 = BivalueCells[j];

				// Check the candidates that cell holds is totally same with 'c1'.
				if (grid.GetCandidates(c2) != mask)
				{
					continue;
				}

				// Check the two cells are not in same house index.
				if ((c1.AsCellMap() + c2).FirstSharedHouse != 32)
				{
					continue;
				}

				var block1 = c1.ToHouse(HouseType.Block);
				var block2 = c2.ToHouse(HouseType.Block);
				if (block1 % 3 == block2 % 3 || block1 / 3 == block2 / 3)
				{
					continue;
				}

				// Check the block that two cells both see.
				var interMap = (c1.AsCellMap() + c2).PeerIntersection;
				var unionMap = (PeersMap[c1] | PeersMap[c2]) + c1 + c2;
				foreach (var interCell in interMap)
				{
					var block = interCell.ToHouse(HouseType.Block);
					ref readonly var houseMap = ref HousesMap[block];
					var checkingMap = houseMap & ~unionMap & houseMap;
					if (checkingMap & CandidatesMap[d1] || checkingMap & CandidatesMap[d2])
					{
						continue;
					}

					// Check whether two digits are both in the same empty rectangle.
					var b1 = c1.ToHouse(HouseType.Block);
					var b2 = c2.ToHouse(HouseType.Block);
					var erMap = unionMap & houseMap & ~interMap & (CandidatesMap[d1] | CandidatesMap[d2]);
					var m = grid[erMap];
					if ((m & mask) != mask)
					{
						continue;
					}

					// Check eliminations.
					var conclusions = new List<Conclusion>();
					var z = (interMap & houseMap)[0];
					var c1Map = HousesMap[(z.AsCellMap() + c1).SharedLine];
					var c2Map = HousesMap[(z.AsCellMap() + c2).SharedLine];
					foreach (var elimCell in (c1Map | c2Map) - c1 - c2 & ~erMap)
					{
						if (CandidatesMap[d1].Contains(elimCell))
						{
							conclusions.Add(new(Elimination, elimCell, d1));
						}
						if (CandidatesMap[d2].Contains(elimCell))
						{
							conclusions.Add(new(Elimination, elimCell, d2));
						}
					}
					if (conclusions.Count == 0)
					{
						continue;
					}

					var candidateOffsets = new List<CandidateViewNode>();
					foreach (var digit in grid.GetCandidates(c1))
					{
						candidateOffsets.Add(new(ColorIdentifier.Normal, c1 * 9 + digit));
					}
					foreach (var digit in grid.GetCandidates(c2))
					{
						candidateOffsets.Add(new(ColorIdentifier.Normal, c2 * 9 + digit));
					}
					foreach (var cell in erMap)
					{
						foreach (var digit in (Mask)(grid.GetCandidates(cell) & (Mask)(1 << d1 | 1 << d2)))
						{
							candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + digit));
						}
					}

					// Special: we should treat the intersection cell at the block same as 'block' as one of the ER pattern.
					// This fixes for issue #758: https://github.com/KyouyamaKazusa0805/Sudoku/issues/758
					foreach (var cannibalCell in HousesMap[block] & interMap)
					{
						foreach (var digit in (Mask)(grid.GetCandidates(cannibalCell) & (Mask)(1 << d1 | 1 << d2)))
						{
							candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cannibalCell * 9 + digit));
						}
					}

					var step = new EmptyRectangleIntersectionPairStep(
						conclusions.AsMemory(),
						[[.. candidateOffsets, new HouseViewNode(ColorIdentifier.Normal, block)]],
						context.Options,
						c1,
						c2,
						block,
						d1,
						d2
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
