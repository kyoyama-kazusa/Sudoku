namespace Sudoku.MinlexOrder;

/// <summary>
/// Indicates a data structure that describes the cell and label handled.
/// </summary>
internal struct Mapper
{
	/// <summary>
	/// Indicates the cell <see cref="byte"/> values.
	/// </summary>
	public safe fixed byte Cell[81];

	/// <summary>
	/// Indicates the label <see cref="byte"/> values.
	/// </summary>
	public safe fixed byte Label[10];
}
