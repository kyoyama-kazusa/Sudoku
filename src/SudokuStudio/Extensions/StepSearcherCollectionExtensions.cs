namespace SudokuStudio;

/// <summary>
/// Represents extension methods on step searcher collection.
/// </summary>
public static class StepSearcherCollectionExtensions
{
	/// <summary>
	/// Try to get <see cref="StepSearcher"/> instances via configuration for the specified application.
	/// </summary>
	/// <param name="this">The application.</param>
	/// <returns>A list of <see cref="StepSearcher"/> instances.</returns>
	public static StepSearcher[] GetStepSearchers(this App @this)
		=> [
			..
			from data in @this.Preference.StepSearcherOrdering.StepSearchersOrder
			where data.IsEnabled
			select data.CreateStepSearcher()
		];
}
