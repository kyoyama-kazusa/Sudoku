namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Represents a pair of data on a certain <see cref="Node"/>, describing all possible <see cref="Node"/> instances
/// that can implicitly connect to it, grouped by its <see cref="Node.IsOn"/> property value.
/// </summary>
/// <param name="OnNodes">Indicates all possible nodes that can implicitly connect to node, supposed to "on".</param>
/// <param name="OffNodes">Indicates all possible nodes that can implicitly connect to node, supposed to "off".</param>
/// <seealso cref="Node"/>
/// <seealso cref="Node.IsOn"/>
public readonly partial record struct ForcingChainsInfo(HashSet<Node> OnNodes, HashSet<Node> OffNodes) :
	IComponent,
	IEnumerable<Node>,
	IReadOnlyCollection<Node>
{
	/// <inheritdoc/>
	public int Count => OnNodes.Count + OffNodes.Count;

	/// <summary>
	/// Indicates the start node.
	/// </summary>
	public Node StartNode => OnNodes[0].Root;

	/// <inheritdoc/>
	ComponentType IComponent.Type => ComponentType.ForcingChains;


	/// <inheritdoc/>
	public bool Equals(ForcingChainsInfo other) => StartNode == other.StartNode;

	/// <summary>
	/// Determines whether two <see cref="ForcingChainsInfo"/> instances contain a same <see cref="StartNode"/> property,
	/// with the specified comparison rule.
	/// </summary>
	/// <param name="other">The other instance to be compared.</param>
	/// <param name="nodeComparison">The node comparison rule.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool Equals(ForcingChainsInfo other, NodeComparison nodeComparison) => StartNode.Equals(other.StartNode, nodeComparison);

	/// <inheritdoc/>
	public override int GetHashCode() => StartNode.GetHashCode();

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public Enumerator GetEnumerator() => new([.. OnNodes, .. OffNodes]);

	/// <summary>
	/// Try to enumerate for nodes that supposed with "on" state.
	/// </summary>
	/// <returns>An enumerator that can iterate on nodes supposed with "on" state.</returns>
	public NodesEnumerator EnumerateOnNodes() => new(OnNodes.GetEnumerator());

	/// <summary>
	/// Try to enumerate for nodes that supposed with "off" state.
	/// </summary>
	/// <returns>An enumerator that can iterate on nodes supposed with "off" state.</returns>
	public NodesEnumerator EnumerateOffNodes() => new(OffNodes.GetEnumerator());

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => this.AsEnumerable().GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<Node> IEnumerable<Node>.GetEnumerator()
	{
		foreach (var node in OnNodes)
		{
			yield return node;
		}
		foreach (var node in OffNodes)
		{
			yield return node;
		}
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp9/feature[@name='records']/target[@name='method' and @cref='PrintMembers']"/>
	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append($"{nameof(StartNode)} = {StartNode}");
		return true;
	}
}
