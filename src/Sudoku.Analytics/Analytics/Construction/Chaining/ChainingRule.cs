namespace Sudoku.Analytics.Construction.Chaining;

/// <summary>
/// Represents a rule that make inferences (strong or weak) between two <see cref="Node"/> instances.
/// </summary>
/// <seealso cref="Node"/>
[TypeImpl(
	TypeImplFlags.AllObjectMethods,
	EqualsBehavior = EqualsBehavior.ThrowNotSupportedException,
	OtherModifiersOnEquals = "sealed",
	GetHashCodeBehavior = GetHashCodeBehavior.ThrowNotSupportedException,
	OtherModifiersOnGetHashCode = "sealed",
	ToStringBehavior = ToStringBehavior.ThrowNotSupportedException,
	OtherModifiersOnToString = "sealed")]
public abstract partial class ChainingRule
{
	/// <summary>
	/// Indicates the elementary link types.
	/// </summary>
	public static readonly LinkType[] ElementaryLinkTypes = [LinkType.SingleDigit, LinkType.SingleCell];

	/// <summary>
	/// Indicates the advanced link types.
	/// </summary>
	public static readonly LinkType[] AdvancedLinkTypes = [
		LinkType.LockedCandidates,
		LinkType.AlmostLockedSets,
		//LinkType.KrakenNormalFish,
		LinkType.UniqueRectangle_SameDigit,
		LinkType.UniqueRectangle_DifferentDigit,
		LinkType.UniqueRectangle_SingleSideExternal,
		LinkType.UniqueRectangle_DoubleSideExternal,
		LinkType.UniqueRectangle_DifferentDigitWeak,
		LinkType.AvoidableRectangle,
		//LinkType.XyzWing,
	];


	/// <summary>
	/// Collects for both strong links and weak links appeared in <paramref name="grid"/>,
	/// and insert all found links into <paramref name="strongLinks"/> and <paramref name="weakLinks"/> respectively.
	/// </summary>
	/// <param name="grid">Indicates the grid used.</param>
	/// <param name="strongLinks">Indicates the strong links.</param>
	/// <param name="weakLinks">Indicates the weak links.</param>
	/// <param name="options">Indicates the options.</param>
	[Cached]
	public abstract void GetLinks(in Grid grid, LinkDictionary strongLinks, LinkDictionary weakLinks, StepGathererOptions options);

	/// <summary>
	/// Collects nodes that is supposed to "on" from the current node supposed to "off", storing them into <paramref name="nodes"/>.
	/// </summary>
	/// <param name="currentNode">Indicates the current node.</param>
	/// <param name="grid">Indicates the grid.</param>
	/// <param name="originalGrid">Indicates the original grid.</param>
	/// <param name="nodesSupposedOff">Indicates the nodes that are supposed to "off".</param>
	/// <param name="options">Indicates the options.</param>
	/// <param name="nodes">The nodes collected.</param>
	public virtual void CollectOnNodes(
		Node currentNode,
		in Grid grid,
		in Grid originalGrid,
		HashSet<Node> nodesSupposedOff,
		StepGathererOptions options,
		ref HashSet<Node> nodes
	)
	{
	}

	/// <summary>
	/// Collects nodes that is supposed to "off" from the current node supposed to "on", storing them into <paramref name="nodes"/>.
	/// </summary>
	/// <param name="currentNode">Indicates the current node.</param>
	/// <param name="grid">Indicates the grid.</param>
	/// <param name="options">Indicates the options.</param>
	/// <param name="nodes">The nodes collected.</param>
	public virtual void CollectOffNodes(Node currentNode, in Grid grid, StepGathererOptions options, ref HashSet<Node> nodes)
	{
	}

	/// <summary>
	/// Collects for extra view nodes for the pattern.
	/// This method will be useful in advanced chaining rules such as ALS and AUR extra maps checking.
	/// </summary>
	/// <param name="grid">Indicates the grid used.</param>
	/// <param name="pattern">Indicates the chain pattern.</param>
	/// <param name="view">Indicates the view.</param>
	/// <param name="currentAlsIndex">The temporary index that records the current ALS index applied.</param>
	/// <param name="currentUrIndex">The temporary index that records the current UR index applied.</param>
	/// <param name="producedViewNodes">Indicates the produced view nodes.</param>
	public virtual void GetViewNodes(
		in Grid grid,
		Chain pattern,
		View view,
		ref int currentAlsIndex,
		ref int currentUrIndex,
		out ReadOnlySpan<ViewNode> producedViewNodes
	) => producedViewNodes = [];

	/// <summary>
	/// Try to find extra eliminations that can only be created inside a Grouped Continuous Nice Loop.
	/// This method will be useful in advanced chaining rules such as ALS, AHS and AUR eliminations checking.
	/// </summary>
	/// <param name="grid">Indicates the grid.</param>
	/// <param name="links">Indicates the links.</param>
	/// <param name="conclusions">
	/// Indicates the conclusions. Use operator <see cref="ConclusionSet.op_BitwiseOr(ConclusionSet, ConclusionSet)"/>
	/// to collect conclusions.
	/// </param>
	/// <returns>A list of found conclusions.</returns>
	/// <seealso cref="ConclusionSet.op_BitwiseOr(ConclusionSet, ConclusionSet)"/>
	[Cached]
	public virtual void GetLoopConclusions(in Grid grid, ReadOnlySpan<Link> links, ref ConclusionSet conclusions)
	{
	}
}
