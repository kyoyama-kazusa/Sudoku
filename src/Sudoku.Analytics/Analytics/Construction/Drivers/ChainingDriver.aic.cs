#define ONLY_FIND_ONE_CHAIN_FOR_ONE_START
#define STRICT_LENGTH_CHECKING
#define STRICT_LENGTH_CHECKING_OPTIMIZATION
#if !STRICT_LENGTH_CHECKING && STRICT_LENGTH_CHECKING_OPTIMIZATION
#undef STRICT_LENGTH_CHECKING_OPTIMIZATION
#warning 'STRICT_LENGTH_CHECKING_OPTIMIZATION' won't work if 'STRICT_LENGTH_CHECKING' is not configured.
#endif

namespace Sudoku.Analytics.Construction.Drivers;

internal partial class ChainingDriver
{
	/// <summary>
	/// Collect all chains and loops appeared in a grid.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="allowsAdvancedLinks">Indicates whether this method allows for advanced links.</param>
	/// <param name="onlyFindOne">Indicates whether the method only find one valid step.</param>
	/// <param name="makeConclusionAroundBackdoors">
	/// <inheritdoc cref="ChainStepSearcher.MakeConclusionAroundBackdoors" path="/summary"/>
	/// </param>
	/// <returns>All possible <see cref="Chain"/> instances.</returns>
	/// <remarks>
	/// <include file="../../global-doc-comments.xml" path="/g/developer-notes" />
	/// A valid chain can only be categorized by the following three cases:
	/// <list type="number">
	/// <item>
	/// <para><b>Discontinuous Nice Loop</b></para>
	/// <para>
	/// Start with weak link, alternating strong and weak links and return to itself of a weak link
	/// (with an even number of nodes, odd number of links).
	/// </para>
	/// </item>
	/// <item>
	/// <para><b>Discontinuous Nice Loop</b></para>
	/// <para>
	/// Start with strong link, alternating strong and weak links and return to itself of a strong link
	/// (with an even number of nodes, even number of links).
	/// </para>
	/// </item>
	/// <item>
	/// <para><b>Continuous Nice Loop</b></para>
	/// <para>
	/// Start with strong link, alternating strong and weak links and return to itself of a weak link
	/// (with an even number of nodes, even number of links).
	/// </para>
	/// </item>
	/// </list>
	/// </remarks>
	public static ReadOnlySpan<NamedChain> CollectChains(
		in Grid grid,
		bool allowsAdvancedLinks,
		bool onlyFindOne,
		bool makeConclusionAroundBackdoors
	)
	{
		var result = new SortedSet<NamedChain>(ChainingComparers.ChainComparer);
		var traversedCandidates = CandidateMap.Empty;
		if (makeConclusionAroundBackdoors && grid.IsValid)
		{
			// Find backdoors of the puzzle.
			// If the puzzle is invalid (multiple solutions found), we won't call this inferring method to find backdoors,
			// in order to prevent potential exceptions.
			if (!BackdoorInferrer.TryInfer(grid, out var backdoorResult))
			{
				throw new PuzzleInvalidException(
					grid,
					allowsAdvancedLinks ? typeof(ChainStepSearcher) : typeof(GroupedChainStepSearcher)
				);
			}

			foreach (var (type, cell, digit) in backdoorResult.Candidates)
			{
				if (type == Elimination)
				{
					var node = new Node((cell * 9 + digit).AsCandidateMap(), true);
					if (FindChains(node, grid, onlyFindOne, result) is { } chain1)
					{
						return new SingletonArray<NamedChain>(chain1);
					}
				}
				else
				{
					var node = new Node((cell * 9 + digit).AsCandidateMap(), false);
					if (FindChains(node, grid, onlyFindOne, result) is { } chain1)
					{
						return new SingletonArray<NamedChain>(chain1);
					}
				}
				traversedCandidates.Add(cell * 9 + digit);
			}
		}

		foreach (var cell in EmptyCells)
		{
			var trueDigit = Solution.IsUndefined ? -1 : Solution.GetDigit(cell);
			foreach (var digit in grid.GetCandidates(cell))
			{
				if (traversedCandidates.Contains(cell * 9 + digit))
				{
					// Skip for travsered candidates.
					continue;
				}

				var node = new Node((cell * 9 + digit).AsCandidateMap(), true);

				// Suppose the digit as "off" (false) to make a contradiction.
				// Obviously, only incorrect digits can be formed a contradiction.
				// Therefore, we only need to check such incorrect digits.
				if ((trueDigit != -1 && digit != trueDigit || trueDigit == -1)
					&& FindChains(node, grid, onlyFindOne, result) is { } chain1)
				{
					return new SingletonArray<NamedChain>(chain1);
				}

#if STRICT_LENGTH_CHECKING && STRICT_LENGTH_CHECKING_OPTIMIZATION
				if (onlyFindOne && result.Min is { Length: var possibleMinimalLength } lengthOptimizedChain1
					&& possibleMinimalLength <= (allowsAdvancedLinks ? 4 : 6))
				{
					// Optimization: Directly returns the searching if we can assert the chain is already shortest,
					// This binds with issue #730: https://github.com/KyouyamaKazusa0805/Sudoku/issues/730
					return new SingletonArray<NamedChain>(lengthOptimizedChain1);
				}
#endif
				// Same reason as above - only correct digits can be formed a chain that makes an assignment.
				if ((digit == trueDigit || trueDigit == -1) && FindChains(~node, grid, onlyFindOne, result) is { } chain2)
				{
					return new SingletonArray<NamedChain>(chain2);
				}
			}
		}
		return result.ToArray();
	}

	/// <summary>
	/// <para>
	/// Find all possible <see cref="NamedChain"/> patterns starting with the current node,
	/// and to make a conflict with itself.
	/// </para>
	/// <para>
	/// This method will return <see langword="null"/> if <paramref name="onlyFindOne"/> is <see langword="false"/>,
	/// or the method cannot find a valid chain that forms a conflict with the current node.
	/// </para>
	/// </summary>
	/// <param name="startNode">The current instance.</param>
	/// <param name="grid">The grid to be checked.</param>
	/// <param name="onlyFindOne">Indicates whether the method only find one valid <see cref="NamedChain"/> and return.</param>
	/// <param name="result">
	/// A collection that stores all possible found <see cref="NamedChain"/> patterns
	/// if <paramref name="onlyFindOne"/> is <see langword="false"/>.
	/// </param>
	/// <returns>The first found <see cref="NamedChain"/> pattern.</returns>
	/// <seealso cref="NamedChain"/>
#if STRICT_LENGTH_CHECKING
#pragma warning disable IDE0079
#pragma warning disable CA1859
#endif
	private static NamedChain? FindChains(Node startNode, in Grid grid, bool onlyFindOne, SortedSet<NamedChain> result)
	{
		var pendingNodesSupposedOn = new LinkedList<Node>();
		var pendingNodesSupposedOff = new LinkedList<Node>();
		(startNode.IsOn ? pendingNodesSupposedOn : pendingNodesSupposedOff).AddLast(startNode);

		var visitedNodesSupposedOn = new HashSet<Node>(ChainingComparers.NodeMapComparer);
		var visitedNodesSupposedOff = new HashSet<Node>(ChainingComparers.NodeMapComparer);
		_ = (visitedNodesSupposedOn.Add(startNode), visitedNodesSupposedOff.Add(startNode));

		while (pendingNodesSupposedOn.Count != 0 || pendingNodesSupposedOff.Count != 0)
		{
			while (pendingNodesSupposedOn.Count != 0)
			{
				var currentNode = pendingNodesSupposedOn.RemoveFirstNode();
				if (WeakLinkDictionary.TryGetValue(currentNode, out var nodesSupposedOff))
				{
					foreach (var nodeSupposedOff in nodesSupposedOff)
					{
						var nextNode = nodeSupposedOff >> currentNode;

						////////////////////////////////////////////
						// Continuous Nice Loop 3) Weak -> Strong //
						////////////////////////////////////////////
						if (nodeSupposedOff == startNode && ((IParentLinkedNode<Node>)nextNode).AncestorsLength >= 4)
						{
							var loop = new ContinuousNiceLoop(nextNode);
							if (!loop.GetConclusions(grid).IsWorthFor(grid))
							{
								goto Next;
							}

#if !STRICT_LENGTH_CHECKING
							if (onlyFindOne)
							{
								return loop;
							}
#endif

							result.Add(loop);
#if ONLY_FIND_ONE_CHAIN_FOR_ONE_START
							return null;
#endif
						Next:;
						}

						/////////////////////////////////////////////
						// Discontinuous Nice Loop 1) Weak -> Weak //
						/////////////////////////////////////////////
						if (nodeSupposedOff == ~startNode)
						{
							var chain = new AlternatingInferenceChain(nextNode);
							if (chain.IsImplicitLoop || !chain.GetConclusions(grid).IsWorthFor(grid))
							{
								goto Next;
							}

#if !STRICT_LENGTH_CHECKING
							if (onlyFindOne)
							{
								return chain;
							}
#endif

							result.Add(chain);
#if ONLY_FIND_ONE_CHAIN_FOR_ONE_START
							return null;
#endif
						Next:;
						}

						// This step will filter duplicate nodes in order not to make an internal loop on chains.
						// The second argument must be 'NodeComparison.IgnoreIsOn' because we should explicitly ignore them
						// no matter what state the node is.
						// This will fix issue #673: https://github.com/KyouyamaKazusa0805/Sudoku/issues/673
						// Counter-example:
						//   4.+3.6+85...+57.....8+89.5...3..7..+8+6.2.23..94.+8..+84.....15..6..8+7+3+3..+871.5.+7+68.....2:114 124 324 425 427 627 943 366 667 967 272 273 495 497
						if (!nodeSupposedOff.IsAncestorOf(currentNode, NodeComparison.IgnoreIsOn)
							&& visitedNodesSupposedOff.Add(nodeSupposedOff))
						{
							pendingNodesSupposedOff.AddLast(nextNode);
						}
					}
				}
			}
			while (pendingNodesSupposedOff.Count != 0)
			{
				var currentNode = pendingNodesSupposedOff.RemoveFirstNode();
				if (StrongLinkDictionary.TryGetValue(currentNode, out var nodesSupposedOn))
				{
					foreach (var nodeSupposedOn in nodesSupposedOn)
					{
						var nextNode = nodeSupposedOn >> currentNode;

						/////////////////////////////////////////////////
						// Discontinuous Nice Loop 2) Strong -> Strong //
						/////////////////////////////////////////////////
						if (nodeSupposedOn == ~startNode)
						{
							var chain = new AlternatingInferenceChain(nextNode);
							if (chain.IsImplicitLoop || !chain.GetConclusions(grid).IsWorthFor(grid))
							{
								goto Next;
							}

							if (onlyFindOne)
							{
								return chain;
							}

							result.Add(chain);
#if ONLY_FIND_ONE_CHAIN_FOR_ONE_START
							return null;
#endif
						Next:;
						}

						if (!nodeSupposedOn.IsAncestorOf(currentNode, NodeComparison.IgnoreIsOn)
							&& visitedNodesSupposedOn.Add(nodeSupposedOn))
						{
							pendingNodesSupposedOn.AddLast(nextNode);
						}
					}
				}
			}
		}
		return null;
	}
#if STRICT_LENGTH_CHECKING
#pragma warning restore CA1859
#pragma warning restore IDE0079
#endif
}
