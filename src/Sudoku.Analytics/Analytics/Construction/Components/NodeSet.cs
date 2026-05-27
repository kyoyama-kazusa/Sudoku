namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Provides with a list of nodes.
/// </summary>
[CollectionBuilder(typeof(NodeSet), nameof(Create))]
public sealed class NodeSet : IComponent, IReadOnlyList<Node>, IReadOnlyCollection<Node>
{
	/// <summary>
	/// Indicates the internal list of nodes.
	/// </summary>
	private readonly List<Node> _nodes = [with(6)];


	/// <summary>
	/// Initializes a <see cref="NodeSet"/> instance via the specified node.
	/// </summary>
	/// <param name="node">The node.</param>
	private NodeSet(Node node) => _nodes = [node];

	/// <summary>
	/// Initializes a <see cref="NodeSet"/> instance via the specified list of nodes.
	/// </summary>
	/// <param name="nodes">The nodes.</param>
	private NodeSet(List<Node> nodes) => _nodes = nodes;


	/// <summary>
	/// Indicates the number of elements in the collection.
	/// </summary>
	public int Length => _nodes.Count;

	/// <inheritdoc/>
	int IReadOnlyCollection<Node>.Count => Length;

	/// <inheritdoc/>
	ComponentType IComponent.Type => ComponentType.ChainNodeSet;


	/// <inheritdoc/>
	public Node this[int index] => _nodes[index];


	/// <inheritdoc/>
	public override string ToString() => ToString(CoordinateConverter.InvariantCulture);

	/// <inheritdoc cref="ToString(CoordinateConverter)"/>
	public string ToString(CultureInfo culture)
		=> $"[{string.Join(", ", from element in _nodes select element.ToString(culture))}]";

	/// <inheritdoc cref="ToString(CoordinateConverter)"/>
	public string ToString(CoordinateConverter converter)
		=> $"[{string.Join(", ", from element in _nodes select element.ToString(converter))}]";

	/// <inheritdoc cref="Node.ToString(ICandidateMapConverter)"/>
	public string ToString(ICandidateMapConverter converter)
		=> $"[{string.Join(", ", from element in _nodes select element.ToString(converter))}]";

	/// <inheritdoc cref="Node.ToString(ICandidateMapConverter, IFormatProvider?)"/>
	public string ToString(ICandidateMapConverter converter, IFormatProvider? formatProvider)
		=> $"[{string.Join(", ", from element in _nodes select element.ToString(converter, formatProvider))}]";

	/// <summary>
	/// Converts the current instance into a <see cref="ReadOnlySpan{T}"/> of <see cref="Node"/> instances.
	/// </summary>
	/// <returns>A <see cref="ReadOnlySpan{T}"/> of <see cref="Node"/> instances.</returns>
	public ReadOnlySpan<Node> AsSpan() => _nodes.AsSpan();

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public AnonymousSpanEnumerator<Node> GetEnumerator() => new(AsSpan());

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => _nodes.GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<Node> IEnumerable<Node>.GetEnumerator() => _nodes.GetEnumerator();


	/// <summary>
	/// Creates a <see cref="NodeSet"/> instance via a list of nodes.
	/// </summary>
	/// <param name="nodes">A list of nodes.</param>
	/// <returns>A <see cref="NodeSet"/> instance returned.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static NodeSet Create(ReadOnlySpan<Node> nodes) => new([.. nodes]);
}
