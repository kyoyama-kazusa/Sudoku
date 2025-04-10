namespace Sudoku.Analytics;

/// <summary>
/// Provides extra data for event handler <see cref="Analyzer.ExceptionThrown"/>.
/// </summary>
/// <param name="exception">Indicates the exception thrown.</param>
/// <seealso cref="Analyzer.ExceptionThrown"/>
public sealed partial class AnalyzerExceptionThrownEventArgs([Property] Exception exception) : EventArgs;
