namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Represents a binary forcing chains (contradiction forcing chains or double forcing chains).
/// </summary>
/// <param name="branch1"><inheritdoc cref="Branch1" path="/summary"/></param>
/// <param name="branch2"><inheritdoc cref="Branch2" path="/summary"/></param>
/// <param name="conclusion"><inheritdoc cref="Conclusion" path="/summary"/></param>
/// <param name="isContradiction"><inheritdoc cref="IsContradiction" path="/summary"/></param>
public sealed partial class BinaryForcingChains(
	UnnamedChain branch1,
	UnnamedChain branch2,
	Conclusion conclusion,
	bool isContradiction
) :
	IBinaryForcingChains<BinaryForcingChains, UnnamedChain>,
	IDynamicForcingChains
{
	/// <inheritdoc/>
	public bool IsGrouped => false;

	/// <inheritdoc/>
	public bool IsStrictlyGrouped => false;

	/// <inheritdoc/>
	public bool IsDynamic => true;

	/// <summary>
	/// Indicates whether the pattern is a contradiction forcing chains.
	/// </summary>
	public bool IsContradiction { get; } = isContradiction;

	/// <inheritdoc/>
	public int Complexity => BranchedComplexity.Sum();

	/// <summary>
	/// Indicates the conclusion.
	/// </summary>
	public Conclusion Conclusion { get; } = conclusion;

	/// <inheritdoc/>
	public Mask DigitsMask
	{
		get
		{
			var result = (Mask)0;
			foreach (var value in Branch1)
			{
				result |= value.Map.Digits;
			}
			foreach (var value in Branch2)
			{
				result |= value.Map.Digits;
			}
			return result;
		}
	}

	/// <inheritdoc/>
	public ReadOnlySpan<int> BranchedComplexity => from v in Branches select v.Length;

	/// <summary>
	/// Indicates the first branch.
	/// </summary>
	public UnnamedChain Branch1 { get; } = branch1;

	/// <summary>
	/// Indicates the second branch.
	/// </summary>
	public UnnamedChain Branch2 { get; } = branch2;

	/// <inheritdoc/>
	ComponentType IComponent.Type => ComponentType.BinaryForcingChains;

	/// <inheritdoc/>
	StepConclusions IForcingChains.Conclusions => new SingletonArray<Conclusion>(Conclusion);

	/// <inheritdoc/>
	ReadOnlySpan<UnnamedChain> IForcingChains.Branches => Branches;

	/// <summary>
	/// Indicates the backing branches.
	/// </summary>
	private ReadOnlySpan<UnnamedChain> Branches => (UnnamedChain[])[Branch1, Branch2];


	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToString(IFormatProvider? formatProvider)
	{
		var converter = CoordinateConverter.GetInstance(formatProvider);
		return string.Join(", ", from branch in Branches select $"{branch.ToString(converter)}");
	}

	/// <inheritdoc/>
	ReadOnlySpan<ViewNode[]> IForcingChains.GetViewsCore(in Grid grid, ChainingRuleCollection rules, Conclusion[] newConclusions)
	{
		var result = new ViewNode[3][];
		var i = 0;
		var globalView = new List<ViewNode>();
		foreach (var chain in Branches)
		{
			var subview = View.Empty;
			foreach (var node in chain)
			{
				var id = node.IsOn ? ColorIdentifier.Normal : ColorIdentifier.Auxiliary1;
				foreach (var candidate in node.Map)
				{
					var currentViewNode = new CandidateViewNode(id, candidate);
					globalView.Add(currentViewNode);
					subview.Add(currentViewNode);
				}
			}

			var j = 0;
			foreach (var (firstNode, secondNode, isStrong) in chain.Links)
			{
				// Skip the link if there are >= 2 conclusions.
				if (newConclusions.Length >= 2 && j++ == 0)
				{
					continue;
				}

				var currentViewNode = new ChainLinkViewNode(ColorIdentifier.Normal, firstNode.Map, secondNode.Map, isStrong);
				globalView.Add(currentViewNode);
				subview.Add(currentViewNode);
			}
			result[++i] = [.. subview];
		}
		result[0] = [.. globalView];
		return result;
	}
}
