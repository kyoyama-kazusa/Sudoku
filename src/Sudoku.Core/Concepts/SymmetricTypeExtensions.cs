namespace Sudoku.Concepts;

/// <summary>
/// Provides with extension methods on <see cref="SymmetricType"/>.
/// </summary>
/// <seealso cref="SymmetricType"/>
public static class SymmetricTypeExtensions
{
	/// <summary>
	/// Try to get the number of cells that the current symmetry should be formed a complete symmetric pattern.
	/// </summary>
	/// <param name="this">The symmetric type.</param>
	/// <returns>
	/// The number of cells should form a complete pattern:
	/// <list type="table">
	/// <listheader>
	/// <term>Argument</term>
	/// <description>Return value</description>
	/// </listheader>
	/// <item>
	/// <term><see cref="SymmetricType.None"/> (0)</term>
	/// <description>1 (Itself)</description>
	/// </item>
	/// <item>
	/// <term>
	/// <see cref="SymmetricType.Central"/>,
	/// <see cref="SymmetricType.Diagonal"/>, <see cref="SymmetricType.AntiDiagonal"/>,
	/// <see cref="SymmetricType.XAxis"/>, <see cref="SymmetricType.YAxis"/>
	/// </term>
	/// <description>2</description>
	/// </item>
	/// <item>
	/// <term>The other defined values</term>
	/// <description>4</description>
	/// </item>
	/// <item>
	/// <term>Otherwise</term>
	/// <description><see cref="ArgumentOutOfRangeException"/> thrown</description>
	/// </item>
	/// </list>
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when the argument is not defined.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetSymmetryCellsCount(this SymmetricType @this)
		=> @this switch
		{
			SymmetricType.None => 1, // Itself
			> SymmetricType.None and < SymmetricType.AxisBoth => 2,
			SymmetricType.AxisBoth or SymmetricType.DiagonalBoth or SymmetricType.All => 4,
			_ => throw new ArgumentOutOfRangeException(nameof(@this))
		};

	/// <summary>
	/// Try to get the number of axes of the specified symmetric type.
	/// </summary>
	/// <param name="this">The symmetry.</param>
	/// <returns>
	/// The number of axes the current symmetric type contains.
	/// If <paramref name="this"/> is <see cref="SymmetricType.None"/>, -1 will be returned.
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when the argument <paramref name="this"/> is not defined.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetAxisDimension(this SymmetricType @this)
		=> @this switch
		{
			SymmetricType.None => -1,
			SymmetricType.Central => 0,
			SymmetricType.Diagonal => 1,
			SymmetricType.AntiDiagonal => 1,
			SymmetricType.XAxis => 1,
			SymmetricType.YAxis => 1,
			SymmetricType.AxisBoth => 2,
			SymmetricType.DiagonalBoth => 2,
			SymmetricType.All => 4,
			_ => throw new ArgumentOutOfRangeException(nameof(@this))
		};

	/// <summary>
	/// Gets the name of thr symmetry.
	/// </summary>
	/// <param name="this">The symmetry value.</param>
	/// <param name="formatProvider">The culture.</param>
	/// <returns>The string.</returns>
	/// <exception cref="InvalidOperationException">Throws when the argument holds multiple flag values.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetName(this SymmetricType @this, IFormatProvider? formatProvider)
		=> BitOperations.PopCount((int)@this) < 2
			? SR.Get($"SymmetricType_{@this}", formatProvider as CultureInfo)
			: throw new InvalidOperationException(SR.ExceptionMessage("MultipleFlagsExist"));

	/// <summary>
	/// Try to get all possible cells in symmetry axis (or point).
	/// </summary>
	/// <param name="this">The symmetry.</param>
	/// <returns>Returns cells in the symmetry axis (or point).</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the argument <paramref name="this"/> is not defined or <see cref="SymmetricType.None"/>.
	/// </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CellMap GetCellsInSymmetryAxis(this SymmetricType @this)
		=> @this switch
		{
			SymmetricType.Central => [40],
			SymmetricType.Diagonal => [0, 10, 20, 30, 40, 50, 60, 70, 80],
			SymmetricType.AntiDiagonal => [8, 16, 24, 32, 40, 48, 56, 64, 72],
			SymmetricType.XAxis => [36, 37, 38, 39, 40, 41, 42, 43, 44],
			SymmetricType.YAxis => [4, 13, 22, 31, 40, 49, 58, 67, 76],
			SymmetricType.AxisBoth => SymmetricType.XAxis.GetCellsInSymmetryAxis() | SymmetricType.YAxis.GetCellsInSymmetryAxis(),
			SymmetricType.DiagonalBoth => SymmetricType.Diagonal.GetCellsInSymmetryAxis() | SymmetricType.AntiDiagonal.GetCellsInSymmetryAxis(),
			SymmetricType.All => SymmetricType.AxisBoth.GetCellsInSymmetryAxis() | SymmetricType.DiagonalBoth.GetCellsInSymmetryAxis(),
			SymmetricType.None => [],
			_ => throw new ArgumentOutOfRangeException(nameof(@this))
		};

	/// <inheritdoc cref="GetCells(SymmetricType, RowIndex, ColumnIndex)"/>
	/// <param name="this"><inheritdoc cref="GetCells(SymmetricType, RowIndex, ColumnIndex)"/></param>
	/// <param name="cell">Indicates the target cell to be checked.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CellMap GetCells(this SymmetricType @this, Cell cell) => @this.GetCells(cell / 9, cell % 9);

	/// <summary>
	/// Get the cells that is used for swapping via the specified symmetric type, and the specified row and column value.
	/// </summary>
	/// <param name="this">The symmetric type.</param>
	/// <param name="row">The row value.</param>
	/// <param name="column">The column value.</param>
	/// <returns>The cells.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CellMap GetCells(this SymmetricType @this, RowIndex row, ColumnIndex column)
		=> @this switch
		{
			SymmetricType.Central => [row * 9 + column, (8 - row) * 9 + 8 - column],
			SymmetricType.Diagonal => [row * 9 + column, column * 9 + row],
			SymmetricType.AntiDiagonal => [row * 9 + column, (8 - column) * 9 + 8 - row],
			SymmetricType.XAxis => [row * 9 + column, (8 - row) * 9 + column],
			SymmetricType.YAxis => [row * 9 + column, row * 9 + 8 - column],
			SymmetricType.DiagonalBoth => [row * 9 + column, column * 9 + row, (8 - column) * 9 + 8 - row, (8 - row) * 9 + 8 - column],
			SymmetricType.AxisBoth => [row * 9 + column, (8 - row) * 9 + column, row * 9 + 8 - column, (8 - row) * 9 + 8 - column],
			SymmetricType.All => [
				row * 9 + column,
				row * 9 + (8 - column),
				(8 - row) * 9 + column,
				(8 - row) * 9 + (8 - column),
				column * 9 + row,
				column * 9 + (8 - row),
				(8 - column) * 9 + row,
				(8 - column) * 9 + (8 - row)
			],
			SymmetricType.None => [row * 9 + column],
			_ => []
		};
}
