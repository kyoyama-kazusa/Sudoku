// Copyright (C) 2008-12 Bernhard Hobiger
// 
// HoDoKu is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// 
// HoDoKu is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with HoDoKu. If not, see <http://www.gnu.org/licenses/>.
// 
// This code is actually a Java port of code posted by Glenn Fowler in the Sudoku Player's Forum (http://www.setbb.com/sudoku).
// Many thanks for letting me use it!
//
// Link: https://sourceforge.net/p/hodoku/code/HEAD/tree/HoDoKu/trunk/src/generator/SudokuGenerator.java

namespace Sudoku.Generating;

/// <summary>
/// Represents a puzzle generator, implemented by HoDoKu.
/// </summary>
[TypeImpl(TypeImplFlags.AllObjectMethods)]
public ref partial struct Generator() : IGenerator<Grid>
{
	/// <summary>
	/// Indicates whether the solution grid can be configured by user.
	/// </summary>
	/// <!--2024/10/28: Add this field.-->
	private readonly bool _useCustomizedSolution;

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
	/// Initializes a <see cref="Generator"/> instance via the specified template.
	/// </summary>
	/// <param name="template">The template.</param>
	public Generator(in Grid template) : this()
	{
		// 2024/10/25: Add this constructor as template initialization.
		// 2024/10/28: Add '_useCustomizedSolution = true;'.

		ArgumentOutOfRangeException.ThrowIfNotEqual(template.IsSolved, true);

		_newFullSudoku = template.UnfixedGrid;
		_useCustomizedSolution = true;
	}


	/// <summary>
	/// Try to generate a puzzle randomly, or return <see cref="Grid.Undefined"/> if a user cancelled the operation.
	/// </summary>
	/// <param name="cluesCount">
	/// <para>Indicates the number of clues of a puzzle generated. Assign -1 if the value is not required.</para>
	/// <para>
	/// Please note that the target puzzle may not contain the same number of givens as this value.
	/// If the number of givens from a puzzle is below this value but it also has a unique solution,
	/// this puzzle will be still treated as valid one.
	/// </para>
	/// <para>The default value is -1.</para>
	/// </param>
	/// <param name="symmetricType">The symmetric type to be specified. The value is <see cref="SymmetricType.Central"/> by default.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the operation.</param>
	/// <returns>The result grid, or <see cref="Grid.Undefined"/> if a user cancelled the operation.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the argument <paramref name="symmetricType"/> holds multiple flags,
	/// or the argument <paramref name="cluesCount"/> is invalid.
	/// </exception>
	public Grid Generate(int cluesCount = -1, SymmetricType symmetricType = SymmetricType.Central, CancellationToken cancellationToken = default)
	{
		_stack.Fill(new());

		ArgumentOutOfRangeException.ThrowIfNotEqual(symmetricType.IsFlag(), true);
		ArgumentOutOfRangeException.ThrowIfNotEqual(cluesCount is >= 17 and <= 80 or -1, true);

		try
		{
			// 2024/10/25: Add this if block to skip initialization for templates.
			// 2024/10/28: Change if statement from 'if (_newFullSudoku.IsUndefined)' to 'if (!_useCustomizedSolution)'.
			if (!_useCustomizedSolution)
			{
				while (!GenerateForFullGrid()) ;
			}

			GenerateInitPos(cluesCount, symmetricType, cancellationToken);
			return _newValidSudoku.FixedGrid;
		}
		catch (OperationCanceledException)
		{
			return Grid.Undefined;
		}
	}

	/// <summary>
	/// Takes a full sudoku from <see cref="_newFullSudoku"/> and generates a valid puzzle by deleting cells.
	/// If a deletion produces a grid with more than one solution it is of course undone.
	/// </summary>
	/// <inheritdoc cref="Generate(int, SymmetricType, CancellationToken)"/>
	private void GenerateInitPos(int cluesCount, SymmetricType symmetricType, CancellationToken cancellationToken = default)
	{
		// We start with the full board.
		(_newValidSudoku, var (used, usedCount, remainingClues)) = (_newFullSudoku, (CellMap.Empty, 81, 81));
		var candidateCells = new List<Cell>(8);

		// Do until we have only 17 clues left or until all cells have been tried.
		while (remainingClues > (cluesCount == -1 ? 17 : cluesCount) && usedCount > 1)
		{
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
			used.Add(cell);
			usedCount--;

			if (_newValidSudoku.GetDigit(cell) == -1)
			{
				// Already deleted (symmetry).
				continue;
			}

			candidateCells.Clear();

			foreach (var tempCell in symmetricType.GetCells(cell / 9, cell % 9))
			{
				if (_newValidSudoku.GetDigit(tempCell) != -1)
				{
					candidateCells.Add(tempCell);
				}
			}
			if (candidateCells.Count == 0)
			{
				// The other end of our symmetric puzzle is already deleted.
				continue;
			}

			foreach (var candidateCell in candidateCells)
			{
				// Delete cell.
				_newValidSudoku.SetDigit(candidateCell, -1);
				used.Add(candidateCell);
				remainingClues--;
				if (candidateCell != cell)
				{
					usedCount--;
				}
			}

			if (!_solver.CheckValidity(_newValidSudoku.ToString("!0")))
			{
				// If not unique, revert deletion.
				foreach (var candidateCell in candidateCells)
				{
					_newValidSudoku.SetDigit(candidateCell, _newFullSudoku.GetDigit(candidateCell));
					remainingClues++;
				}
			}

			cancellationToken.ThrowIfCancellationRequested();
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

			@ref.Swap(ref _generateIndices[index1], ref _generateIndices[index2]);
		}

		// First set a new empty Sudoku.
		(_stack[0].SudokuGrid, _stack[0].Cell, var level) = (Grid.Empty, -1, 0);
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

			// Go to the next level.
			var done = false;
			do
			{
				// This loop runs as long as the next candidate tried produces an invalid sudoku or until all candidates have been tried.
				// Fall back all levels, where nothing is to do anymore.
				while (_stack[level].CandidateIndex >= Mask.PopCount(_stack[level].Candidates))
				{
					level--;
					if (level <= 0)
					{
						// No level with candidates left.
						done = true;
						break;
					}
				}
				if (done)
				{
					break;
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
			if (done)
			{
				break;
			}
		}
		return false;


		static bool checkValidityOnDuplicate(in Grid grid, Cell cell)
		{
			foreach (var peer in PeersMap[cell])
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

					if (HouseMask.IsPow2(houseMask))
					{
						// Hidden single.
						var cell = HousesCells[house][HouseMask.TrailingZeroCount(houseMask)];
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
				if (Mask.IsPow2(mask))
				{
					grid.SetDigit(cell, Mask.TrailingZeroCount(mask));
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
