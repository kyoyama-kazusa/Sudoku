namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with a <b>Grouped Finned Chain (with Unique Rectangle)</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Grouped Finned Chain</item>
/// </list>
/// </summary>
[StepSearcher("StepSearcherName_FinnedRectangleChainStepSearcher", Technique.FinnedGroupedChain)]
public sealed partial class FinnedRectangleChainStepSearcher : StepSearcher
{
	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		var accumulator = new SortedSet<ChainStep>();
		if (ChainingDriver.CollectRectangleMultipleCore(ref context, accumulator, true, true) is { } step)
		{
			return step;
		}

		if (!context.OnlyFindOne && accumulator.Count != 0)
		{
			context.Accumulator.AddRange(accumulator);
		}
		return null;
	}
}
