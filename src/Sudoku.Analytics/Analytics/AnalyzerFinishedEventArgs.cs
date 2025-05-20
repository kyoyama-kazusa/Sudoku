namespace Sudoku.Analytics;

/// <summary>
/// Provides extra data for event handler <see cref="Analyzer.Finished"/>.
/// </summary>
/// <param name="result"><inheritdoc cref="Result" path="/summary"/></param>
/// <seealso cref="Analyzer.Finished"/>
public sealed class AnalyzerFinishedEventArgs(AnalysisResult result) : EventArgs
{
	/// <summary>
	/// Indicates the result created.
	/// </summary>
	public AnalysisResult Result { get; } = result;
}
