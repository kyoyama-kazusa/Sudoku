namespace Sudoku.Analytics;

/// <summary>
/// Provides extra data for event handler <see cref="Analyzer.StepFound"/>.
/// </summary>
/// <param name="step">Indicates the found step.</param>
/// <seealso cref="Analyzer.StepFound"/>
public sealed partial class AnalyzerStepFoundEventArgs([Property] Step step) : EventArgs;
