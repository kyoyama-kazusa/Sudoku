namespace Sudoku.Solving;

/// <summary>
/// Represents a solver that can find all possible solutions with detection on every operation of new solution found.
/// </summary>
public interface ISolutionEnumerableSolver
{
	/// <summary>
	/// Provide a way to detect event to be triggered when a solution is found;
	/// no matter whether the puzzle has a unique solution or not (multiple solutions).
	/// </summary>
	public abstract event EventHandler<SolverSolutionFoundEventArgs>? SolutionFound;


	/// <summary>
	/// Try to perform enumeration on solutions.
	/// </summary>
	/// <param name="grid">The grid to be solved.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	public abstract void EnumerateSolutionsCore(Grid grid, CancellationToken cancellationToken);
}
