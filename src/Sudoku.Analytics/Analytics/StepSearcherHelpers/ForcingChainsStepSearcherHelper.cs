namespace Sudoku.Analytics.StepSearcherHelpers;

/// <summary>
/// Represents a type that can search for forcing chains and its related techniques.
/// </summary>
internal closed class ForcingChainsStepSearcherHelper : ChainingStepSearcherHelper
{
	/// <summary>
	/// The internal method that can collect for general-typed multiple forcing chains.
	/// </summary>
	/// <typeparam name="TMultipleForcingChains">The type of multiple forcing chains.</typeparam>
	/// <typeparam name="TMultipleForcingChainsStep">The type of target step can be created.</typeparam>
	/// <param name="context">The context.</param>
	/// <param name="accumulator">The instance that temporarily records for chain steps.</param>
	/// <param name="allowsAdvancedLinks">Indicates whether the method allows advanced links.</param>
	/// <param name="onlyFindFinnedChain">Indicates whether the method only finds for (grouped) finned chains.</param>
	/// <param name="componentCreator">Indicates the component that the current forcing chains pattern is.</param>
	/// <param name="chainsCollector">
	/// The collector method that can find a list of <typeparamref name="TMultipleForcingChains"/> instances in the grid.
	/// </param>
	/// <param name="stepCreator">The creator method that can create a chain step.</param>
	/// <returns>The first found step.</returns>
	protected static unsafe Step? CollectGeneralizedMultipleCore<TMultipleForcingChains, TMultipleForcingChainsStep>(
		ref StepAnalysisContext context,
		SortedSet<ChainStep> accumulator,
		bool allowsAdvancedLinks,
		bool onlyFindFinnedChain,
		delegate*<TMultipleForcingChains, MultipleChainBasedComponent> componentCreator,
		delegate*<in Grid, bool, ReadOnlySpan<TMultipleForcingChains>> chainsCollector,
		delegate*<TMultipleForcingChains, in Grid, ref readonly StepAnalysisContext, ChainingRuleCollection, TMultipleForcingChainsStep> stepCreator
	)
		where TMultipleForcingChains : MultipleForcingChains
		where TMultipleForcingChainsStep : PatternBasedChainStep
	{
		ref readonly var grid = ref context.Grid;
		InitializeLinks(
			grid,
			LinkType.MergeFlags([.. ChainingRule.ElementaryLinkTypes, .. allowsAdvancedLinks ? ChainingRule.AdvancedLinkTypes : []]),
			context.Options,
			out var supportedRules
		);

		foreach (var chain in chainsCollector(context.Grid, context.OnlyFindOne))
		{
			if (onlyFindFinnedChain && chain.TryCastToFinnedChain(out var finnedChain, out var f))
			{
				ref readonly var fins = ref Nullable.GetValueRefOrDefaultRef(in f);
				chain.PrepareFinnedChainViewNodes(finnedChain, supportedRules, grid, fins, out var views);

				var finnedChainStep = new FinnedChainStep(
					chain.Conclusions,
					views,
					context.Options,
					finnedChain,
					fins,
					componentCreator(chain)
				);
				if (finnedChain.IsStrictlyGrouped ^ allowsAdvancedLinks)
				{
					continue;
				}

				if (context.OnlyFindOne)
				{
					return finnedChainStep;
				}

				accumulator.Add(finnedChainStep);
				continue;
			}

			if (!onlyFindFinnedChain)
			{
				var rfcStep = stepCreator(chain, grid, in context, supportedRules);
				if (context.OnlyFindOne)
				{
					return rfcStep;
				}

				accumulator.Add(rfcStep);
			}
		}
		return null;
	}

	/// <summary>
	/// <para>Finds a list of nodes that can implicitly connects to current node via a forcing chain.</para>
	/// <para>This method only uses cached fields <see cref="StrongLinkDictionary"/> and <see cref="WeakLinkDictionary"/>.</para>
	/// </summary>
	/// <param name="startNode">The current instance.</param>
	/// <returns>
	/// A pair of <see cref="HashSet{T}"/> of <see cref="Node"/> instances, indicating all possible nodes
	/// that can implicitly connects to the current node via the whole forcing chain, grouped by their own initial states,
	/// encapsulating with type <see cref="ForcingChainsInfo"/>.
	/// </returns>
	/// <seealso cref="StrongLinkDictionary"/>
	/// <seealso cref="WeakLinkDictionary"/>
	/// <seealso cref="HashSet{T}"/>
	/// <seealso cref="Node"/>
	/// <seealso cref="ForcingChainsInfo"/>
	protected static ForcingChainsInfo FindForcingChains(Node startNode)
	{
		var (pendingNodesSupposedOn, pendingNodesSupposedOff) = (new Queue<Node>(), new Queue<Node>());
		(startNode.IsOn ? pendingNodesSupposedOn : pendingNodesSupposedOff).Enqueue(startNode);

		var nodesSupposedOn = new HashSet<Node>(ChainingComparers.NodeMapComparer);
		var nodesSupposedOff = new HashSet<Node>(ChainingComparers.NodeMapComparer);
	chaining_bfs_loop:
		while (pendingNodesSupposedOn.Count != 0 || pendingNodesSupposedOff.Count != 0)
		{
			if (pendingNodesSupposedOn.Count != 0)
			{
				var currentNode = pendingNodesSupposedOn.Dequeue();
				if (WeakLinkDictionary.TryGetValue(currentNode, out var supposedOff))
				{
					foreach (var node in supposedOff)
					{
						var nextNode = Node.Create(node, currentNode);
						if (nodesSupposedOn.Contains(~nextNode))
						{
							// Contradiction is found.
							break chaining_bfs_loop;
						}

						if (nodesSupposedOff.Add(nextNode))
						{
							pendingNodesSupposedOff.Enqueue(nextNode);
						}
					}
				}
			}
			else
			{
				var currentNode = pendingNodesSupposedOff.Dequeue();
				if (StrongLinkDictionary.TryGetValue(currentNode, out var supposedOn))
				{
					foreach (var node in supposedOn)
					{
						var nextNode = Node.Create(node, currentNode);
						if (nodesSupposedOff.Contains(~nextNode))
						{
							// Contradiction is found.
							break chaining_bfs_loop;
						}

						if (nodesSupposedOn.Add(nextNode))
						{
							pendingNodesSupposedOn.Enqueue(nextNode);
						}
					}
				}
			}
		}

		return new(nodesSupposedOn, nodesSupposedOff);
	}
}
