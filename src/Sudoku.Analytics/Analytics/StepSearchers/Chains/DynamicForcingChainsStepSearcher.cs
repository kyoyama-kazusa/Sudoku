namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with a <b>Dynamic Forcing Chains</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Dynamic Cell Forcing Chains</item>
/// <item>Dynamic Region Forcing Chains (i.e. Dynamic House Forcing Chains)</item>
/// <item>Dynamic Contradiction Forcing Chains</item>
/// <item>Dynamic Double Forcing Chains</item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_DynamicForcingChainsStepSearcher",
	Technique.DynamicCellForcingChains, Technique.DynamicRegionForcingChains,
	Technique.DynamicContradictionForcingChains, Technique.DynamicDoubleForcingChains)]
public sealed partial class DynamicForcingChainsStepSearcher : StepSearcher
{
	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		// Test examples:
		// Dynamic Contradiction Forcing Chains
		// 9876.....65....8....4.......7.9..4......3..5......2..1..68..7......5...3.....1.2.:424 724 234 334 734 135 235 336 536 637 937 138 338 239 539 841 843 646 657 259 368 471 472 972 979 486 986 188 688 599
		//
		// Dynamic Double Forcing Chains
		// .....1..2.3..2..4.5..6...7...6.....7.7..1..3.8.3...5..7....5.......3.72.3.987.6..:818 918 129 829 929 935 836 337 242 244 444 944 945 246 946 847 947 253 254 256 456 957 459 959 262 464 964 466 666 966 669 969 872 873 475 177 877 978 179 479 979 482 483 189 889

		var accumulator = new List<PatternBasedChainStep>();
		if (ChainingDriver.CollectDynamicForcingChainsCore(ref context, accumulator) is { } step)
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
