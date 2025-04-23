namespace Sudoku.Analytics.Drivers;

internal partial class ChainingDriver
{
	/// <summary>
	/// Collect all dynamic multiple forcing chains and dynamic binary forcing chains appeared in a grid.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="context">Indicates the context to be used.</param>
	/// <param name="chainingRules">Indicates the chaining rules used.</param>
	/// <returns>All possible dynamic forcing chains instances.</returns>
	public static ReadOnlySpan<IDynamicForcingChains> CollectDynamicForcingChains(
		in Grid grid,
		in StepAnalysisContext context,
		ChainingRuleCollection chainingRules
	)
	{
		var result = new List<IDynamicForcingChains>();
		foreach (var cell in EmptyCells)
		{
			var nodesSupposedOn_GroupedByDigit = new Dictionary<Candidate, HashSet<Node>>();
			var nodesSupposedOff_GroupedByDigit = new Dictionary<Candidate, HashSet<Node>>();
			var nodesSupposedOn_InCell = default(HashSet<Node>);
			var nodesSupposedOff_InCell = default(HashSet<Node>);
			var digitsMask = grid.GetCandidates(cell);
			foreach (var digit in digitsMask)
			{
				if (chainingOnBinary(cell, digit, grid, context, chainingRules, out var nodesSupposedOn, out var nodesSupposedOff)
					is var binaryForcingChainsFound and not [])
				{
					return binaryForcingChainsFound;
				}

				if (chainingOnRegion(cell, digit, grid, context, nodesSupposedOn, nodesSupposedOff)
					is var regionForcingChainsFound and not [])
				{
					return regionForcingChainsFound;
				}

				nodesSupposedOn_GroupedByDigit.Add(cell * 9 + digit, nodesSupposedOn);
				nodesSupposedOff_GroupedByDigit.Add(cell * 9 + digit, nodesSupposedOff);
				if (nodesSupposedOn_InCell is null)
				{
					nodesSupposedOn_InCell = new(ChainingComparers.NodeMapComparer);
					nodesSupposedOff_InCell = new(ChainingComparers.NodeMapComparer);
					nodesSupposedOn_InCell.UnionWith(nodesSupposedOn);
					nodesSupposedOff_InCell.UnionWith(nodesSupposedOff);
				}
				else
				{
					Debug.Assert(nodesSupposedOff_InCell is not null);
					nodesSupposedOn_InCell.IntersectWith(nodesSupposedOn);
					nodesSupposedOff_InCell.IntersectWith(nodesSupposedOff);
				}
			}

			if (chainingOnCell(
				cell, digitsMask, grid, context, nodesSupposedOn_GroupedByDigit, nodesSupposedOff_GroupedByDigit,
				nodesSupposedOn_InCell, nodesSupposedOff_InCell)
				is var cellForcingChainsFound and not [])
			{
				return cellForcingChainsFound;
			}
		}
		return result.ToArray();


		ReadOnlySpan<BinaryForcingChains> chainingOnBinary(
			Cell cell,
			Digit digit,
			in Grid grid,
			in StepAnalysisContext context,
			ChainingRuleCollection chainingRules,
			out HashSet<Node> nodesSupposedOn,
			out HashSet<Node> nodesSupposedOff
		)
		{
			///////////////////////////////////////////////
			// Collect with contradiction forcing chains //
			///////////////////////////////////////////////

			// Test for "on" state.
			var currentNodeOn = new Node((cell * 9 + digit).AsCandidateMap(), true);
			(nodesSupposedOn, nodesSupposedOff) = FindDynamicForcingChains(
				currentNodeOn,
				grid,
				chainingRules,
				context.Options,
				out var contradiction
			);
			if (contradiction is var (onNode_OnState, offNode_OnState))
			{
				if (bfcOn(grid, context, currentNodeOn, onNode_OnState, offNode_OnState, true)
					is var contradictionForcingChains and not [])
				{
					return contradictionForcingChains;
				}
			}

			// Test for "off" state.
			var currentNodeOff = ~currentNodeOn;
			var (nodesSupposedOn_OffCase, nodesSupposedOff_OffCase) = FindDynamicForcingChains(
				currentNodeOff,
				grid,
				chainingRules,
				context.Options,
				out contradiction
			);
			if (contradiction is var (onNode_OffState, offNode_OffState))
			{
				if (bfcOff(grid, context, currentNodeOff, onNode_OffState, offNode_OffState, true)
					is var contradictionForcingChains and not [])
				{
					return contradictionForcingChains;
				}
			}

			////////////////////////////////////////
			// Collect with double forcing chains //
			////////////////////////////////////////
			foreach (var node in nodesSupposedOn)
			{
				var conflictedNode = nodesSupposedOn_OffCase.FirstOrDefault(n => n == node);
				if (conflictedNode is not null)
				{
					if (bfcOff(grid, context, node, node, conflictedNode, false) is var doubleForcingChains and not [])
					{
						return doubleForcingChains;
					}
				}
			}
			foreach (var node in nodesSupposedOff)
			{
				var conflictedNode = nodesSupposedOff_OffCase.FirstOrDefault(n => n == node);
				if (conflictedNode is not null)
				{
					if (bfcOn(grid, context, node, node, conflictedNode, false) is var doubleForcingChains and not [])
					{
						return doubleForcingChains;
					}
				}
			}
			return [];
		}

		ReadOnlySpan<MultipleForcingChains> chainingOnCell(
			Cell cell,
			Mask digitsMask,
			in Grid grid,
			in StepAnalysisContext context,
			Dictionary<Cell, HashSet<Node>> nodesSupposedOn_GroupedByDigit,
			Dictionary<Cell, HashSet<Node>> nodesSupposedOff_GroupedByDigit,
			HashSet<Node>? nodesSupposedOn_InCell,
			HashSet<Node>? nodesSupposedOff_InCell
		)
		{
			//////////////////////////////////////
			// Collect with cell forcing chains //
			//////////////////////////////////////
			var cellOn = cfcOn(grid, cell, context, nodesSupposedOn_GroupedByDigit, nodesSupposedOn_InCell, digitsMask);
			if (!cellOn.IsEmpty)
			{
				return cellOn;
			}
			var cellOff = cfcOff(grid, cell, context, nodesSupposedOff_GroupedByDigit, nodesSupposedOff_InCell, digitsMask);
			if (!cellOff.IsEmpty)
			{
				return cellOff;
			}
			return [];
		}

		ReadOnlySpan<MultipleForcingChains> chainingOnRegion(
			Cell cell,
			Digit digit,
			in Grid grid,
			in StepAnalysisContext context,
			HashSet<Node> nodesSupposedOn,
			HashSet<Node> nodesSupposedOff
		)
		{
			foreach (var houseType in HouseTypes)
			{
				var house = cell.ToHouse(houseType);
				var cellsInHouse = HousesMap[house] & CandidatesMap[digit];
				if (cellsInHouse.Count <= 2)
				{
					// There's no need iterating on such house because the chain only contains 2 branches,
					// which means it can be combined into one normal chain.
					continue;
				}

				var firstCellInHouse = cellsInHouse[0];
				if (firstCellInHouse != cell)
				{
					// We should skip the other cells in the house, in order to avoid duplicate forcing chains.
					continue;
				}

				var nodesSupposedOn_GroupedByHouse = new Dictionary<Candidate, HashSet<Node>>();
				var nodesSupposedOff_GroupedByHouse = new Dictionary<Candidate, HashSet<Node>>();
				var nodesSupposedOn_InHouse = new HashSet<Node>(ChainingComparers.NodeMapComparer);
				var nodesSupposedOff_InHouse = new HashSet<Node>(ChainingComparers.NodeMapComparer);
				foreach (var otherCell in cellsInHouse)
				{
					var otherCandidate = otherCell * 9 + digit;
					if (otherCell == cell)
					{
						nodesSupposedOn_GroupedByHouse.Add(otherCandidate, nodesSupposedOn);
						nodesSupposedOff_GroupedByHouse.Add(otherCandidate, nodesSupposedOff);
						nodesSupposedOn_InHouse.UnionWith(nodesSupposedOn);
						nodesSupposedOff_InHouse.UnionWith(nodesSupposedOff);
					}
					else
					{
						var other = new Node(otherCandidate.AsCandidateMap(), true);
						var (otherNodesSupposedOn_InHouse, otherNodesSupposedOff_InHouse) = FindForcingChains(other);
						nodesSupposedOn_GroupedByHouse.Add(otherCandidate, otherNodesSupposedOn_InHouse);
						nodesSupposedOff_GroupedByHouse.Add(otherCandidate, otherNodesSupposedOff_InHouse);
						nodesSupposedOn_InHouse.IntersectWith(otherNodesSupposedOn_InHouse);
						nodesSupposedOff_InHouse.IntersectWith(otherNodesSupposedOff_InHouse);
					}
				}

				////////////////////////////////////////
				// Collect with region forcing chains //
				////////////////////////////////////////
				var regionOn = rfcOn(grid, digit, cellsInHouse, context, nodesSupposedOn_GroupedByHouse, nodesSupposedOn_InHouse);
				if (!regionOn.IsEmpty)
				{
					return regionOn;
				}
				var regionOff = rfcOff(grid, digit, cellsInHouse, context, nodesSupposedOff_GroupedByHouse, nodesSupposedOff_InHouse);
				if (!regionOff.IsEmpty)
				{
					return regionOff;
				}
			}
			return [];
		}

		ReadOnlySpan<MultipleForcingChains> cfcOn(
			in Grid grid,
			Cell cell,
			in StepAnalysisContext context,
			Dictionary<Candidate, HashSet<Node>> onNodes,
			HashSet<Node>? resultOnNodes,
			Mask digitsMask
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

				var cfc = new MultipleForcingChains(conclusion);
				foreach (var d in digitsMask)
				{
					var branchNode = onNodes[cell * 9 + d].First(n => n.Equals(node, NodeComparison.IncludeIsOn));
					cfc.Add(cell * 9 + d, node.IsOn ? new StrongForcingChain(branchNode, true) : new WeakForcingChain(branchNode, true));
				}
				if (context.OnlyFindOne)
				{
					return (MultipleForcingChains[])[cfc];
				}
				result.Add(cfc);
			}
			return [];
		}

		ReadOnlySpan<MultipleForcingChains> cfcOff(
			in Grid grid,
			Cell cell,
			in StepAnalysisContext context,
			Dictionary<Candidate, HashSet<Node>> offNodes,
			HashSet<Node>? resultOffNodes,
			Mask digitsMask
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

				var cfc = new MultipleForcingChains();
				foreach (var d in digitsMask)
				{
					var branchNode = offNodes[cell * 9 + d].First(n => n.Equals(node, NodeComparison.IncludeIsOn));
					cfc.Add(cell * 9 + d, node.IsOn ? new StrongForcingChain(branchNode, true) : new WeakForcingChain(branchNode, true));
				}
				if (cfc.GetThoroughConclusions(grid) is not { Length: not 0 } conclusions)
				{
					continue;
				}

				cfc.Conclusions = conclusions;
				if (context.OnlyFindOne)
				{
					return (MultipleForcingChains[])[cfc];
				}
				result.Add(cfc);
			}
			return [];
		}

		ReadOnlySpan<MultipleForcingChains> rfcOn(
			in Grid grid,
			Digit digit,
			scoped in CellMap cellsInHouse,
			in StepAnalysisContext context,
			Dictionary<Candidate, HashSet<Node>> onNodes,
			HashSet<Node> houseOnNodes
		)
		{
			foreach (var node in houseOnNodes)
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

				var rfc = new MultipleForcingChains(conclusion);
				foreach (var c in cellsInHouse)
				{
					var branchNode = onNodes[c * 9 + digit].First(n => n.Equals(node, NodeComparison.IncludeIsOn));
					rfc.Add(c * 9 + digit, node.IsOn ? new StrongForcingChain(branchNode, true) : new WeakForcingChain(branchNode, true));
				}
				if (context.OnlyFindOne)
				{
					return (MultipleForcingChains[])[rfc];
				}
				result.Add(rfc);
			}
			return [];
		}

		ReadOnlySpan<MultipleForcingChains> rfcOff(
			in Grid grid,
			Digit digit,
			scoped in CellMap cellsInHouse,
			in StepAnalysisContext context,
			Dictionary<Candidate, HashSet<Node>> offNodes,
			HashSet<Node> houseOffNodes
		)
		{
			foreach (var node in houseOffNodes)
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

				var rfc = new MultipleForcingChains();
				foreach (var c in cellsInHouse)
				{
					var branchNode = offNodes[c * 9 + digit].First(n => n.Equals(node, NodeComparison.IncludeIsOn));
					rfc.Add(c * 9 + digit, node.IsOn ? new StrongForcingChain(branchNode, true) : new WeakForcingChain(branchNode, true));
				}
				if (rfc.GetThoroughConclusions(grid) is not { Length: not 0 } conclusions)
				{
					continue;
				}

				rfc.Conclusions = conclusions;
				if (context.OnlyFindOne)
				{
					return (MultipleForcingChains[])[rfc];
				}
				result.Add(rfc);
			}
			return [];
		}

		ReadOnlySpan<BinaryForcingChains> bfcOn(
			in Grid grid,
			in StepAnalysisContext context,
			Node targetNode,
			Node onNode_OnState,
			Node offNode_OnState,
			bool isContradiction
		)
		{
			// Because an "on" node makes such contradiction, we can conclude that the node is false in logic.
			if (targetNode is not { Map: [var candidate] })
			{
				return [];
			}

			var conclusion = new Conclusion(Elimination, candidate);
			if (grid.Exists(conclusion.Candidate) is not true)
			{
				return [];
			}

			var dynamicChain = new BinaryForcingChains(
				onNode_OnState.IsOn
					? new StrongForcingChain(onNode_OnState, true)
					: new WeakForcingChain(onNode_OnState, true),
				offNode_OnState.IsOn
					? new StrongForcingChain(offNode_OnState, true)
					: new WeakForcingChain(offNode_OnState, true),
				conclusion,
				isContradiction
			);
			if (context.OnlyFindOne)
			{
				return (BinaryForcingChains[])[dynamicChain];
			}

			result.Add(dynamicChain);
			return [];
		}

		ReadOnlySpan<BinaryForcingChains> bfcOff(
			in Grid grid,
			in StepAnalysisContext context,
			Node targetNode,
			Node onNode_OffState,
			Node offNode_OffState,
			bool isContradiction
		)
		{
			// Because an "off" node makes such contradiction, we can conclude that the node is true in logic.
			if (targetNode is not { Map: [var candidate] })
			{
				return [];
			}

			var conclusion = new Conclusion(Assignment, candidate);
			if (grid.Exists(conclusion.Candidate) is not true)
			{
				return [];
			}

			var dynamicChain = new BinaryForcingChains(
				onNode_OffState.IsOn
					? new StrongForcingChain(onNode_OffState, true)
					: new WeakForcingChain(onNode_OffState, true),
				offNode_OffState.IsOn
					? new StrongForcingChain(offNode_OffState, true)
					: new WeakForcingChain(offNode_OffState, true),
				conclusion,
				isContradiction
			);
			if (context.OnlyFindOne)
			{
				return (BinaryForcingChains[])[dynamicChain];
			}

			result.Add(dynamicChain);
			return [];
		}
	}

	/// <summary>
	/// Finds a list of nodes that can implicitly connects to current node via a dynamic forcing chain.
	/// This method won't use cached dictionary.
	/// </summary>
	/// <param name="startNode">The current instance.</param>
	/// <param name="grid">Indicates the interim cached grid.</param>
	/// <param name="chainingRules">Indicates the chaining rules to be used.</param>
	/// <param name="options">Indicates the options used.</param>
	/// <param name="contradiction">
	/// <para>
	/// The found contradiction node pairs.
	/// If there's no contradiction here, this argument will keep the value <see langword="null"/>.
	/// </para>
	/// <para>
	/// In general, non-dynamic cases will always make this argument <see langword="null"/>,
	/// because non-dynamic contradiction can be integrated into one normal alternating inference chain,
	/// by combining two different branches.
	/// </para>
	/// </param>
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
	private static ForcingChainsInfo FindDynamicForcingChains(
		Node startNode,
		in Grid grid,
		ChainingRuleCollection chainingRules,
		StepGathererOptions options,
		out (Node On, Node Off)? contradiction
	)
	{
		(contradiction, var (pendingNodesSupposedOn, pendingNodesSupposedOff)) = (null, (new LinkedList<Node>(), new LinkedList<Node>()));
		(startNode.IsOn ? pendingNodesSupposedOn : pendingNodesSupposedOff).AddLast(startNode);

		var tempGrid = grid;
		var nodesSupposedOn = new HashSet<Node>(ChainingComparers.NodeMapComparer);
		var nodesSupposedOff = new HashSet<Node>(ChainingComparers.NodeMapComparer);
		(startNode.IsOn ? nodesSupposedOn : nodesSupposedOff).Add(startNode);

		while (pendingNodesSupposedOn.Count != 0 || pendingNodesSupposedOff.Count != 0)
		{
			if (pendingNodesSupposedOn.Count != 0)
			{
				var currentNode = pendingNodesSupposedOn.RemoveFirstNode();
				if (GetNodesFromOnToOff(currentNode, chainingRules, options, tempGrid) is var supposedOff and not [])
				{
					foreach (var node in supposedOff)
					{
						if (nodesSupposedOn.FirstOrDefault(n => n.Equals(~node, NodeComparison.IncludeIsOn)) is { } nextNodeNegated)
						{
							// Contradiction is found.
							contradiction = (nextNodeNegated, node);
							goto ReturnResult;
						}

						if (nodesSupposedOff.Add(node))
						{
							pendingNodesSupposedOff.AddLast(node);
						}
					}
				}
			}
			else
			{
				var currentNode = pendingNodesSupposedOff.RemoveFirstNode();
				var supposedOn = GetNodesFromOffToOn(currentNode, chainingRules, nodesSupposedOff, options, tempGrid, grid);

				tempGrid.Apply(currentNode);

				if (!supposedOn.IsEmpty)
				{
					foreach (var node in supposedOn)
					{
						if (nodesSupposedOff.FirstOrDefault(n => n.Equals(~node, NodeComparison.IncludeIsOn)) is { } nextNodeNegated)
						{
							// Contradiction is found.
							contradiction = (node, nextNodeNegated);
							goto ReturnResult;
						}

						if (nodesSupposedOn.Add(node))
						{
							pendingNodesSupposedOn.AddLast(node);
						}
					}
				}
			}
		}

	ReturnResult:
		// Returns the found result.
		return new(nodesSupposedOn, nodesSupposedOff);
	}

	/// <summary>
	/// Try to find nodes that will be considered as "off" state if the specified node is "on" state.
	/// </summary>
	/// <param name="node">The previous node to be checked.</param>
	/// <param name="chainingRules">The chaining rule collection to be used.</param>
	/// <param name="options">The options.</param>
	/// <param name="grid">The current grid.</param>
	/// <returns>A list of nodes found.</returns>
	/// <remarks><i>
	/// This method doesn't use cache on purpose, therefore this method won't be marked type
	/// <see cref="InterceptorMethodCallerAttribute"/>.
	/// </i></remarks>
	/// <seealso cref="InterceptorMethodCallerAttribute"/>
	private static ReadOnlySpan<Node> GetNodesFromOnToOff(Node node, ChainingRuleCollection chainingRules, StepGathererOptions options, in Grid grid)
	{
		var nodes = new HashSet<Node>();
		foreach (var chainingRule in chainingRules)
		{
			chainingRule.CollectOffNodes(node, grid, options, ref nodes);
		}
		return nodes.ToArray();
	}

	/// <summary>
	/// Try to find nodes that will be considered as "on" state if the specified node is "off" state.
	/// </summary>
	/// <param name="node">The previous node to be checked.</param>
	/// <param name="chainingRules">The chaining rule collection to be used.</param>
	/// <param name="nodesSupposedOff">Indicates the nodes supposed to be "off".</param>
	/// <param name="options">The options.</param>
	/// <param name="grid">The current grid.</param>
	/// <param name="originalGrid">Indicates the original grid.</param>
	/// <returns>A list of nodes found.</returns>
	/// <remarks><i>
	/// This method doesn't use cache on purpose, therefore this method won't be marked type
	/// <see cref="InterceptorMethodCallerAttribute"/>.
	/// </i></remarks>
	/// <seealso cref="InterceptorMethodCallerAttribute"/>
	private static ReadOnlySpan<Node> GetNodesFromOffToOn(
		Node node,
		ChainingRuleCollection chainingRules,
		HashSet<Node> nodesSupposedOff,
		StepGathererOptions options,
		in Grid grid,
		in Grid originalGrid
	)
	{
		var nodes = new HashSet<Node>();
		foreach (var chainingRule in chainingRules)
		{
			chainingRule.CollectOnNodes(node, grid, originalGrid, nodesSupposedOff, options, ref nodes);
		}
		return nodes.ToArray();
	}
}

/// <summary>
/// Provides with extension methods on <see cref="Grid"/>.
/// </summary>
/// <seealso cref="Grid"/>
file static class GridNodeExtensions
{
	/// <summary>
	/// Provides extension members on <see langword="ref"/> <see cref="Grid"/>.
	/// </summary>
	extension(ref Grid grid)
	{
		/// <summary>
		/// Treat the specified node as a real conclusion, and apply to a grid.
		/// </summary>
		/// <param name="node">The node.</param>
		public void Apply(Node node)
		{
			ref readonly var map = ref node.Map;
			if (node.IsOn)
			{
				// Find intersections to be removed; or assign it directly if the node only uses 1 candidate.
				if (map is [var onlyCandidate])
				{
					grid.SetDigit(onlyCandidate / 9, onlyCandidate % 9);
				}
				else
				{
					foreach (var candidate in map.PeerIntersection)
					{
						if (grid.GetState(candidate / 9) == CellState.Empty)
						{
							grid[candidate / 9] &= (Mask)~(1 << candidate % 9);
						}
					}
				}
			}
			else
			{
				// If a node is considered as false, we should remove all possible candidates of the node.
				foreach (var candidate in map)
				{
					grid[candidate / 9] &= (Mask)~(1 << candidate % 9);
				}
			}
		}
	}
}
