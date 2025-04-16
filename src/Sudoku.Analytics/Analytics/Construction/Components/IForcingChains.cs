namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Represents a forcing chains component.
/// </summary>
internal interface IForcingChains : IChainOrForcingChains, IFormattable
{
	/// <summary>
	/// Indicates the complexity of the whole pattern.
	/// </summary>
	public abstract int Complexity { get; }

	/// <summary>
	/// Indicates the digits used in this pattern.
	/// </summary>
	public abstract Mask DigitsMask { get; }

	/// <summary>
	/// Indicates the conclusions of the pattern.
	/// </summary>
	public abstract StepConclusions Conclusions { get; }

	/// <summary>
	/// Indicates the complexity of each branch.
	/// </summary>
	public abstract ReadOnlySpan<int> BranchedComplexity { get; }

	/// <summary>
	/// Indicates all possible branches.
	/// </summary>
	protected abstract ReadOnlySpan<UnnamedChain> Branches { get; }


	/// <summary>
	/// Collect views for the current chain.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="newConclusions">The conclusions.</param>
	/// <param name="supportedRules">The supported rules.</param>
	/// <returns>The views.</returns>
	public sealed View[] GetViews(in Grid grid, Conclusion[] newConclusions, ChainingRuleCollection supportedRules)
	{
		var viewNodes = GetViewsCore(grid, supportedRules, newConclusions);
		var result = new View[viewNodes.Length];
		for (var i = 0; i < viewNodes.Length; i++)
		{
			var elimMap = (from conclusion in newConclusions select conclusion.Candidate).AsCandidateMap();
			result[i] = [
				..
				from node in viewNodes[i]
				where node is not CandidateViewNode { Candidate: var c } || !elimMap.Contains(c)
				select node
			];
		}

		var (viewIndex, cachedAlsIndex, cachedUrIndex) = (1, 0, 0);
		foreach (var branch in Branches)
		{
			foreach (var supportedRule in supportedRules)
			{
				supportedRule.GetViewNodes(grid, branch, result[viewIndex], ref cachedAlsIndex, ref cachedUrIndex, out var producedViewNodes);
				result[0].AddRange(producedViewNodes);
			}
			viewIndex++;
		}
		return result;
	}

	/// <summary>
	/// Represents a method that creates a list of views.
	/// </summary>
	/// <param name="grid">The target grid.</param>
	/// <param name="rules">The rules used.</param>
	/// <param name="newConclusions">The conclusions used.</param>
	/// <returns>A list of nodes.</returns>
	protected abstract ReadOnlySpan<ViewNode[]> GetViewsCore(in Grid grid, ChainingRuleCollection rules, Conclusion[] newConclusions);

	/// <inheritdoc/>
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(formatProvider);
}
