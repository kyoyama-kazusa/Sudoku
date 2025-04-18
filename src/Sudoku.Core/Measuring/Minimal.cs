namespace Sudoku.Measuring;

/// <summary>
/// Represents extension methods on <see cref="Grid"/> for minimal.
/// </summary>
/// <seealso cref="Grid"/>
public static class Minimal
{
	/// <summary>
	/// Determines whether the puzzle is a minimal puzzle, which means the puzzle will become multiple solution
	/// if arbitrary one given digit will be removed from the grid.
	/// </summary>
	/// <param name="this">Indicates the current instance.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool GetIsMinimal(this in Grid @this) => @this.CheckMinimal(out _);

	/// <summary>
	/// Determines whether the puzzle is a minimal puzzle, which means the puzzle will become multiple solution
	/// if arbitrary one given digit will be removed from the grid.
	/// </summary>
	/// <param name="this">Indicates the current instance.</param>
	/// <param name="firstCandidateMakePuzzleNotMinimal">
	/// <para>
	/// Indicates the first found candidate that can make the puzzle not minimal, which means
	/// if we remove the digit in the cell, the puzzle will still keep unique.
	/// </para>
	/// <para>If the return value is <see langword="true"/>, this argument will be -1.</para>
	/// </param>
	/// <returns>A <see cref="bool"/> value indicating that.</returns>
	/// <exception cref="InvalidOperationException">Throws when the puzzle is invalid (i.e. not unique).</exception>
	public static bool CheckMinimal(this in Grid @this, out Candidate firstCandidateMakePuzzleNotMinimal)
	{
		if (!@this.IsValid)
		{
			throw new InvalidOperationException(SR.ExceptionMessage("GridMultipleSolutions"));
		}

		switch (@this)
		{
			case { IsSolved: true, GivenCells.Count: 81 }:
			{
				// Very special case: all cells are givens.
				// The puzzle is considered not a minimal puzzle, because any digit in the grid can be removed.
				firstCandidateMakePuzzleNotMinimal = @this.GetDigit(0);
				return false;
			}
			default:
			{
				var gridCopied = @this.UnfixedGrid;
				foreach (var cell in gridCopied.ModifiableCells)
				{
					var newGrid = gridCopied;
					newGrid.SetDigit(cell, -1);
					newGrid.Fix();

					if (newGrid.IsValid)
					{
						firstCandidateMakePuzzleNotMinimal = cell * 9 + @this.GetDigit(cell);
						return false;
					}
				}

				firstCandidateMakePuzzleNotMinimal = -1;
				return true;
			}
		}
	}
}
