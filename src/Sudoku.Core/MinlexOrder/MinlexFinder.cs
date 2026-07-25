namespace Sudoku.MinlexOrder;

/// <summary>
/// Represents a finder type.
/// </summary>
public sealed class MinlexFinder
{
	/// <summary>
	/// Represents empty cell placeholder.
	/// </summary>
	internal const byte EmptyCellPlaceholder = 99;

	/// <summary>
	/// Indicates the total number of candidate list, which means the worst case.
	/// </summary>
	private const int CandidateListTotal = 15552;


	/// <summary>
	/// Indicates the internal mappers.
	/// </summary>
	private readonly List<Mapper> _mappers = [];


	/// <inheritdoc cref="Find(string, out Transform)"/>
	public unsafe string Find(string grid)
	{
		_mappers.Clear();

		var givensCount = GridPattern.FromString(grid, out var pair);
		var minTopRowScores = (int[])[GridPattern.BestTopRowScore(pair[0]), GridPattern.BestTopRowScore(pair[1])];
		var minTopRowScore = minTopRowScores[0] <= minTopRowScores[1] ? minTopRowScores[0] : minTopRowScores[1];

		var result = (stackalloc char[81]);
		result[0] = (char)((minTopRowScore >> 8) & 1);
		result[1] = (char)((minTopRowScore >> 7) & 1);
		result[2] = (char)((minTopRowScore >> 6) & 1);
		result[3] = (char)((minTopRowScore >> 5) & 1);
		result[4] = (char)((minTopRowScore >> 4) & 1);
		result[5] = (char)((minTopRowScore >> 3) & 1);
		result[6] = (char)((minTopRowScore >> 2) & 1);
		result[7] = (char)((minTopRowScore >> 1) & 1);
		result[8] = (char)(minTopRowScore & 1);

		// Step 1: Determine for top rows.
		var candidatesRow02468 = (stackalloc MinlexCandidate[CandidateListTotal]);
		var candidatesRow1357 = (stackalloc MinlexCandidate[CandidateListTotal]);
		var casesCount = 0;
		for (var nowTransposed = (sbyte)0; nowTransposed < 2; nowTransposed++)
		{
			if (minTopRowScores[nowTransposed] > minTopRowScore)
			{
				continue;
			}

			for (var topRow = 0; topRow < 9; topRow++)
			{
				if (GridPattern.MinCanNineBits[pair[nowTransposed].Rows[topRow]] <= minTopRowScore)
				{
					// Here we have a top row candidate.
					// In the empty template fix only the transposition and row 0.
					var cand = new MinlexCandidate(nowTransposed, topRow);

					// To fix all minimal stack permutations and store for later row expansion
					cand.ExpandStacks(pair, minTopRowScore, candidatesRow02468, ref casesCount);
				}
			}
		}
		// Here we have all top row candidates with fixed top row, stacks, and column permutation masks.

		// Step 2: Expand rows.
		ref var currentCandidates = ref candidatesRow02468[0]; // Read from here.
		ref var nextCandidates = ref candidatesRow1357[0]; // Add row and write here.
		var nextCandidatesCount = 0;
		for (var toRow = 1; toRow < 9; toRow++)
		{
			var (bestTriplets0, bestTriplets1, bestTriplets2, rowInBand) = (7, 7, 7, toRow % 3);
			for (var caseIndex = 0; caseIndex < casesCount; caseIndex++)
			{
				ref readonly var old = ref Unsafe.Add(ref currentCandidates, caseIndex);
				var (startRow, endRow) = rowInBand != 0 && old.MapRowsBackward[3 * (toRow / 3)] / 3 is var band
					? (band * 3, (band + 1) * 3) // Combine with unmapped rows from the same band.
					: (0, 9); // Try any unmapped row.

				for (var fromRow = startRow; fromRow < endRow; fromRow++)
				{
					if (old.MapRowsForward[fromRow] >= 0)
					{
						// Skip previously mapped rows.
						continue;
					}

					var toTriplets = new int[3];
					var rowGivens = pair[old.IsTransposed].Rows[fromRow]; // Stacks unmapped.
					toTriplets[BestTripletPermutation.Perm[old.StacksPermutation][0]] = rowGivens >> 6;
					toTriplets[BestTripletPermutation.Perm[old.StacksPermutation][1]] = (rowGivens >> 3) & 7;
					toTriplets[BestTripletPermutation.Perm[old.StacksPermutation][2]] = rowGivens & 7;
					ref readonly var bt0 = ref BestTripletPermutation.BestTripletPermutations[toTriplets[0]][old.ColumnsPermutationMask[0]];
					if (bt0.BestResult > bestTriplets0)
					{
						continue;
					}

					if (bt0.BestResult < bestTriplets0)
					{
						nextCandidatesCount = 0;
						bestTriplets0 = bt0.BestResult;
						bestTriplets1 = 7;
						bestTriplets2 = 7;
					}
					ref readonly var bt1 = ref BestTripletPermutation.BestTripletPermutations[toTriplets[1]][old.ColumnsPermutationMask[1]];
					if (bt1.BestResult > bestTriplets1)
					{
						continue;
					}

					if (bt1.BestResult < bestTriplets1)
					{
						nextCandidatesCount = 0;
						bestTriplets1 = bt1.BestResult;
						bestTriplets2 = 7;
					}
					ref readonly var bt2 = ref BestTripletPermutation.BestTripletPermutations[toTriplets[2]][old.ColumnsPermutationMask[2]];
					if (bt2.BestResult > bestTriplets2)
					{
						continue;
					}

					if (bt2.BestResult < bestTriplets2)
					{
						nextCandidatesCount = 0;
						bestTriplets2 = bt2.BestResult;
					}

					// Tests passed, output the new candidate.
					ref var next = ref Unsafe.Add(ref nextCandidates, nextCandidatesCount++);
					next = old;
					next.MapRowsForward[fromRow] = (sbyte)toRow;
					next.MapRowsBackward[toRow] = (sbyte)fromRow;
					next.ColumnsPermutationMask[0] = (byte)bt0.ResultMask;
					next.ColumnsPermutationMask[1] = (byte)bt1.ResultMask;
					next.ColumnsPermutationMask[2] = (byte)bt2.ResultMask;
				}
			}

			// Flip-flop the old/new.
			ref var tmp = ref currentCandidates;
			currentCandidates = ref nextCandidates;
			nextCandidates = ref tmp;
			casesCount = nextCandidatesCount;
			nextCandidatesCount = 0;

			// Store the best result.
			ref var r = ref result[9 * toRow];
			r = (char)((bestTriplets0 >> 2) & 1);
			Unsafe.Add(ref r, 1) = (char)((bestTriplets0 >> 1) & 1);
			Unsafe.Add(ref r, 2) = (char)(bestTriplets0 & 1);
			Unsafe.Add(ref r, 3) = (char)((bestTriplets1 >> 2) & 1);
			Unsafe.Add(ref r, 4) = (char)((bestTriplets1 >> 1) & 1);
			Unsafe.Add(ref r, 5) = (char)(bestTriplets1 & 1);
			Unsafe.Add(ref r, 6) = (char)((bestTriplets2 >> 2) & 1);
			Unsafe.Add(ref r, 7) = (char)((bestTriplets2 >> 1) & 1);
			Unsafe.Add(ref r, 8) = (char)(bestTriplets2 & 1);
		}

		if (casesCount == 0)
		{
			throw new InvalidOperationException("bad news: no candidatesRow02468 for minlex due to program errors");
		}

		// Step 3: Find the lexicographically minimal representative within the morphs.
		// This time taking into account the real values of the input givens.
		Unsafe.SkipInit<Mapper>(out var map);
		var minlex = (stackalloc int[81]); // The best result so far.
		for (var cell = 0; cell < 81; cell++)
		{
			minlex[cell] = result[cell] << 5; // Initially set to large values.
		}

		for (var currentCandidateIndex = 0; currentCandidateIndex < casesCount; currentCandidateIndex++)
		{
			ref var target = ref Unsafe.Add(ref currentCandidates, currentCandidateIndex);
			var toTriplets = new int[3];
			toTriplets[BestTripletPermutation.Perm[target.StacksPermutation][0]] = 0;
			toTriplets[BestTripletPermutation.Perm[target.StacksPermutation][1]] = 3;
			toTriplets[BestTripletPermutation.Perm[target.StacksPermutation][2]] = 6;
			for (var colsPerm0 = 0; colsPerm0 < 6; colsPerm0++)
			{
				if (((target.ColumnsPermutationMask[0] >> colsPerm0) & 1) == 0)
				{
					continue;
				}

				var toColsInStack = new int[9];
				toColsInStack[BestTripletPermutation.Perm[colsPerm0][0]] = toTriplets[0];
				toColsInStack[BestTripletPermutation.Perm[colsPerm0][1]] = toTriplets[0] + 1;
				toColsInStack[BestTripletPermutation.Perm[colsPerm0][2]] = toTriplets[0] + 2;
				for (var colsPerm1 = 0; colsPerm1 < 6; colsPerm1++)
				{
					if (((target.ColumnsPermutationMask[1] >> colsPerm1) & 1) == 0)
					{
						continue;
					}

					toColsInStack[3 + BestTripletPermutation.Perm[colsPerm1][0]] = toTriplets[1];
					toColsInStack[3 + BestTripletPermutation.Perm[colsPerm1][1]] = toTriplets[1] + 1;
					toColsInStack[3 + BestTripletPermutation.Perm[colsPerm1][2]] = toTriplets[1] + 2;
					for (var colsPerm2 = 0; colsPerm2 < 6; colsPerm2++)
					{
						if (((target.ColumnsPermutationMask[2] >> colsPerm2) & 1) == 0)
						{
							continue;
						}

						toColsInStack[6 + BestTripletPermutation.Perm[colsPerm2][0]] = toTriplets[2];
						toColsInStack[6 + BestTripletPermutation.Perm[colsPerm2][1]] = toTriplets[2] + 1;
						toColsInStack[6 + BestTripletPermutation.Perm[colsPerm2][2]] = toTriplets[2] + 2;
						var (labelPerm, nextFreeLabel, setCellsCount) = (new int[10], 1, 0);
					loop_toRow:
						for (var toRow = 0; toRow < 9; toRow++)
						{
							ref readonly var rowGivens = ref pair[target.IsTransposed].Digits[target.MapRowsBackward[toRow] * 9];
							for (var col = 0; col < 9; col++)
							{
								var fromDigit = Unsafe.Add(ref Unsafe.AsRef(in rowGivens), toColsInStack[col]);
								if (fromDigit == 0)
								{
									continue;
								}

								if (labelPerm[fromDigit] == 0)
								{
									labelPerm[fromDigit] = nextFreeLabel++;
								}
								if (labelPerm[fromDigit] > minlex[toRow * 9 + col])
								{
									break loop_toRow;
								}

								setCellsCount++;
								if (labelPerm[fromDigit] < minlex[toRow * 9 + col])
								{
									for (var i = toRow * 9 + col + 1; i < 81; i++)
									{
										minlex[i] = result[i] << 5; // Invalidate the rest.
									}

									minlex[toRow * 9 + col] = labelPerm[fromDigit];

									// The buffered transformations become invalid at this point.
									_mappers.Clear();
								}
								if (setCellsCount == givensCount)
								{
									// An isomorph of the currently best ordering
									// at this point we have the necessary information
									// for the transformation and can buffer it (if eventually it is one of the best ones).
									for (var r = 0; r < 9; r++)
									{
										for (var c = 0; c < 9; c++)
										{
											var src = target.IsTransposed != 0
												? target.MapRowsBackward[r] + 9 * toColsInStack[c]
												: target.MapRowsBackward[r] * 9 + toColsInStack[c];

											// Map all non-givens to 'EmptyCellPlaceholder', this masking irrelevant permutations.
											map.Cell[src] = (byte)(minlex[r * 9 + c] != 0 ? r * 9 + c : EmptyCellPlaceholder);
										}
									}
									for (var d = 0; d < 10; d++)
									{
										map.Label[d] = 0;
									}

									for (var d = 0; d < 10; d++)
									{
										map.Label[labelPerm[d]] = (byte)d;
									}

									map.Label[0] = 0;
									_mappers.AddRef(map); // Will automatically ignore duplicates.
								}
							}
						}
					}
				}
			}
		}
		for (var cell = 0; cell < 81; cell++)
		{
			result[cell] = minlex[cell] != 0 ? (char)(minlex[cell] + '0') : '0';
		}
		return result.ToString();
	}

	/// <inheritdoc cref="Find(string)"/>
	public unsafe Grid Find(in Grid grid) => grid.ToString("0") | Find | &Grid.Parse;

	/// <summary>
	/// Finds the minimum lexicographical form of the source grid code.
	/// </summary>
	/// <param name="grid">Indicates the source grid.</param>
	/// <param name="transform">The transform.</param>
	/// <returns>The corresponding minimum lexicographical form of the grid.</returns>
	internal string Find(string grid, out Transform transform)
	{
		var minlexed = Find(grid);
		if (grid == minlexed)
		{
			// Already min-lex.
			transform = Transform.Equivalent;
			return minlexed;
		}

		var mappers = _mappers.AsSpan();
		foreach (ref readonly var mapper in mappers)
		{
			var reconstructed = ReconstructOriginal(mapper, minlexed);
			if (reconstructed == grid)
			{
				transform = mapper.FromMapperInfer();
				return minlexed;
			}
		}
		throw new InvalidOperationException();
	}

	/// <summary>
	/// Reconstruct original.
	/// </summary>
	/// <param name="mapper">The mapper instance.</param>
	/// <param name="minlex">The min-lex grid string.</param>
	/// <returns>The target string.</returns>
	/// <exception cref="InvalidOperationException">Throws when label is invalid.</exception>
	internal unsafe string ReconstructOriginal(in Mapper mapper, string minlex)
	{
		var inversed = (stackalloc int[81]);
		inversed.Fill(-1);
		fixed (byte* cells = mapper.Cell)
		{
			for (var source = 0; source < 81; source++)
			{
				var c = cells[source];
				if (c < EmptyCellPlaceholder)
				{
					inversed[c] = source;
				}
			}
		}

		var resultCharacters = (stackalloc char[81]);
		resultCharacters.Fill('.');
		fixed (byte* labels = mapper.Label)
		{
			for (var canon = 0; canon < 81; canon++)
			{
				var dst = inversed[canon];
				if (dst < 0)
				{
					// Skip - canonical position cannot be found in source cell.
					continue;
				}

				var ch = minlex[canon];
				if (ch == '0')
				{
					// Canonical is empty -> Original is empty.
					resultCharacters[dst] = '0';
				}
				else
				{
					var label = ch - '0';
					if (label is < 0 or > 9)
					{
						throw new InvalidOperationException("bad canonical label");
					}

					var origDigit = labels[label]; // map.Label[labelIndex] = original digit.
					resultCharacters[dst] = origDigit == 0 ? '0' : (char)('0' + origDigit);
				}
			}
		}
		return resultCharacters.ToString();
	}

	/// <inheritdoc cref="Find(string, out Transform)"/>
	internal Grid Find(in Grid grid, out Transform transform) => Grid.Parse(Find(grid.ToString("0"), out transform));
}
