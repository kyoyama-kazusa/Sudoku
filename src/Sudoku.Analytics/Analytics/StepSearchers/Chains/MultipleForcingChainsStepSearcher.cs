namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with a <b>Multiple Forcing Chains</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Cell Forcing Chains</item>
/// <item>Region Forcing Chains (i.e. House Forcing Chains)</item>
/// <item>Merged Cell Forcing Chains</item>
/// <item>Merged Region Forcing Chains (i.e. Merged House Forcing Chains)</item>
/// </list>
/// </summary>
[StepSearcher("StepSearcherName_MultipleForcingChainsStepSearcher", Technique.CellForcingChains, Technique.RegionForcingChains)]
public sealed partial class MultipleForcingChainsStepSearcher : StepSearcher
{
	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		// Test examples:
		// +27+1+896...+9+4352+768+1+8+56+3+147+9248.....2.+6+3.......51.......+3+9+5....7.+7+2+4.3+85.9168...+2+43

		var accumulator = new SortedSet<ChainStep>();
		if (ChainingDriver.CollectMultipleCore(ref context, accumulator, true, false) is { } step)
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
