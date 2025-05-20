namespace Sudoku.Analytics;

/// <summary>
/// Provides extra data for event handler <see cref="Analyzer.ExceptionThrown"/>.
/// </summary>
/// <param name="exception"><inheritdoc cref="Exception" path="/summary"/></param>
/// <seealso cref="Analyzer.ExceptionThrown"/>
public sealed class AnalyzerExceptionThrownEventArgs(Exception exception) : EventArgs
{
	/// <summary>
	/// Indicates the exception thrown.
	/// </summary>
	public Exception Exception { get; } = exception;
}
