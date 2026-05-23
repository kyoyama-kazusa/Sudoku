namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Represents a single <see cref="Node"/> instance or a list of <see cref="Node"/> instances
/// (a.k.a. <see cref="NodeSet"/> instance).
/// </summary>
/// <seealso cref="Node"/>
/// <seealso cref="NodeSet"/>
[CollectionBuilder(typeof(NodeParents), nameof(Create))]
public readonly partial union NodeParents(Node, NodeSet) : IEnumerable<Node>
{
	/// <summary>
	/// Returns an enumerator instance that can iterate on each object of this type.
	/// </summary>
	/// <returns>An enumerator instance of this type.</returns>
	public Enumerator GetEnumerator() => new(this);

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator()
		=> (this switch { Node node => (Node[])[node], NodeSet nodes => [.. nodes], _ => [] }).GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<Node> IEnumerable<Node>.GetEnumerator()
		=> (this switch { Node node => (Node[])[node], NodeSet nodes => [.. nodes], _ => [] }).AsEnumerable().GetEnumerator();


	/// <summary>
	/// Creates a <see cref="NodeParents"/> instance via the specified sequence of nodes.
	/// </summary>
	/// <param name="nodes">The nodes.</param>
	/// <returns>A <see cref="NodeParents"/> instance created.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static NodeParents Create(ReadOnlySpan<Node> nodes)
		=> nodes switch { [] => default, [var node] => node, _ => NodeSet.Create(nodes) };


	/// <summary>
	/// Explicit cast from <see cref="NodeParents"/> into a <see cref="Node"/> instance;
	/// if failed to be cast, the result value will be <see langword="null"/> instead of throwing exceptions.
	/// </summary>
	/// <param name="value">The value.</param>
	public static explicit operator Node?(NodeParents value) => value is Node node ? node : null;

	/// <summary>
	/// Explicit cast from <see cref="NodeParents"/> into a <see cref="Node"/> instance;
	/// if failed to be cast, an <see cref="InvalidCastException"/> exception will be thrown.
	/// </summary>
	/// <param name="value">The value.</param>
	public static explicit operator checked Node(NodeParents value)
		=> value is Node node ? node : throw new InvalidCastException();
}
