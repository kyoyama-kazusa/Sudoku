namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Provides with extension methods on <see cref="Chain"/>.
/// </summary>
/// <seealso cref="Chain"/>
internal static class ChainExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="Chain"/>.
	/// </summary>
	extension(Chain @this)
	{
		/// <summary>
		/// Indicates whether the pattern only uses same digits.
		/// </summary>
		public bool IsX
		{
			get
			{
				var digitsMask = (Mask)0;
				foreach (var node in @this.ValidNodes)
				{
					digitsMask |= node.Map.Digits;
				}
				return BitOperations.IsPow2(digitsMask);
			}
		}

		/// <summary>
		/// Indicates whether the pattern only uses cell strong links.
		/// </summary>
		public bool IsY
		{
			get
			{
				foreach (var link in @this.StrongLinks)
				{
					if (link is ({ Map.Digits: var digits1 }, { Map.Digits: var digits2 }) && digits1 == digits2)
					{
						return false;
					}
				}
				return @this.First.Map.Digits == @this.Last.Map.Digits;
			}
		}

		/// <summary>
		/// Indicates whether at least one node in the whole pattern overlaps with a node.
		/// </summary>
		public bool IsOverlapped
		{
			get
			{
				foreach (var nodePair in (from node in @this.ValidNodes select node.Map).GetSubsets(2))
				{
					ref readonly var map1 = ref nodePair[0];
					ref readonly var map2 = ref nodePair[1];
					if (map1 & map2)
					{
						return true;
					}
				}
				return false;
			}
		}

		/// <summary>
		/// Indicates whether at least one link in strong links contains grouped nodes,
		/// and is not advanced node (i.e. contains grouped pattern).
		/// </summary>
		internal bool IsStrongLinksStrictlyGrouped
		{
			get
			{
				foreach (var link in @this.StrongLinks)
				{
					if (link is ({ Map.Count: var d1 }, { Map.Count: var d2 }, _, var groupedPattern)
						&& (d1 != 1 || d2 != 1 || groupedPattern is not null))
					{
						return true;
					}
				}
				return false;
			}
		}

		/// <summary>
		/// Indicates whether at least one link in weak links contains grouped nodes,
		/// and is not advanced node (i.e. contains grouped pattern).
		/// </summary>
		internal bool IsWeakLinksStrictlyGrouped
		{
			get
			{
				foreach (var link in @this.WeakLinks)
				{
					if (link is ({ Map.Count: var d1 }, { Map.Count: var d2 }, _, var groupedPattern)
						&& (d1 != 1 || d2 != 1 || groupedPattern is not null))
					{
						return true;
					}
				}
				return false;
			}
		}


		/// <summary>
		/// Collect views for the current chain, without nested paths checking.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="supportedRules">The supported rules.</param>
		/// <returns>The views.</returns>
		public View[] GetViews_Monoparental(in Grid grid, ChainingRuleCollection supportedRules)
		{
			var result = (View[])[
				[
					.. GetGlobalViewNodes(@this),
					..
					from link in @this.Links
					let node1 = link.FirstNode
					let node2 = link.SecondNode
					select new ChainLinkViewNode(ColorIdentifier.Normal, node1.Map, node2.Map, link.IsStrong)
				]
			];

			var (alsIndex, urIndex) = (0, 0);
			foreach (var supportedRule in supportedRules)
			{
				supportedRule.GetViewNodes(grid, @this, result[0], ref alsIndex, ref urIndex, out var producedViewNodes);
				result[0].AddRange(producedViewNodes);
			}
			return result;
		}


		private static ReadOnlySpan<CandidateViewNode> GetGlobalViewNodes(Chain chain)
		{
			var result = new List<CandidateViewNode>();
			for (var i = 0; i < chain.Length; i++)
			{
				var id = (i & 1) == 0 ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal;
				foreach (var candidate in chain[i].Map)
				{
					result.Add(new(id, candidate));
				}
			}
			return result.AsSpan();
		}
	}
}
