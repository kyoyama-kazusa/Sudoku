#define ENABLE_VERIFICATION_ON_BLOCK_COMBINATION

namespace Sudoku.Generating;

/// <summary>
/// Represents a puzzle generator that can avoid selections on empty houses.
/// </summary>
public ref partial struct EmptyHouseBasedGenerator() : IGenerator<Grid>
{
	/// <summary>
	/// The order in which cells are set when generating a full grid.
	/// </summary>
	private readonly int[] _generateIndices = new int[81];

	/// <summary>
	/// A random generator for creating new puzzles.
	/// </summary>
	private readonly Random _rng = new();

	/// <summary>
	/// Indicates the internal fast solver.
	/// </summary>
	private readonly BitwiseSolver _solver = new();

	/// <summary>
	/// The recursion stack.
	/// </summary>
	private readonly Span<GeneratorRecursionStackEntry> _stack = new GeneratorRecursionStackEntry[82];

	/// <summary>
	/// The final grid to be used.
	/// </summary>
	private Grid _newFullSudoku, _newValidSudoku;


	/// <summary>
	/// <para>Indicates the desired missing houses mask.</para>
	/// <para>
	/// This property can be made a <see cref="Dictionary{TKey, TValue}"/>
	/// of type arguments <see cref="HouseType"/> (the desired house types)
	/// and <see cref="int"/> (indicating the number of empty houses) in logic.
	/// You can set the desired number of 1 bits to construct the mask,
	/// telling the current generator the number of empty houses you want to keep for generated puzzles.
	/// Use 0 as placeholders. Set all 1's to target bit ranges that the mask represents.
	/// </para>
	/// <para>
	/// For example, if you want to keep 1 empty row, 1 empty column and 1 empty block,
	/// you can set the mask with value <c>0b000000001000000001000000001</c> (i.e. 262657).
	/// </para>
	/// </summary>
	public HouseMask DesiredMissingHousesMask { get; init; }


	/// <summary>
	/// Try to generate a puzzle randomly, or return <see cref="Grid.Undefined"/> if a user canceled the operation.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token that can cancel the operation.</param>
	/// <returns>The result grid, or <see cref="Grid.Undefined"/> if a user canceled the operation.</returns>
	public Grid Generate(CancellationToken cancellationToken = default)
	{
		_stack.Fill(new());

		while (true)
		{
			while (!GenerateForFullGrid()) ;

			if (GenerateInitPos(cancellationToken))
			{
				break;
			}

			if (cancellationToken.IsCancellationRequested)
			{
				return Grid.Undefined;
			}
		}
		return _newValidSudoku.FixedGrid;
	}

	/// <summary>
	/// Takes a full sudoku from <see cref="_newFullSudoku"/> and generates a valid puzzle by deleting cells.
	/// If a deletion produces a grid with more than one solution it is of course undone.
	/// </summary>
	/// <inheritdoc cref="Generate(CancellationToken)"/>
	private unsafe bool GenerateInitPos(CancellationToken cancellationToken = default)
	{
		var bitsMap = new Dictionary<HouseType, int>(3)
		{
			{ HouseType.Block, PopCount((uint)(DesiredMissingHousesMask & Grid.MaxCandidatesMask)) },
			{ HouseType.Row, PopCount((uint)(DesiredMissingHousesMask >> 9 & Grid.MaxCandidatesMask)) },
			{ HouseType.Column, PopCount((uint)(DesiredMissingHousesMask >> 18)) }
		};

		var blockIndices = SpanEnumerable.Range(0, 9).ToArray().AsSpan();
		var rowIndices = SpanEnumerable.Range(9, 9).ToArray().AsSpan();
		var columnIndices = SpanEnumerable.Range(18, 9).ToArray().AsSpan();
		var localPairs = stackalloc LocalPair[]
		{
			new() { HouseType = HouseType.Block, HouseIndices = &blockIndices },
			new() { HouseType = HouseType.Row, HouseIndices = &rowIndices },
			new() { HouseType = HouseType.Column, HouseIndices = &columnIndices }
		};

		var patternTestTimes = -1;
		while (true)
		{
			// If the maximum test times are reached, we should change a solution to create puzzles.
			//
			// You might ask me why 126 is here.
			// In practice, a valid puzzle (having a unique solution) cannot remove all cells from 5+ complete blocks.
			// Therefore, I put 126 here to represent the maximum number of test cases (C(9, 4) = 126).
			// But it is not always a correct limit in fact.
			if (++patternTestTimes >= 126)
			{
				return false;
			}

			if (cancellationToken.IsCancellationRequested)
			{
				return false;
			}

			// Step 1: Clear some houses.
			// We start with the full board.
			// Randomize the number of values indicating the target houses are empty.
			_newValidSudoku = _newFullSudoku;
			var used = CellMap.Empty;
			var remainingClues = 81;

			// Fetch the number of empty houses set.
			var hasUniqueSolutionAfterClearingValuesFromSpecifiedHouses = true;
		loop_i:
			for (var i = 0; i < 3; i++)
			{
				var l = localPairs + i;
				if (bitsMap[l->HouseType] == 0)
				{
					// Skip for the unset house types.
					continue;
				}

				_rng.Shuffle(*l->HouseIndices);
				var (previousHousesMask, j) = (0, 0);
				foreach (var house in *l->HouseIndices)
				{
					// Check whether the house selected will directly cause multiple solutions.
					// If so, we should skip for the house.
					var willHouseCauseMultipleSolutions = false;
					if (house < 9)
					{
#if ENABLE_VERIFICATION_ON_BLOCK_COMBINATION
						if (PopCount((uint)previousHousesMask) >= 2)
						{
							var allSetBits = previousHousesMask.AllSets;
							foreach (var pair in allSetBits & 2)
							{
								var index = pair[0] * 9 + pair[1];
								if ((ValidBlockCombinations[index] >> house & 1) == 0)
								{
									// Invalid selection.
									willHouseCauseMultipleSolutions = true;
									break;
								}
							}
						}
#endif
					}
					else
					{
						foreach (var combination in InvalidLineCombinations[house - 9])
						{
							if (combination == (previousHousesMask | 1 << house))
							{
								willHouseCauseMultipleSolutions = true;
								break;
							}
						}
					}
					if (willHouseCauseMultipleSolutions)
					{
						continue;
					}

					// Remove all possible cells from such target house.
					foreach (var cell in HousesMap[house])
					{
						_newValidSudoku.SetDigit(cell, -1);
						if (used.Add(cell))
						{
							remainingClues--;
						}
					}

					// Determine whether the puzzle has unique solution.
					if (!_solver.CheckValidity(_newValidSudoku.ToString("!0")))
					{
						hasUniqueSolutionAfterClearingValuesFromSpecifiedHouses = false;
						break loop_i;
					}

					previousHousesMask |= 1 << house;
					if (++j >= bitsMap[l->HouseType])
					{
						// Enough houses are cleared.
						break;
					}
				}
			}
			if (!hasUniqueSolutionAfterClearingValuesFromSpecifiedHouses)
			{
				continue;
			}

			// Step 2: Generate puzzles.
			var step2TrialTimes = -1;
			while (true)
			{
				if (++step2TrialTimes >= 1000)
				{
					break;
				}

				var targetRemainingClues = _rng.Next(17, 30);
				while (remainingClues > targetRemainingClues && used != ~CellMap.Empty)
				{
					if (cancellationToken.IsCancellationRequested)
					{
						return false;
					}

					// Get the next position to try.
					var cell = _rng.NextCell();
					do
					{
						if (cell < 80)
						{
							cell++;
						}
						else
						{
							cell = 0;
						}
					}
					while (used.Contains(cell));
					used += cell;

					if (_newValidSudoku.GetDigit(cell) == -1)
					{
						// Already deleted.
						continue;
					}

					// Delete cell.
					_newValidSudoku.SetDigit(cell, -1);
					remainingClues--;

					if (!_solver.CheckValidity(_newValidSudoku.ToString("!0")))
					{
						// If not unique, revert deletion.
						_newValidSudoku.SetDigit(cell, _newFullSudoku.GetDigit(cell));
						remainingClues++;
					}
				}

				// Check validity.
				if (_newValidSudoku.ModifiableCellsCount <= targetRemainingClues
					&& _solver.CheckValidity(_newValidSudoku.ToString("!0")))
				{
					return true;
				}
			}
		}
	}

	/// <summary>
	/// Generate a solution grid.
	/// </summary>
	/// <returns>A <see cref="bool"/> value indicating whether the generation operation is succeeded.</returns>
	private bool GenerateForFullGrid()
	{
		// Limit the number of tries.
		var actTries = 0;

		// Generate a random order for setting the cells.
		for (var i = 0; i < 81; i++)
		{
			_generateIndices[i] = i;
		}

		for (var i = 0; i < 81; i++)
		{
			var (index1, index2) = (_rng.NextCell(), _rng.NextCell());
			while (index1 == index2)
			{
				index2 = _rng.NextCell();
			}

			Unsafe.Swap(ref _generateIndices[index1], ref _generateIndices[index2]);
		}

		// First set a new empty Sudoku.
		(_stack[0].SudokuGrid, _stack[0].Cell, var level) = (Grid.Empty, -1, 0);
	outer_while_loop:
		while (true)
		{
			// Get the next unsolved cell according to _generateIndices.
			if (_stack[level].SudokuGrid.EmptyCellsCount == 0)
			{
				// Generation is complete.
				_newFullSudoku = _stack[level].SudokuGrid;
				return true;
			}

			var index = -1;
			var actValues = _stack[level].SudokuGrid.ToDigitsArray();
			for (var i = 0; i < 81; i++)
			{
				var actTry = _generateIndices[i];
				if (actValues[actTry] == 0)
				{
					index = actTry;
					break;
				}
			}

			level++;
			_stack[level].Cell = (short)index;
			_stack[level].Candidates = _stack[level - 1].SudokuGrid.GetCandidates(index);
			_stack[level].CandidateIndex = 0;

			// Not too many tries...
			actTries++;
			if (actTries > 100)
			{
				return false;
			}
			do
			{
				// This loop runs as long as the next candidate tried produces an invalid sudoku or until all candidates have been tried.
				// Fall back all levels, where nothing is to do anymore.
				while (_stack[level].CandidateIndex >= PopCount((uint)_stack[level].Candidates))
				{
					level--;
					if (level <= 0)
					{
						break outer_while_loop;
					}
				}

				// Try the next candidate.
				var nextCandidate = _stack[level].Candidates.SetAt(_stack[level].CandidateIndex++);

				// Start with a fresh sudoku.
				ref var targetGrid = ref _stack[level].SudokuGrid;
				targetGrid = _stack[level - 1].SudokuGrid;
				targetGrid.SetDigit(_stack[level].Cell, nextCandidate);
				if (!checkValidityOnDuplicate(targetGrid, _stack[level].Cell))
				{
					// Invalid -> try next candidate.
					continue;
				}

				if (fillFastForSingles(ref targetGrid))
				{
					// Valid move, break from the inner loop to advance to the next level.
					break;
				}
			} while (true);
		}
		return false;


		static bool checkValidityOnDuplicate(in Grid grid, Cell cell)
		{
			foreach (var peer in Peer.PeersMap[cell])
			{
				var digit = grid.GetDigit(peer);
				if (digit == grid.GetDigit(cell) && digit != -1)
				{
					return false;
				}
			}
			return true;
		}

		static bool fillFastForSingles(ref Grid grid)
		{
			var emptyCells = grid.EmptyCells;

			// For hidden singles.
			for (var house = 0; house < 27; house++)
			{
				for (var digit = 0; digit < 9; digit++)
				{
					var houseMask = 0;
					for (var i = 0; i < 9; i++)
					{
						var cell = HousesCells[house][i];
						if (emptyCells.Contains(cell) && (grid.GetCandidates(cell) >> digit & 1) != 0)
						{
							houseMask |= 1 << i;
						}
					}

					if (IsPow2(houseMask))
					{
						// Hidden single.
						var cell = HousesCells[house][TrailingZeroCount(houseMask)];
						grid.SetDigit(cell, digit);
						if (!checkValidityOnDuplicate(grid, cell))
						{
							// Invalid.
							return false;
						}
					}
				}
			}

			// For naked singles.
			foreach (var cell in emptyCells)
			{
				var mask = grid.GetCandidates(cell);
				if (IsPow2(mask))
				{
					grid.SetDigit(cell, TrailingZeroCount(mask));
					if (!checkValidityOnDuplicate(grid, cell))
					{
						// Invalid.
						return false;
					}
				}
			}

			// Both hidden singles and naked singles are valid. Return true.
			return true;
		}
	}

	/// <inheritdoc/>
	Grid IGenerator<Grid>.Generate(IProgress<GeneratorProgress>? progress, CancellationToken cancellationToken)
		=> Generate(cancellationToken: cancellationToken);
}

/// <summary>
/// To simplify for loop, we use a temporary type to construct values into a sequence of unsafe values.
/// </summary>
file ref struct LocalPair
{
	/// <summary>
	/// Indicates the target house type.
	/// </summary>
	public HouseType HouseType;

	/// <summary>
	/// Indicates the target house indices.
	/// </summary>
	public unsafe Span<int>* HouseIndices;
}
