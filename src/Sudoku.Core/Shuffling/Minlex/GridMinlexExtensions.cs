namespace Sudoku.Shuffling.Minlex;

/// <summary>
/// Provides with extension methods on <see cref="Grid"/> for minlex.
/// </summary>
/// <seealso cref="Grid"/>
public static class GridMinlexExtensions
{
	/// <summary>
	/// Provides extension members on <see langword="ref"/> <see cref="Grid"/>.
	/// </summary>
	extension(ref Grid @this)
	{
		/// <summary>
		/// Adjust the grid to minimal lexicographical form.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MakeMinLex() => @this = @this.MinLexGrid;
	}

	/// <summary>
	/// Provides extension members on <see langword="in"/> <see cref="Grid"/>.
	/// </summary>
	extension(in Grid @this)
	{
		/// <summary>
		/// Indicates whether the current grid is the minimal lexicographical form,
		/// meaning the corresponding string text code is the minimum value in all equivalent transforming cases
		/// in lexicographical order.
		/// </summary>
		public bool IsMinLex
			=> @this.PuzzleType != SudokuType.Sukaku && @this.Uniqueness == Uniqueness.Unique && @this.ToString("0") is var s
				? new MinlexFinder().Find(s) == s
				: throw new InvalidOperationException(SR.ExceptionMessage("MinLexShouldBeUniqueAndNotSukaku"));

		/// <summary>
		/// Indicates the new grid form of the current grid, which is at the minimal-lexicographical order.
		/// </summary>
		public Grid MinLexGrid => new MinlexFinder().Find(in @this);


		/// <summary>
		/// Determine whether the specified <see cref="Grid"/> instance hold the same values as the current instance,
		/// by using the specified comparison type.
		/// </summary>
		/// <param name="other">The instance to compare.</param>
		/// <param name="comparisonType">One of the enumeration values that specifies the rules for the comparison.</param>
		/// <returns>A <see cref="bool"/> result.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Throws when the argument <paramref name="comparisonType"/> is not defined.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(in Grid other, BoardComparison comparisonType)
			=> comparisonType switch
			{
				BoardComparison.Default => @this.Equals(other),
				BoardComparison.IncludingTransforms => @this.MinLexGrid == other.MinLexGrid,
				_ => throw new ArgumentOutOfRangeException(nameof(comparisonType))
			};

		/// <inheritdoc cref="Grid.GetHashCode"/>
		/// <param name="comparisonType">
		/// Indicates the comparison type that specifies the target grid to be calculated its hash code.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Throws when the argument <paramref name="comparisonType"/> isn't defined.
		/// </exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetHashCode(BoardComparison comparisonType)
		{
			var grid = comparisonType switch
			{
				BoardComparison.Default => @this,
				BoardComparison.IncludingTransforms => @this.MinLexGrid,
				_ => throw new ArgumentOutOfRangeException(nameof(comparisonType))
			};
			return grid.GetHashCode();
		}

		/// <summary>
		/// Compares the current instance with another object of the same type and returns an integer
		/// that indicates whether the current instance precedes, follows or occurs in the same position in the sort order as the other object.
		/// </summary>
		/// <param name="other">The other object to be compared.</param>
		/// <param name="comparisonType">The comparison type to be used.</param>
		/// <returns>A value that indicates the relative order of the objects being compared.</returns>
		/// <exception cref="InvalidOperationException">Throws when one of the grids to be compared is a Sukaku puzzle.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Throws when the argument <paramref name="comparisonType"/> is not defined.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(in Grid other, BoardComparison comparisonType)
			=> (@this.PuzzleType, other.PuzzleType) switch
			{
				(not SudokuType.Sukaku, not SudokuType.Sukaku) => comparisonType switch
				{
					BoardComparison.Default => @this.ToString("#").CompareTo(other.ToString("#")),
					BoardComparison.IncludingTransforms => @this.MinLexGrid.ToString("#").CompareTo(other.MinLexGrid.ToString("#")),
					_ => throw new ArgumentOutOfRangeException(nameof(comparisonType))
				},
				_ => throw new InvalidOperationException(SR.ExceptionMessage("ComparableGridMustBeStandard"))
			};
	}
}
