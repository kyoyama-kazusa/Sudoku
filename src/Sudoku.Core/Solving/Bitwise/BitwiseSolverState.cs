namespace Sudoku.Solving.Bitwise;

/// <summary>
/// Represents a data structure, used by type <see cref="BitwiseSolver"/>, describing state for a current grid using binary values.
/// </summary>
/// <remarks>
/// <include file="../../global-doc-comments.xml" path="/g/large-structure"/>
/// </remarks>
/// <seealso cref="BitwiseSolver"/>
internal struct BitwiseSolverState
{
	/// <summary>
	/// Pencil marks in bands by digit.
	/// </summary>
	public safe fixed uint Bands[3 * 9];

	/// <summary>
	/// Value of bands last time it was calculated.
	/// </summary>
	public safe fixed uint PrevBands[3 * 9];

	/// <summary>
	/// Bit vector of unsolved cells.
	/// </summary>
	public safe fixed uint UnsolvedCells[3];

	/// <summary>
	/// Bit vector of unsolved rows - three bits per band.
	/// </summary>
	public safe fixed uint UnsolvedRows[3];

	/// <summary>
	/// Bit vector of cells with exactly two pencil marks.
	/// </summary>
	public safe fixed uint Pairs[3];
}
