namespace Sudoku.Analytics.StepSearchers.Chains;

/// <summary>
/// Provides with a <b>Chain</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Alternating Inference Chains</item>
/// <item>Continuous Nice Loops</item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_ChainStepSearcher",
	// 3-Chains
	Technique.Skyscraper, Technique.TwoStringKite, Technique.TurbotFish,

	// 5-Chains
	Technique.WWing, Technique.MWing, Technique.SWing, Technique.LWing, Technique.HWing, Technique.PurpleCow,

	// Chains
	Technique.XChain, Technique.XyChain, Technique.XyXChain,
	Technique.AlternatingInferenceChain, Technique.DiscontinuousNiceLoop,

	// Overlapping
	Technique.SelfConstraint,

	// Loops
	Technique.ContinuousNiceLoop, Technique.XyCycle, Technique.FishyCycle,

	RuntimeFlags = StepSearcherRuntimeFlags.SkipVerification)]
public sealed partial class ChainStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates whether conclusions will firstly aim to backdoors.
	/// </summary>
	[SettingItemName(SettingItemNames.MakeConclusionAroundBackdoorsNormalChain)]
	public bool MakeConclusionAroundBackdoors { get; set; }


	/// <inheritdoc/>
	/// <remarks>
	/// <include file="../../global-doc-comments.xml" path="/g/developer-notes" />
	/// <para>
	/// All implementations are extracted in type <see cref="ChainingDriver"/>. Please visit it to learn more information.
	/// </para>
	/// </remarks>
	/// <seealso cref="ChainingDriver"/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		var accumulator = new SortedSet<NormalChainStep>();
		if (ChainingDriver.CollectCore(ref context, accumulator, false, MakeConclusionAroundBackdoors) is { } step)
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
