namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// <para>Represents a link that describes a relation between two <see cref="Node"/> instances.</para>
/// <para><b>
/// Please note that two <see cref="Link"/> instances will be considered as equal
/// only if they holds same node values, regardless of what link type two <see cref="Link"/> instances use.
/// </b></para>
/// </summary>
/// <param name="firstNode">Indicates the first node to be used.</param>
/// <param name="secondNode">Indicates the second node to be used.</param>
/// <param name="isStrong">Indicates whether the link type is a strong link or not.</param>
/// <param name="groupedLinkPattern">
/// Indicates the pattern that the grouped link used. The value can be used as a "tag" recording extra information.
/// The default value is <see langword="null"/>.
/// </param>
/// <seealso cref="Node"/>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.EqualityOperators)]
public sealed partial class Link(
	[Property] Node firstNode,
	[Property] Node secondNode,
	[Property] bool isStrong,
	[Property] Pattern? groupedLinkPattern = null
) :
	IComponent,
	IEquatable<Link>,
	IEqualityOperators<Link, Link, bool>
{
	/// <summary>
	/// Indicates the strong link connector.
	/// </summary>
	private const string StrongLinkConnector = " == ";

	/// <summary>
	/// Indicates the weak link connector.
	/// </summary>
	private const string WeakLinkConnector = " -- ";


	/// <summary>
	/// Indicates whether the link is grouped (has a node using at least 2 candidates).
	/// </summary>
	public bool IsGrouped => FirstNode.Map.Count != 1 || SecondNode.Map.Count != 1;

	/// <summary>
	/// Indicates whether the link is strictly grouped (uses grouped node, or contains a grouped pattern).
	/// </summary>
	public bool IsStrictlyGrouped => IsGrouped || GroupedLinkPattern is not null;

	/// <summary>
	/// Indicates whether the link is inside a cell.
	/// </summary>
	public bool IsBivalueCellLink
		=> this is ({ Map: { Cells: [var c1], Digits: var d1 } }, { Map: { Cells: [var c2], Digits: var d2 } })
		&& c1 == c2 && d1 != d2 && BitOperations.IsPow2(d1) && BitOperations.IsPow2(d2);

	/// <inheritdoc/>
	ComponentType IComponent.Type => ComponentType.ChainLink;


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out Node firstNode, out Node secondNode) => (firstNode, secondNode) = (FirstNode, SecondNode);

	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out Node firstNode, out Node secondNode, out bool isStrong)
		=> ((firstNode, secondNode), isStrong) = (this, IsStrong);

	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out Node firstNode, out Node secondNode, out bool isStrong, out Pattern? groupedLinkPattern)
		=> ((firstNode, secondNode, isStrong), groupedLinkPattern) = (this, GroupedLinkPattern);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals([NotNullWhen(true)] Link? other) => other is not null && Equals(other, LinkComparison.Undirected);

	/// <summary>
	/// Determine whether two <see cref="Link"/> are considered equal on the specified comparison rule.
	/// </summary>
	/// <param name="other">The other object to be compared.</param>
	/// <param name="comparison">The comparison rule to be used.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when the argument <paramref name="comparison"/> is not defined.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(Link other, LinkComparison comparison)
		=> Enum.IsDefined(comparison)
			? IsStrong == other.IsStrong && comparison switch
			{
				LinkComparison.Undirected
					=> FirstNode == other.FirstNode && SecondNode == other.SecondNode
					|| FirstNode == other.SecondNode && SecondNode == other.FirstNode,
				_ => FirstNode == other.FirstNode && SecondNode == other.SecondNode
			}
			: throw new ArgumentOutOfRangeException(nameof(comparison));

	/// <inheritdoc/>
	public override int GetHashCode() => GetHashCode(LinkComparison.Undirected);

	/// <summary>
	/// Serves as hash code functions, with consideration on the specified comparison rule.
	/// </summary>
	/// <param name="comparison">The comparison rule.</param>
	/// <returns>An <see cref="int"/> indicating the hash code.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when the argument <paramref name="comparison"/> is not defined.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetHashCode(LinkComparison comparison)
		=> Enum.IsDefined(comparison)
			? comparison switch
			{
				LinkComparison.Undirected => HashCode.Combine(FirstNode.GetHashCode() ^ SecondNode.GetHashCode()),
				_ => HashCode.Combine(FirstNode, SecondNode)
			}
			: throw new ArgumentOutOfRangeException(nameof(comparison));

	/// <inheritdoc/>
	public override string ToString() => $"{FirstNode}{(IsStrong ? StrongLinkConnector : WeakLinkConnector)}{SecondNode}";
}
