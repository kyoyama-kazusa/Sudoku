namespace Sudoku.Concepts;

/// <summary>
/// Represents a list of methods to filter the cells.
/// </summary>
[DebuggerStepThrough]
internal static class GridPredicates
{
	/// <summary>
	/// Determines whether the specified cell in the specified grid is a given cell.
	/// </summary>
	/// <typeparam name="TGrid">The type of the grid.</typeparam>
	/// <param name="g">The grid.</param>
	/// <param name="cell">The cell to be checked.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool GivenCells<TGrid>(in TGrid g, Cell cell) where TGrid : unmanaged, IGrid<TGrid>
		=> g.GetState(cell) == CellState.Given;

	/// <summary>
	/// Determines whether the specified cell in the specified grid is a modifiable cell.
	/// </summary>
	/// <inheritdoc cref="GivenCells{TGrid}(in TGrid, Cell)"/>
	public static bool ModifiableCells<TGrid>(in TGrid g, Cell cell) where TGrid : unmanaged, IGrid<TGrid>
		=> g.GetState(cell) == CellState.Modifiable;

	/// <summary>
	/// Determines whether the specified cell in the specified grid is an empty cell.
	/// </summary>
	/// <inheritdoc cref="GivenCells{TGrid}(in TGrid, Cell)"/>
	public static bool EmptyCells<TGrid>(in TGrid g, Cell cell) where TGrid : unmanaged, IGrid<TGrid>
		=> g.GetState(cell) == CellState.Empty;

	/// <summary>
	/// Determines whether the specified cell in the specified grid is a bi-value cell, which means the cell is an empty cell,
	/// and contains and only contains 2 candidates.
	/// </summary>
	/// <inheritdoc cref="GivenCells{TGrid}(in TGrid, Cell)"/>
	public static bool BivalueCells<TGrid>(in TGrid g, Cell cell) where TGrid : unmanaged, IGrid<TGrid>
		=> BitOperations.PopCount(g.GetCandidates(cell)) == 2;

	/// <summary>
	/// Checks the existence of the specified digit in the specified cell.
	/// </summary>
	/// <typeparam name="TGrid">The type of the grid.</typeparam>
	/// <param name="g">The grid.</param>
	/// <param name="cell">The cell to be checked.</param>
	/// <param name="digit">The digit to be checked.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool CandidatesMap<TGrid>(in TGrid g, Cell cell, Digit digit) where TGrid : unmanaged, IGrid<TGrid>
		=> g.Exists(cell, digit) is true;

	/// <summary>
	/// Checks the existence of the specified digit in the specified cell, or whether the cell is a value cell, being filled by the digit.
	/// </summary>
	/// <inheritdoc cref="GivenCells{TGrid}(in TGrid, Cell)"/>
	public static bool DigitsMap<TGrid>(in TGrid g, Cell cell, Digit digit) where TGrid : unmanaged, IGrid<TGrid>
		=> (g.GetCandidates(cell) >> digit & 1) != 0;

	/// <summary>
	/// Checks whether the cell is a value cell, being filled by the digit.
	/// </summary>
	/// <inheritdoc cref="CandidatesMap{TGrid}(in TGrid, Cell, Digit)"/>
	public static bool ValuesMap<TGrid>(in TGrid g, Cell cell, Digit digit) where TGrid : unmanaged, IGrid<TGrid>
		=> g.GetDigit(cell) == digit;
}
