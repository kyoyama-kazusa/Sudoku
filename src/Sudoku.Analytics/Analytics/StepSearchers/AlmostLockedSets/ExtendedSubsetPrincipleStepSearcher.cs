namespace Sudoku.Analytics.StepSearchers.AlmostLockedSets;

/// <summary>
/// Provides with an <b>Extended Subset Principle</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Extended Subset Principle</item>
/// </list>
/// </summary>
[StepSearcher("StepSearcherName_ExtendedSubsetPrincipleStepSearcher", Technique.ExtendedSubsetPrinciple)]
public sealed partial class ExtendedSubsetPrincipleStepSearcher : StepSearcher
{
	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		// Must starts with an extended WXYZ-Wing (4 pattern cells and at least one elimination).
		if (EmptyCells.Count < 5)
		{
			return null;
		}

		// A valid ESP must starts with a locked candidate, in order to make no duplicate.
		ref readonly var grid = ref context.Grid;
		var list = new List<CellMap>(7);
		var results = new HashSet<CellMap>();
		foreach (var kvp in Miniline.Map)
		{
			ref readonly var key = ref kvp.KeyRef;
			ref readonly var value = ref kvp.ValueRef;
			var ((baseSet, coverSet), (a, b, c, _)) = (key, value);

			var emptyCellsInInterMap = c & EmptyCells;

			// Add all combinations into the collection.
			list.Clear();
			list.AddRangeRef(emptyCellsInInterMap | emptyCellsInInterMap.Count);

			// Iterate on each intersection combination.
			foreach (ref readonly var currentInterMap in list.AsSpan())
			{
				var selectedInterMask = grid[currentInterMap];
				if (BitOperations.PopCount(selectedInterMask) <= currentInterMap.Count + 1)
				{
					// The intersection combination is an ALS or a normal subset, which is invalid in ESPs.
					continue;
				}

				var blockMap = (b | c & ~currentInterMap) & EmptyCells;
				var lineMap = a & EmptyCells;

				// Iterate on the number of the cells that should be selected in block.
				for (var i = 1; i < blockMap.Count; i++)
				{
					// Iterate on each combination in block.
					foreach (ref readonly var currentBlockMap in blockMap & i)
					{
						// Iterate on the number of the cells that should be selected in line.
						var blockMask = grid[currentBlockMap];
						for (var j = 1; j <= 9 - i - currentInterMap.Count && j <= lineMap.Count; j++)
						{
							// Iterate on each combination in line.
							foreach (ref readonly var currentLineMap in lineMap & j)
							{
								var lineMask = grid[currentLineMap];

								// Calculates for z-digit.
								// A z-digit is a digit that is appeared in two different houses,
								// which will effect pattern forming of "Disjointed Distribute Subset".
								// A sketch is like this:
								// .-----------.-----.
								// | abd  abcd | abc |
								// | abcd      |     |
								// '-----------'-----'
								// where the digit 'c' has spanned two different houses (both block and line).

								var zDigitsMask = (Mask)(blockMask & lineMask);
								if (!BitOperations.IsPow2(zDigitsMask))
								{
									// Z-digit cannot be used as elimination,
									// like multiple z-digits found, or an SdC pattern that has already handled
									// by another step searcher type.
									continue;
								}

								var zDigit = BitOperations.Log2(zDigitsMask);
								var isolatedDigitsMask = (Mask)(selectedInterMask & ~(blockMask | lineMask));
								var digitsCount = BitOperations.PopCount((Mask)(blockMask | lineMask) | isolatedDigitsMask);
								var cellsCount = currentInterMap.Count + i + j;
								var pattern = currentBlockMap | currentLineMap | currentInterMap;
								var elimMap = pattern % CandidatesMap[zDigit];
								if (!elimMap)
								{
									// Possible eliminations in both modes are not found.
									continue;
								}

								if (cellsCount != digitsCount)
								{
									// The number of cells must be equal to the number of digits appearing in them.
									continue;
								}

								var candidateOffsets = new List<CandidateViewNode>();
								foreach (var cell in pattern)
								{
									foreach (var digit in grid.GetCandidates(cell))
									{
										candidateOffsets.Add(
											new(
												digit == zDigit ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal,
												cell * 9 + digit
											)
										);
									}
								}

								if (results.Add(pattern))
								{
									var step = new ExtendedSubsetPrincipleStep(
										(from cell in elimMap select new Conclusion(Elimination, cell, zDigit)).ToArray(),
										[[.. candidateOffsets]],
										context.Options,
										pattern,
										(Mask)(blockMask | lineMask),
										zDigit
									);
									if (context.OnlyFindOne)
									{
										return step;
									}

									context.Accumulator.Add(step);
								}
							}
						}
					}
				}
			}
		}

		return null;
	}
}
