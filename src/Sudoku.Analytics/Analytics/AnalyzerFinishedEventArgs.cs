namespace Sudoku.Analytics;

/// <summary>
/// Provides extra data for event handler <see cref="Analyzer.Finished"/>.
/// </summary>
/// <param name="result">Indicates the result created.</param>
/// <seealso cref="Analyzer.Finished"/>
public sealed partial class AnalyzerFinishedEventArgs([Property] AnalysisResult result) : EventArgs;
