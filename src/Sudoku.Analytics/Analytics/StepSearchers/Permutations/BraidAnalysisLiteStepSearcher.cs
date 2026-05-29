namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with a <b>Braid Analysis</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Braid Analysis Lite</item>
/// </list>
/// </summary>
[StepSearcher("StepSearcherName_BraidAnalysisLiteStepSearcher", Technique.BraidAnalysisLite)]
public sealed partial class BraidAnalysisLiteStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates braiding indices.
	/// </summary>
	private static readonly int[][] BraidingIndices = [[0, 4, 8], [0, 7, 5], [1, 3, 8], [1, 5, 6], [2, 3, 7], [2, 4, 6]];

	/// <summary>
	/// Indicates transition path indices.
	/// </summary>
	private static readonly int[][] TransitionIndices = [[0, 1, 2], [0, 2, 1], [1, 0, 2], [1, 2, 0], [2, 0, 1], [2, 1, 0]];


	/// <inheritdoc/>
	protected internal override unsafe Step? Collect(ref StepAnalysisContext context)
	{
		ref readonly var grid = ref context.Grid;
		var cellsGroup = stackalloc CellMap*[3];
		var transitionPath = stackalloc CellMap*[3];

		// Iterate on each chute.
		for (var chuteIndex = 0; chuteIndex < 6; chuteIndex++)
		{
			// Represents a chute cells.
			var chuteCells = Segments.SegmentsGroupedByChuteIndex[chuteIndex].AsReadOnlySpan();

			// Iterate on each combination of triplets of braiding indices.
			foreach (var indices in BraidingIndices)
			{
				// Gets segment cells in this group to be checked.
				var cells1 = chuteCells[indices[0]];
				var cells2 = chuteCells[indices[1]];
				var cells3 = chuteCells[indices[2]];
				cellsGroup[0] = &cells1;
				cellsGroup[1] = &cells2;
				cellsGroup[2] = &cells3;

				// Check whether every segment has enough cells to be iterated.
				if (!CheckWhetherEverySegmentHasAtLeastTwoEmptyCells(grid, cellsGroup, chuteCells, indices))
				{
					continue;
				}

				// Then we should iterate on each combination of cells.
				foreach (ref readonly var a in cells1 & 2)
				{
					// The digits appeared in group 'a'.
					var digitsMask = grid[a];

					// Check whether digits are enough to be checked.
					if (BitOperations.PopCount((uint)digitsMask) <= 2)
					{
						continue;
					}

					// The other unused cell in this segment shouldn't contain same digits to be checked.
					if ((grid.GetCandidates((chuteCells[indices[0]] & ~a)[0]) & digitsMask) != 0)
					{
						continue;
					}

					foreach (ref readonly var b in cells2 & 2)
					{
						// The digits appeared in group 'b'.
						var bDigitsMask = grid[b];

						// Check 'a = b' on digits appeared.
						if (digitsMask != bDigitsMask)
						{
							continue;
						}

						// The other unused cell in this segment shouldn't contain same digits to be checked.
						if ((grid.GetCandidates((chuteCells[indices[1]] & ~b)[0]) & digitsMask) != 0)
						{
							continue;
						}

						foreach (ref readonly var c in cells3 & 2)
						{
							// The digits appeared in group 'c'.
							var cDigitsMask = grid[c];

							// Check 'a = b = c' on digits appeared.
							if (digitsMask != cDigitsMask)
							{
								continue;
							}

							// The other unused cell in this segment shouldn't contain same digits to be checked.
							if ((grid.GetCandidates((chuteCells[indices[2]] & ~c)[0]) & digitsMask) != 0)
							{
								continue;
							}

							// Here, all variables 'aDigitsMask', 'bDigitsMask', 'cDigitsMask' are equal,
							// and the number of digits are at least 3 (having enough digits to be checked).

							// Check whether the cell group is valid or not.
							if (!CheckWhetherAll3TransitionPathsDifferentStartAreOkay(grid, digitsMask, transitionPath, cellsGroup))
							{
								continue;
							}

							// Construct a table of valid digit combinations.
							var allDigitPairs = digitsMask.AllSets & 2;
							var validDigitPairCombinationsDictionary = new Dictionary<Digit, HashSet<Mask>>();
							foreach (var digitPair in allDigitPairs)
							{
								var d1 = digitPair[0];
								var d2 = digitPair[1];
								var mask = (Mask)(1 << d1 | 1 << d2);
								if (!validDigitPairCombinationsDictionary.TryAdd(d1, [mask]))
								{
									validDigitPairCombinationsDictionary[d1].Add(mask);
								}
								if (!validDigitPairCombinationsDictionary.TryAdd(d2, [mask]))
								{
									validDigitPairCombinationsDictionary[d2].Add(mask);
								}
							}

							// Now enumerate pairs can be filled in fact grid
							// for pair of cells in 'a', 'b' and 'c'.
							foreach (var digitPair in allDigitPairs)
							{
								var d1 = digitPair[0];
								var d2 = digitPair[1];
								var mask = (Mask)(1 << d1 | 1 << d2);

								// Check whether all 3 cell pairs can fill with that digit pair.
								var all3CellPairsCanFillWithDigitPair = true;
								for (var i = 0; i < 3; i++)
								{
									ref readonly var cells = ref i == 0 ? ref a : ref i == 1 ? ref b : ref c;
									var isCellPairValid = false;
									foreach (var (cell1, cell2) in ((cells[0], cells[1]), (cells[1], cells[0])))
									{
										if ((grid.GetCandidates(cell1) >> d1 & 1) != 0
											&& (grid.GetCandidates(cell2) >> d2 & 1) != 0)
										{
											isCellPairValid = true;
											break;
										}
									}
									if (!isCellPairValid)
									{
										all3CellPairsCanFillWithDigitPair = false;
										break;
									}
								}
								if (!all3CellPairsCanFillWithDigitPair)
								{
									// This digit pair is invalid. We should remove from dictionary.
									validDigitPairCombinationsDictionary[d1].Remove(mask);
									validDigitPairCombinationsDictionary[d2].Remove(mask);
								}
							}

							// Check whether at least one digit has no valid pair combinations in dictionary.
							// If so, the digit is impossible to be valid, we can remove it.
							var conclusions = new List<Conclusion>();
							var invalidDigitsMask = (Mask)0;
							foreach (var digit in validDigitPairCombinationsDictionary.Keys)
							{
								if (validDigitPairCombinationsDictionary[digit].Count != 0)
								{
									continue;
								}

								invalidDigitsMask |= (Mask)(1 << digit);

								// Check eliminations.
								foreach (var cell in (a | b | c) & CandidatesMap[digit])
								{
									conclusions.Add(new(Elimination, cell, digit));
								}
							}
							if (conclusions.Count == 0)
							{
								// No eliminations found.
								continue;
							}

							var unknownCharacterSequence = context.Options.BabaGroupInitialLetter.GetSequence(context.Options.BabaGroupLetterCase);
							var step = new BraidAnalysisLiteStep(
								conclusions.AsMemory(),
								[
									[
										.. from cell in a select new CellViewNode(ColorDescriptorAlias.Normal, cell),
										.. from cell in b select new CellViewNode(ColorDescriptorAlias.Normal, cell),
										.. from cell in c select new CellViewNode(ColorDescriptorAlias.Normal, cell)
									]
								],
								context.Options,
								digitsMask,
								a,
								b,
								c,
								invalidDigitsMask
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
		return null;
	}


	/// <summary>
	/// Check whether every segment has enough cells to be iterated.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="cellsGroup">The cell groups.</param>
	/// <param name="chuteCells">The chute cells.</param>
	/// <param name="indices">The chosen chute indices.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	private static unsafe bool CheckWhetherEverySegmentHasAtLeastTwoEmptyCells(
		in Grid grid,
		CellMap** cellsGroup,
		ReadOnlySpan<CellMap> chuteCells,
		int[] indices
	)
	{
		// Check whether all 3 segments contain at least 2 empty cells.
		for (var i = 0; i < 3; i++)
		{
			ref var cellsToCheck = ref *cellsGroup[i];
			var counter = 0;
			foreach (var cell in chuteCells[indices[i]])
			{
				if (grid.GetState(cell) != CellState.Empty)
				{
					cellsToCheck -= cell;
					if (++counter >= 2)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	/// <summary>
	/// Check whether all 3 transition paths with different starts are okay.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="digitsMask">The digits.</param>
	/// <param name="transitionPath">The transition path array.</param>
	/// <param name="cellsGroup">The cells group array.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	private static unsafe bool CheckWhetherAll3TransitionPathsDifferentStartAreOkay(
		in Grid grid,
		Mask digitsMask,
		CellMap** transitionPath,
		CellMap** cellsGroup
	)
	{
		// Now, we should check whether the following statement can be true:
		// If we suppose a pair of cells to be 'a' and 'b',
		// then the last two parts of pair of cells will also be 'a' and 'b'.
		// We should check for 6 possible cases:
		//
		//   * Group 1 (requires either is true):
		//     * a -> b -> c
		//     * a -> c -> b
		//   * Group 2 (requires either is true):
		//     * b -> a -> c
		//     * b -> c -> a
		//   * Group 3 (requires either is true):
		//     * c -> a -> b
		//     * c -> b -> a
		//
		// Only if all 3 groups are passed to be checked, the pattern will go valid.

		// Iterate on each global cases (3 cases in total).
		var all3DifferentStartsAreValid = true;
		for (var globalCaseIndex = 0; globalCaseIndex < 3; globalCaseIndex++)
		{
			// Iterate on each subcases (2 cases in total).
			var eitherCaseIsValid = false;
			for (var internalCaseIndex = 0; internalCaseIndex < 2; internalCaseIndex++)
			{
				var i = globalCaseIndex * 2 + internalCaseIndex;
				transitionPath[0] = cellsGroup[TransitionIndices[i][0]];
				transitionPath[1] = cellsGroup[TransitionIndices[i][1]];
				transitionPath[2] = cellsGroup[TransitionIndices[i][2]];

				// Let's start at 'transitionPath[0]', supposing a digit to be filled,
				// and check full path '[0] -> [1] -> [2]'.

				// Iterate on each digit.
				var allDigitsArePassedToCheck = true;
				foreach (var digit in digitsMask)
				{
					// Check both cases [0] -> [1] and [1] -> [2].
					if (!checkSubcase(in *transitionPath[0], in *transitionPath[1], digit)
						|| !checkSubcase(in *transitionPath[1], in *transitionPath[2], digit))
					{
						// Such digit is invalid.
						// But we expect all digits should be passed to be checked - so invalid.
						allDigitsArePassedToCheck = false;
						break;
					}
				}
				if (allDigitsArePassedToCheck)
				{
					eitherCaseIsValid = true;
					break;
				}
			}
			if (!eitherCaseIsValid)
			{
				all3DifferentStartsAreValid = false;
				break;
			}
		}
		return all3DifferentStartsAreValid;


		static bool checkSubcase(ref readonly CellMap cells1, ref readonly CellMap cells2, Digit digit)
		{
			var block = cells2.SharedBlock;
			Debug.Assert(block != FallbackConstants.@int);
			var lastCells = HousesMap[block] & ~cells1.PeerIntersection & CandidatesMap[digit];
			return (cells2 & lastCells) == lastCells;
		}
	}
}
