namespace Sudoku.Analytics.StepSearchers;

using ConflictedInfo = ((Cell Left, Cell Right), CellMap InfluencedRange);

/// <summary>
/// Provides with a <b>Remote Pair</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Remote Pair</item>
/// <item>Complex Remote Pair</item>
/// </list>
/// </summary>
[StepSearcher("StepSearcherName_RemotePairStepSearcher", Technique.RemotePair, Technique.ComplexRemotePair)]
public sealed partial class RemotePairStepSearcher : StepSearcher
{
	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		// Test examples:
		// 46+8....2.7+2.....48.+91+4287...+8.+2.14.7..2.9......76+8...+2+8+761.+9+2.+415+4..+2..9+23+98+4..71:315 515 317 517 325 525 327 527 627 341 541 348 548 157 357 657 358 167 967 385 688

		var characters = context.Options.BabaGroupInitialLetter.GetSequence(context.Options.BabaGroupLetterCasing);

		// Try to collect digits that contain at least one unfilled state.
		var mask = (Mask)0;
		for (var digit = 0; digit < 9; digit++)
		{
			if (CandidatesMap[digit])
			{
				mask |= (Mask)(1 << digit);
			}
		}

		// Iterate on each pair of digits, to check whether a remote pair pattern can be formed.
		var accumulator = new HashSet<RemotePairStep>();
		ref readonly var grid = ref context.Grid;
		for (var d1 = 0; d1 < 8; d1++)
		{
			for (var d2 = d1 + 1; d2 < 9; d2++)
			{
				var suchCells = BivalueCells & CandidatesMap[d1] & CandidatesMap[d2];
				if (suchCells.Count < 4)
				{
					continue;
				}

				// Iterate on each component of the graph.
				var pairMask = (Mask)(1 << d1 | 1 << d2);
				var graph = new CellGraph(suchCells);
				foreach (ref readonly var component in graph.Components)
				{
					var parities = Parity.Create(component);
					if (parities.Length == 0)
					{
						continue;
					}

					ref readonly var firstParityPair = ref Parity.Create(component)[0];
					var parity1 = firstParityPair.On.Cells;
					var parity2 = firstParityPair.Off.Cells;

					// Now we should iterate two collections to get contradiction.
					var (conflictedCells, conflictedPair) = (CellMap.Empty, new HashSet<ConflictedInfo>());
					foreach (var cell1 in parity1)
					{
						foreach (var cell2 in parity2)
						{
							var intersection = (cell1.AsCellMap() + cell2).PeerIntersection;
							var currentConflictCells = intersection & (CandidatesMap[d1] | CandidatesMap[d2]);
							if (!!currentConflictCells
								&& !conflictedPair.Any(p => (p.InfluencedRange & currentConflictCells) == currentConflictCells))
							{
								conflictedPair.Add(((cell1, cell2), currentConflictCells));
								conflictedCells |= currentConflictCells;
							}
						}
					}
					if (!conflictedCells)
					{
						// There're no cells can be used as elimination.
						continue;
					}

					var conclusions = new List<Conclusion>();
					foreach (var cell in conflictedCells)
					{
						if ((grid.GetCandidates(cell) >> d1 & 1) != 0)
						{
							conclusions.Add(new(Elimination, cell, d1));
						}
						if ((grid.GetCandidates(cell) >> d2 & 1) != 0)
						{
							conclusions.Add(new(Elimination, cell, d2));
						}
					}
					if (conclusions.Count == 0)
					{
						// There're no eliminations found.
						continue;
					}

					var candidateOffsets = new List<CandidateViewNode>();
					var cellOffsets = new HashSet<CellViewNode>();
					var babaGroupingOffsets = new List<BabaGroupViewNode>();
					foreach (var cell in component.Map)
					{
						candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + d1));
						candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + d2));
					}
					foreach (var ((c1, c2), _) in conflictedPair)
					{
						cellOffsets.Add(new(ColorIdentifier.Auxiliary1, c1));
						cellOffsets.Add(new(ColorIdentifier.Auxiliary1, c2));
					}
					foreach (var cell in parity1)
					{
						babaGroupingOffsets.Add(new(cell, characters[0], pairMask));
					}
					foreach (var cell in parity2)
					{
						babaGroupingOffsets.Add(new(cell, characters[1], pairMask));
					}

					var step = new RemotePairStep(
						conclusions.AsMemory(),
						[[.. candidateOffsets], [.. cellOffsets, .. babaGroupingOffsets]],
						context.Options,
						component.Map,
						parity1.Count != parity2.Count,
						pairMask
					);
					if (context.OnlyFindOne)
					{
						return step;
					}

					accumulator.Add(step);
				}
			}
		}

		if (!context.OnlyFindOne && accumulator.Count != 0)
		{
			context.Accumulator.AddRange(accumulator);
		}
		return null;
	}
}
