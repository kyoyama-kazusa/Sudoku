namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with a <b>Bi-value Universal Grave + n Forcing Chains</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Bi-value Univerasl Grave + n Forcing Chains</item>
/// </list>
/// </summary>
[StepSearcher("StepSearcherName_BivalueUniversalGraveForcingChainsStepSearcher", Technique.BivalueUniversalGravePlusNForcingChains)]
public sealed partial class BivalueUniversalGraveForcingChainsStepSearcher : StepSearcher
{
	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		var accumulator = new SortedSet<ChainStep>();
		if (ChainingDriver.CollectBivalueUniversalGraveMultipleCore(ref context, accumulator, true, false) is { } step)
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
