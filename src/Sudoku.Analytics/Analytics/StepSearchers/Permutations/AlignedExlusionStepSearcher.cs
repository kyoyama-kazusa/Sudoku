namespace Sudoku.Analytics.StepSearchers.Permutations;

/// <summary>
/// Provides with an <b>Aligned Exclusion</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Aligned Pair Exclusion</item>
/// <item>Aligned Triple Exclusion</item>
/// <item>Aligned Quadruple Exclusion</item>
/// <item>Aligned Quintuple Exclusion</item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_AlignedExclusionStepSearcher",
	Technique.AlignedPairExclusion, Technique.AlignedTripleExclusion,
	Technique.AlignedQuadrupleExclusion, Technique.AlignedQuintupleExclusion,
	RuntimeFlags = StepSearcherRuntimeFlags.TimeComplexity)]
public sealed partial class AlignedExclusionStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates the maximum searching size. This value must be 3, 4 or 5. The default value is 3.
	/// </summary>
	[SettingItemName(SettingItemNames.AlignedExclusionMaxSearchingSize)]
	public int MaxSearchingSize { get; set; } = 3;


	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		ref readonly var grid = ref context.Grid;
		var tempAccumulator = new HashSet<AlignedExclusionStep>();
		foreach (var cell in EmptyCells)
		{
			if (BitOperations.IsPow2(grid.GetCandidates(cell)))
			{
				// This technique shouldn't be used for a grid containing naked singles.
				return null;
			}
		}

		for (var size = 3; size <= MaxSearchingSize; size++)
		{
			// Search for "base" cells that can participate to an exclusion set. For each candidate, collect the potentially excluding cells.
			var (candidateList, cellExcluders) = (CellMap.Empty, new Dictionary<Cell, CellMap>());
			foreach (var cell in EmptyCells)
			{
				if (BitOperations.PopCount(grid.GetCandidates(cell)) < 2)
				{
					continue;
				}

				// Look for potentially excluding cells (whose number of candidates is less than size).
				var excludingCells = CellMap.Empty;
				foreach (var excludingCell in PeersMap[cell])
				{
					var count = BitOperations.PopCount(grid.GetCandidates(excludingCell));
					if (count >= 2 && count <= size)
					{
						excludingCells.Add(excludingCell);
					}
				}

				if (excludingCells)
				{
					candidateList.Add(cell);
					cellExcluders.Add(cell, excludingCells);
				}
			}

			if (cellExcluders.Count < size)
			{
				// Not enough elements to be checked.
				return null;
			}

			// Iterate on all permutations of 'size' cells among the possible base cells.
			// To iterate over 'n' cells (n > 2), we first iterate among two cells.
			// Then we retain only the other cells that are visible by at least one of these two cells (the twinArea),
			// and we continue the iteration on these remaining cells.

			// First iterate on the first two cells.
			foreach (ref readonly var cellPair in candidateList & 2)
			{
				// Setup the first two cells.
				var (cell1, cell2) = (cellPair[0], cellPair[1]);
				var (cell1Count, cell2Count) = (BitOperations.PopCount(grid.GetCandidates(cell1)), BitOperations.PopCount(grid.GetCandidates(cell2)));

				// Create the twin area: set of cells visible by one of the two first cells.
				var twinArea = ((cellExcluders[cell1] | cellExcluders[cell2]) & candidateList) - cell1 - cell2;

				// Check if we have enough cells in the twin Area.
				if (twinArea.Count < size - 2)
				{
					continue;
				}

				// Iterate on remaining cells using the twinArea.
				foreach (ref readonly var tIndices in twinArea & size - 2)
				{
					var (cells, cardinalities) = (new Cell[size], new int[size]);

					// Copy the first two cells.
					(cells[0], cells[1]) = (cell1, cell2);
					(cardinalities[0], cardinalities[1]) = (cell1Count, cell2Count);

					// Add the tail cells.
					for (var i = 0; i < tIndices.Count; i++)
					{
						cells[i + 2] = tIndices[i];
						cardinalities[i + 2] = BitOperations.PopCount(grid.GetCandidates(cells[i + 2]));
					}

					// Build the list of common excluding cells for the base cells 'cells'.
					var commonExcluders = CellMap.Empty;
					for (var i = 0; i < size; i++)
					{
						var excludingCells = cellExcluders[cells[i]];
						if (i == 0)
						{
							commonExcluders = excludingCells;
						}
						else
						{
							commonExcluders &= excludingCells;
						}
					}

					if (commonExcluders.Count < 2)
					{
						continue;
					}

					var potentialIndices = new int[size];

					// Iterate on combinations of candidates across the base cells.
					var allowedCombinations = new List<Digit[]>();
					var lockedCombinations = new List<(Digit[], Cell)>();
					bool isFinished;
					do
					{
						// Get next combination of indices.
						var z = 0;
						bool rollOver;
						do
						{
							if (potentialIndices[z] == 0)
							{
								rollOver = true;
								potentialIndices[z] = cardinalities[z] - 1;
								z++;
							}
							else
							{
								rollOver = false;
								potentialIndices[z]--;
							}
						} while (z < size && rollOver);

						// Build the combination of potentials.
						var potentials = new Digit[size];
						for (var i = 0; i < size; i++)
						{
							var values = grid.GetCandidates(cells[i]);
							var p = BitOperations.TrailingZeroCount(values);
							for (var j = 0; j < potentialIndices[i]; j++)
							{
								p = values.GetNextSet(p);
							}
							potentials[i] = p;
						}

						var isAllowed = true;
						var lockingCell = -1;

						// Check if this candidate combination is allowed, using hidden single rule.
						foreach (var mask in Bits.EnumerateOf<Mask>(size, 2))
						{
							var cellIndices = mask.AllSets;
							if ((potentials[cellIndices[0]], potentials[cellIndices[1]]) is var (p1, p2)
								&& p1 == p2
								&& (cells[cellIndices[0]], cells[cellIndices[1]]) is var (c1, c2)
								&& PeersMap[c1].Contains(c2))
							{
								// Hidden single: Using the same candidate value for 2 cells of the set is only allowed
								// if they don't share a house.
								isAllowed = false;
								break;
							}
						}

						// Check if this candidate combination is allowed, using common excluder cells.
						if (isAllowed)
						{
							foreach (var excludingCell in commonExcluders)
							{
								var values = grid.GetCandidates(excludingCell);
								for (var i = 0; i < size; i++)
								{
									values &= (Mask)~(1 << potentials[i]);
								}
								if (values == 0)
								{
									lockingCell = excludingCell;
									isAllowed = false;
									break;
								}
							}
						}

						// Store the combination in the appropriate pattern.
						if (isAllowed)
						{
							allowedCombinations.Add(potentials);
						}
						else
						{
							lockedCombinations.Add((potentials, lockingCell));
						}

						// Check if last combination of candidates from the base cells has been reached.
						isFinished = true;
						for (var i = 0; i < size; i++)
						{
							if (potentialIndices[i] != 0)
							{
								isFinished = false;
								break;
							}
						}
					} while (!isFinished);

					// For all candidates of all base cells, test if the value is possible in at least one allowed combination.
					var conclusions = new List<Conclusion>();
					var conclusionCandidates = CandidateMap.Empty;
					for (var i = 0; i < size; i++)
					{
						var cell = cells[i];
						var values = grid.GetCandidates(cell);
						for (var p = values.GetNextSet(0); p != -1; p = values.GetNextSet(p + 1))
						{
							var isValueAllowed = false;
							foreach (var combination in allowedCombinations)
							{
								if (combination[i] == p)
								{
									isValueAllowed = true; // At least one allowed combination permits this value.
									break;
								}
							}
							if (!isValueAllowed)
							{
								// Yeah, value p can be excluded from cell.
								conclusions.Add(new(Elimination, cell, p));
								conclusionCandidates.Add(cell * 9 + p);
							}
						}
					}
					if (conclusions.Count == 0)
					{
						continue;
					}

					// Get all highlighted candidates.
					var candidateOffsets = new List<CandidateViewNode>();
					var relevantCandidates = GetRelevantCombinationValues(lockedCombinations, conclusions, cells);
					foreach (var (_, cell) in lockedCombinations)
					{
						if (cell != -1)
						{
							var digits = grid.GetCandidates(cell);
							if ((relevantCandidates & digits) == digits)
							{
								foreach (var digit in digits)
								{
									candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
								}
							}
						}
					}

					foreach (var cell in cells)
					{
						foreach (var digit in grid.GetCandidates(cell))
						{
							if (!conclusionCandidates.Contains(cell * 9 + digit))
							{
								candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + digit));
							}
						}
					}

					// Create hint.
					var cellsMap = cells.AsCellMap();
					var step = new AlignedExclusionStep(
						conclusions.AsMemory(),
						[[.. candidateOffsets]],
						context.Options,
						cellsMap,
						grid[cellsMap],
						[.. lockedCombinations]
					);
					if (context.OnlyFindOne)
					{
						return step;
					}

					tempAccumulator.Add(step);
				}
			}
		}

		if (!context.OnlyFindOne && tempAccumulator.Count != 0)
		{
			context.Accumulator.AddRange(tempAccumulator);
		}
		return null;
	}

	/// <summary>
	/// Get all possible relevant combination digits.
	/// </summary>
	/// <param name="lockedCombinations">The all locked combinations.</param>
	/// <param name="conclusions">All conclusions.</param>
	/// <param name="cells">The cells used.</param>
	/// <returns>A mask of digits for relevant ones.</returns>
	private Mask GetRelevantCombinationValues(List<(Digit[], Cell)> lockedCombinations, List<Conclusion> conclusions, Cell[] cells)
	{
		var result = (Mask)0;
		foreach (var (combination, _) in lockedCombinations)
		{
			if (isRelevant(combination, conclusions, cells))
			{
				foreach (var digit in combination)
				{
					result |= (Mask)(1 << digit);
				}
			}
		}
		return result;


		static bool isRelevant(Digit[] combination, List<Conclusion> conclusions, Cell[] cells)
		{
			Debug.Assert(combination.Length == cells.Length);
			for (var i = 0; i < combination.Length; i++)
			{
				var (cell, digit, flag) = (cells[i], combination[i], false);
				foreach (var conclusion in conclusions)
				{
					if (conclusion.Cell == cell && conclusion.Digit == digit)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					return true;
				}
			}
			return false;
		}
	}
}
