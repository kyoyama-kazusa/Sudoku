namespace Sudoku.Solving;

/// <summary>
/// Provides extra data for event <see cref="ISolutionEnumerableSolver.SolutionFound"/>.
/// </summary>
/// <param name="solution"><inheritdoc cref="Solution" path="/summary"/></param>
/// <seealso cref="ISolutionEnumerableSolver.SolutionFound"/>
public sealed class SolverSolutionFoundEventArgs(in Grid solution) : EventArgs
{
	/// <summary>
	/// Indicates the target solution.
	/// </summary>
	public Grid Solution { get; } = solution;
}
