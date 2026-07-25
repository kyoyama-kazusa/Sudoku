namespace Sudoku.Generating.Qualified;

/// <summary>
/// Represents a generator that supports generating for puzzles that can be solved by <b>only</b> using Direct Single techniques.
/// </summary>
public closed class SingleGenerator : PrimaryGenerator
{
	/// <summary>
	/// Indicates the symmetric type to be set.
	/// </summary>
	public SymmetricType SymmetricType { get; set; }

	/// <summary>
	/// Indicates the number of empty cells the current generator will generate on puzzles.
	/// </summary>
	public Cell EmptyCellsCount { get; set; }


	/// <summary>
	/// Returns a new <see cref="Cell"/> value indicating the valid empty cells count.
	/// </summary>
	/// <returns>The result valid empty cells count.</returns>
	protected Cell GetValidEmptyCellsCount() => EmptyCellsCount == -1 ? -1 : Math.Clamp(EmptyCellsCount, 1, 64);
}
