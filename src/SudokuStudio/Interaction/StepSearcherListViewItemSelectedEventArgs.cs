namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by delegate type <see cref="TechniqueGroupViewStepAppliedEventHandler"/>.
/// </summary>
/// <param name="selectedSearcherInfo"><inheritdoc cref="SelectedSearcherInfo" path="/summary"/></param>
/// <seealso cref="TechniqueGroupViewStepAppliedEventHandler"/>
public sealed class StepSearcherListViewItemSelectedEventArgs(StepSearcherInfo selectedSearcherInfo) : EventArgs
{
	/// <summary>
	/// The selected searcher's information.
	/// </summary>
	public StepSearcherInfo SelectedSearcherInfo { get; } = selectedSearcherInfo;
}
