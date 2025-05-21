namespace Sudoku.Analytics;

/// <summary>
/// Indicates an error that throws when a solving step is wrong (may be due to wrong algorithm, bug, etc.).
/// </summary>
/// <param name="grid"><inheritdoc/></param>
/// <param name="wrongStep"><inheritdoc cref="WrongStep" path="/summary"/></param>
public sealed class WrongStepException(in Grid grid, Step wrongStep) : RuntimeAnalysisException(grid)
{
	/// <inheritdoc/>
	public override string Message => string.Format(SR.Get("Message_WrongStepException"), [InvalidGrid, WrongStep]);

	/// <summary>
	/// Indicates the wrong step.
	/// </summary>
	public Step WrongStep { get; } = wrongStep;
}
