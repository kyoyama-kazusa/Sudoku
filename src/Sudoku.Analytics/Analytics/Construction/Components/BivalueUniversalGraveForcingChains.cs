namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Represents a multiple forcing chains that is applied to a bi-value universal grave + n.
/// </summary>
/// <param name="trueCandidates"><inheritdoc cref="TrueCandidates" path="/summary"/></param>
/// <param name="conclusions"><inheritdoc cref="MultipleForcingChains(Conclusion[])" path="/param[@name='conclusions']"/></param>
public sealed partial class BivalueUniversalGraveForcingChains(in CandidateMap trueCandidates, params Conclusion[] conclusions) :
	MultipleForcingChains(conclusions)
{
	/// <inheritdoc/>
	public override bool IsCellMultiple => false;

	/// <inheritdoc/>
	public override bool IsHouseMultiple => false;

	/// <inheritdoc/>
	public override bool IsAdvancedMultiple => true;

	/// <summary>
	/// Indicates all true candidates.
	/// </summary>
	public CandidateMap TrueCandidates { get; } = trueCandidates;


	/// <inheritdoc/>
	protected internal override void PrepareFinnedChainViewNodes(
		NamedChain finnedChain,
		ChainingRuleCollection supportedRules,
		in Grid grid,
		in CandidateMap fins,
		out View[] views
	)
	{
		base.PrepareFinnedChainViewNodes(finnedChain, supportedRules, grid, fins, out views);
		foreach (var candidate in Candidates)
		{
			var node = new CandidateViewNode(ColorIdentifier.Auxiliary1, candidate);
			foreach (var view in views)
			{
				view.Add(node);
			}
		}
	}

	/// <inheritdoc/>
	protected override ReadOnlySpan<ViewNode> GetInitialViewNodes(in Grid grid)
		=> from candidate in TrueCandidates select (ViewNode)new CandidateViewNode(ColorIdentifier.Auxiliary1, candidate);
}
