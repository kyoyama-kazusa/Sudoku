namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with a <b>Rectangle Forcing Chains</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Rectangle Forcing Chains</item>
/// </list>
/// </summary>
[StepSearcher("StepSearcherName_RectangleForcingChainsStepSearcher", Technique.RectangleForcingChains)]
public sealed partial class RectangleForcingChainsStepSearcher : StepSearcher
{
	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		var accumulator = new SortedSet<ChainStep>();
		if (ChainingDriver.CollectRectangleMultipleCore(ref context, accumulator, true, false) is { } step)
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
