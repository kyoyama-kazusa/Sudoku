namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with a <b>Grouped Chain</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Grouped Alternating Inference Chains</item>
/// <item>Grouped Continuous Nice Loops</item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_GroupedChainStepSearcher",
	// 3-Chains
	Technique.GroupedSkyscraper, Technique.GroupedTwoStringKite, Technique.GroupedTurbotFish,

	// 5-Chains
	Technique.GroupedWWing, Technique.GroupedMWing,
	Technique.GroupedSWing, Technique.GroupedLWing, Technique.GroupedHWing,
	Technique.GroupedPurpleCow,

	// Chains
	Technique.GroupedXChain, Technique.XyChain, Technique.GroupedXyXChain,
	Technique.GroupedAlternatingInferenceChain, Technique.GroupedDiscontinuousNiceLoop,

	// Overlapping
	Technique.GroupedSelfConstraint, Technique.NodeCollision,

	// Loops
	Technique.GroupedContinuousNiceLoop, Technique.GroupedXyCycle, Technique.GroupedFishyCycle)]
public sealed partial class GroupedChainStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates whether conclusions will firstly aim to backdoors.
	/// </summary>
	[SettingItemName(SettingItemNames.MakeConclusionAroundBackdoorsGroupedChain)]
	public bool MakeConclusionAroundBackdoors { get; set; }


	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		// Test examples:
		// AAR Loop
		// 8+2+6.374+1...5...+6.8...6...2+3+41..2..5+6..8+1693+4..+63..+4.+816+8.........4..+68...9+2..5+1+64:921 135 935 937 744 561 761 767 974 977 779 381 781 785 589

		var accumulator = new SortedSet<NormalChainStep>();
		if (ChainingDriver.CollectCore(ref context, accumulator, true, MakeConclusionAroundBackdoors) is { } step)
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
