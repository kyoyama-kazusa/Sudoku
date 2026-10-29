namespace Sudoku.Analytics.StepSearcherHelpers;

/// <summary>
/// Represents a type that can search for bivalue universal grave forcing chains.
/// </summary>
internal sealed class BivalueUniversalGraveForcingChainsStepSearcherHelper : ForcingChainsStepSearcherHelper
{
	/// <inheritdoc/>
	public override ReadOnlyMemory<Type> SupportedStepSearcherTypes => (Type[])[typeof(MultipleForcingChainsStepSearcher)];


	/// <summary>
	/// The collect method called by rectangle forcing chains step searcher.
	/// </summary>
	/// <param name="context">The context.</param>
	/// <param name="accumulator">The instance that temporarily records for chain steps.</param>
	/// <param name="allowsAdvancedLinks">Indicates whether the method allows advanced links.</param>
	/// <param name="onlyFindFinnedChain">Indicates whether the method only finds for (grouped) finned chains.</param>
	/// <returns>The first found step.</returns>
	public static Step? CollectCore(
		ref StepAnalysisContext context,
		SortedSet<ChainStep> accumulator,
		bool allowsAdvancedLinks,
		bool onlyFindFinnedChain
	)
	{
		return unsafe (
			CollectGeneralizedMultipleCore(
				ref context,
				accumulator,
				allowsAdvancedLinks,
				onlyFindFinnedChain,
				&component,
				&CollectBivalueUniversalGraveMultipleForcingChains,
				&stepCreator
			)
		);


		static MultipleChainBasedComponent component(MultipleForcingChains mfc) => MultipleChainBasedComponent.BivalueUniversalGrave;

		static BivalueUniversalGraveForcingChainsStep stepCreator(
			BivalueUniversalGraveForcingChains chain,
			in Grid grid,
			ref readonly StepAnalysisContext context,
			ChainingRuleCollection supportedRules
		) => new(chain.Conclusions, ((IForcingChains)chain).GetViews(grid, chain.Conclusions, supportedRules), context.Options, chain);
	}

	/// <summary>
	/// Collect all multiple forcing chains on applying to a bi-value universal grave, appeared in a grid.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="onlyFindOne">Indicates whether the method only find one valid chain.</param>
	/// <returns>All possible multiple forcing chain instances.</returns>
	[InterceptorMethodCaller]
	public static ReadOnlySpan<BivalueUniversalGraveForcingChains> CollectBivalueUniversalGraveMultipleForcingChains(in Grid grid, bool onlyFindOne)
	{
		var result = new SortedSet<BivalueUniversalGraveForcingChains>(ChainingComparers.MultipleForcingChainsComparer);
		var trueCandidates = TrueCandidate.GetAllTrueCandidates(grid);
		if (!trueCandidates)
		{
			return [];
		}

		// Collect branches for all possible candidates to be iterated,
		// and determine whether at least one candidate can be considered as a conclusion from all different start candidates.
		var nodesSupposedOnGrouped = new Dictionary<Candidate, HashSet<Node>>();
		var nodesSupposedOffGrouped = new Dictionary<Candidate, HashSet<Node>>();
		var resultNodesSupposedOn = default(HashSet<Node>);
		var resultNodesSupposedOff = default(HashSet<Node>);
		foreach (var candidate in trueCandidates)
		{
			var currentNode = new Node(candidate.AsCandidateMap(), true);
			var (nodesSupposedOn, nodesSupposedOff) = FindForcingChains(currentNode);

			nodesSupposedOnGrouped.Add(candidate, nodesSupposedOn);
			nodesSupposedOffGrouped.Add(candidate, nodesSupposedOff);
			if (resultNodesSupposedOn is null)
			{
				resultNodesSupposedOn = [with(ChainingComparers.NodeMapComparer)];
				resultNodesSupposedOff = [with(ChainingComparers.NodeMapComparer)];
				resultNodesSupposedOn.UnionWith(nodesSupposedOn);
				resultNodesSupposedOff.UnionWith(nodesSupposedOff);
			}
			else
			{
				Debug.Assert(resultNodesSupposedOff is not null);
				resultNodesSupposedOn.IntersectWith(nodesSupposedOn);
				resultNodesSupposedOff.IntersectWith(nodesSupposedOff);
			}
		}

		var step1 = rfcOn(trueCandidates, grid, nodesSupposedOnGrouped, resultNodesSupposedOn);
		if (!step1.IsEmpty)
		{
			return step1;
		}

		var step2 = rfcOff(trueCandidates, grid, nodesSupposedOffGrouped, resultNodesSupposedOff);
		if (!step2.IsEmpty)
		{
			return step2;
		}

		return result.ToArray();


		ReadOnlySpan<BivalueUniversalGraveForcingChains> rfcOn(
			scoped in CandidateMap trueCandidates,
			in Grid grid,
			Dictionary<Candidate, HashSet<Node>> onNodes,
			HashSet<Node>? resultOnNodes
		)
		{
			foreach (var node in resultOnNodes ?? [])
			{
				if (node.IsGroupedNode)
				{
					// Grouped nodes are not supported as target node.
					continue;
				}

				var conclusion = new Conclusion(Assignment, node.Map[0]);
				if (grid.Exists(conclusion.Candidate) is not true)
				{
					continue;
				}

				var rfc = new BivalueUniversalGraveForcingChains(trueCandidates, conclusion);
				foreach (var candidate in trueCandidates)
				{
					var branchNode = onNodes[candidate].First(n => n.Equals(node, NodeComparison.IncludeIsOn));
					rfc.Add(candidate, node.IsOn ? new StrongForcingChain(branchNode) : new WeakForcingChain(branchNode));
				}
				if (onlyFindOne)
				{
					return (BivalueUniversalGraveForcingChains[])[rfc];
				}
				result.Add(rfc);
			}
			return [];
		}

		ReadOnlySpan<BivalueUniversalGraveForcingChains> rfcOff(
			scoped in CandidateMap trueCandidates,
			in Grid grid,
			Dictionary<Candidate, HashSet<Node>> offNodes,
			HashSet<Node>? resultOffNodes
		)
		{
			foreach (var node in resultOffNodes ?? [])
			{
				if (node.IsGroupedNode)
				{
					// Grouped nodes are not supported as target node.
					continue;
				}

				var conclusion = new Conclusion(Elimination, node.Map[0]);
				if (grid.Exists(conclusion.Candidate) is not true)
				{
					continue;
				}

				var rfc = new BivalueUniversalGraveForcingChains(trueCandidates);
				foreach (var candidate in trueCandidates)
				{
					var branchNode = offNodes[candidate].First(n => n.Equals(node, NodeComparison.IncludeIsOn));
					rfc.Add(candidate, node.IsOn ? new StrongForcingChain(branchNode) : new WeakForcingChain(branchNode));
				}
				if (rfc.GetThoroughConclusions(grid) is not { Length: not 0 } conclusions)
				{
					continue;
				}

				rfc.Conclusions = conclusions;
				if (onlyFindOne)
				{
					return (BivalueUniversalGraveForcingChains[])[rfc];
				}
				result.Add(rfc);
			}
			return [];
		}
	}
}
