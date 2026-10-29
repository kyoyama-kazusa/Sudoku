namespace Sudoku.Analytics.StepSearcherHelpers;

/// <summary>
/// Represents a type that can search for rectangle forcing chains.
/// </summary>
internal sealed class RectangleForcingChainsStepSearcherHelper : ForcingChainsStepSearcherHelper
{
	/// <inheritdoc/>
	public override ReadOnlyMemory<Type> SupportedStepSearcherTypes
		=> (Type[])[typeof(RectangleForcingChainsStepSearcher), typeof(FinnedRectangleChainStepSearcher)];


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
				&CollectRectangleMultipleForcingChains,
				&stepCreator
			)
		);


		static MultipleChainBasedComponent component(MultipleForcingChains mfc) => MultipleChainBasedComponent.Rectangle;

		static RectangleForcingChainsStep stepCreator(
			RectangleForcingChains chain,
			in Grid grid,
			ref readonly StepAnalysisContext context,
			ChainingRuleCollection supportedRules
		) => new(chain.Conclusions, ((IForcingChains)chain).GetViews(grid, chain.Conclusions, supportedRules), context.Options, chain);
	}

	/// <summary>
	/// Collect all multiple forcing chains on applying to a unique rectangle, appeared in a grid.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="onlyFindOne">Indicates whether the method only find one valid chain.</param>
	/// <returns>All possible multiple forcing chain instances.</returns>
	public static ReadOnlySpan<RectangleForcingChains> CollectRectangleMultipleForcingChains(in Grid grid, bool onlyFindOne)
	{
		var result = new SortedSet<RectangleForcingChains>(ChainingComparers.MultipleForcingChainsComparer);
		foreach (var urCells in UniqueRectanglePattern.AllPatterns)
		{
			var cells = urCells.AsCellMap();
			if ((EmptyCells & cells) != cells)
			{
				// Not all 4 cells are empty cells.
				continue;
			}

			// Collect all digits of the rectangle, to determine which digits can be used as a rectangle.
			var allDigitsMask = grid[cells].AllSets;
			if (allDigitsMask.Length is 0 or 1)
			{
				// No enough digits available.
				continue;
			}

			// Iterate on each combination of pair of digits, as ones used by rectangle.
			foreach (var digitPair in allDigitsMask & 2)
			{
				var (d1, d2) = (digitPair[0], digitPair[1]);

				// Determine whether such digits can be filled in diagonal cells.
				// If a rectangle is correct, we should guarantee both digits can be filled twice in rectangle pattern,
				// in order to make a valid deadly pattern (i.e. guarantee having at least one possible unavoidable set).
				if (!UniqueRectanglePattern.CanMakeDeadlyPattern(grid, d1, d2, urCells))
				{
					continue;
				}

				// If so, the UR pattern is valid. Now check for forcing chains.
				var branchStartCandidates = CandidateMap.Empty;
				foreach (var cell in urCells)
				{
					foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~(1 << d1 | 1 << d2)))
					{
						branchStartCandidates += cell * 9 + digit;
					}
				}

				// Collect branches for all possible candidates to be iterated,
				// and determine whether at least one candidate can be considered as a conclusion from all different start candidates.
				var nodesSupposedOnGrouped = new Dictionary<Candidate, HashSet<Node>>();
				var nodesSupposedOffGrouped = new Dictionary<Candidate, HashSet<Node>>();
				var resultNodesSupposedOn = default(HashSet<Node>);
				var resultNodesSupposedOff = default(HashSet<Node>);
				foreach (var candidate in branchStartCandidates)
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

				var step1 = rfcOn(urCells, grid, d1, d2, branchStartCandidates, nodesSupposedOnGrouped, resultNodesSupposedOn);
				if (!step1.IsEmpty)
				{
					return step1;
				}

				var step2 = rfcOff(urCells, grid, d1, d2, branchStartCandidates, nodesSupposedOffGrouped, resultNodesSupposedOff);
				if (!step2.IsEmpty)
				{
					return step2;
				}
			}
		}
		return result.ToArray();


		ReadOnlySpan<RectangleForcingChains> rfcOn(
			Cell[] urCells,
			in Grid grid,
			Digit d1,
			Digit d2,
			scoped in CandidateMap branchStartCandidates,
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

				var rfc = new RectangleForcingChains(urCells, (Mask)(1 << d1 | 1 << d2), conclusion);
				foreach (var candidate in branchStartCandidates)
				{
					var branchNode = onNodes[candidate].First(n => n.Equals(node, NodeComparison.IncludeIsOn));
					rfc.Add(candidate, node.IsOn ? new StrongForcingChain(branchNode) : new WeakForcingChain(branchNode));
				}
				if (onlyFindOne)
				{
					return (RectangleForcingChains[])[rfc];
				}
				result.Add(rfc);
			}
			return [];
		}

		ReadOnlySpan<RectangleForcingChains> rfcOff(
			Cell[] urCells,
			in Grid grid,
			Digit d1,
			Digit d2,
			scoped in CandidateMap branchStartCandidates,
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

				var rfc = new RectangleForcingChains(urCells, (Mask)(1 << d1 | 1 << d2));
				foreach (var candidate in branchStartCandidates)
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
					return (RectangleForcingChains[])[rfc];
				}
				result.Add(rfc);
			}
			return [];
		}
	}
}
