namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Represents a multiple forcing chains that is applied to a unique rectangle.
/// </summary>
/// <param name="cells"><inheritdoc cref="Cells" path="/summary"/></param>
/// <param name="urDigitsMask"><inheritdoc cref="UrDigitsMask" path="/summary"/></param>
/// <param name="conclusions"><inheritdoc cref="MultipleForcingChains(Conclusion[])" path="/param[@name='conclusions']"/></param>
public sealed partial class RectangleForcingChains(Cell[] cells, Mask urDigitsMask, params Conclusion[] conclusions) :
	MultipleForcingChains(conclusions)
{
	/// <inheritdoc/>
	public override bool IsCellMultiple => false;

	/// <inheritdoc/>
	public override bool IsHouseMultiple => false;

	/// <inheritdoc/>
	public override bool IsAdvancedMultiple => true;

	/// <summary>
	/// Indicates the digits used in pattern.
	/// </summary>
	public Mask UrDigitsMask { get; } = urDigitsMask;

	/// <summary>
	/// Indicates the pattern cells.
	/// </summary>
	public Cell[] Cells { get; } = cells;


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
		foreach (var cell in Cells)
		{
			var node = new CellViewNode(ColorIdentifier.Rectangle1, cell);
			foreach (var view in views)
			{
				view.Add(node);
			}
			foreach (var digit in UrDigitsMask & grid.GetCandidates(cell))
			{
				var candidateNode = new CandidateViewNode(ColorIdentifier.Rectangle1, cell * 9 + digit);
				foreach (var view in views)
				{
					view.Add(candidateNode);
				}
			}
		}
	}

	/// <inheritdoc/>
	protected override ReadOnlySpan<ViewNode> GetInitialViewNodes(in Grid grid)
	{
		var result = new List<ViewNode>();
		foreach (var cell in Cells)
		{
			result.Add(new CellViewNode(ColorIdentifier.Rectangle1, cell));
			foreach (var digit in UrDigitsMask & grid.GetCandidates(cell))
			{
				result.Add(new CandidateViewNode(ColorIdentifier.Rectangle1, cell * 9 + digit));
			}
		}
		return result.AsSpan();
	}
}
