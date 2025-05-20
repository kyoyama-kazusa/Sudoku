namespace Sudoku.Analytics;

/// <summary>
/// Provides extra data for event handler <see cref="Analyzer.StepFound"/>.
/// </summary>
/// <param name="step"><inheritdoc cref="Step" path="/summary"/></param>
/// <seealso cref="Analyzer.StepFound"/>
public sealed class AnalyzerStepFoundEventArgs(Step step) : EventArgs
{
	/// <summary>
	/// Indicates the found step.
	/// </summary>
	public Step Step { get; } = step;
}
