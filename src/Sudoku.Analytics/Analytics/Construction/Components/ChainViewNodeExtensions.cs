namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Provides with extension methods on <see cref="Chain"/>.
/// </summary>
/// <seealso cref="Chain"/>
internal static class ChainViewNodeExtensions
{
	/// <summary>
	/// Collect views for the current chain, without nested paths checking.
	/// </summary>
	/// <param name="this">The chain to be checked.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="supportedRules">The supported rules.</param>
	/// <returns>The views.</returns>
	public static View[] GetViews_Monoparental(this Chain @this, in Grid grid, ChainingRuleCollection supportedRules)
	{
		var result = (View[])[
			[
				.. v(),
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


		ReadOnlySpan<CandidateViewNode> v()
		{
			var result = new List<CandidateViewNode>();
			for (var i = 0; i < @this.Length; i++)
			{
				var id = (i & 1) == 0 ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal;
				foreach (var candidate in @this[i].Map)
				{
					result.Add(new(id, candidate));
				}
			}
			return result.AsSpan();
		}
	}
}
