namespace Sudoku.Analytics;

/// <summary>
/// Represents a progress used by <see cref="StepGatherer"/> instances.
/// </summary>
/// <param name="StepSearcherName">Indicates the currently used step searcher.</param>
/// <param name="Percent">The percent value.</param>
/// <seealso cref="StepGatherer"/>
public readonly record struct StepGathererProgressPresenter(string StepSearcherName, double Percent);
