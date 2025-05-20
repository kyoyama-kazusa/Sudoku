namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Represents a chain node. There're two ways to use it:
/// <list type="number">
/// <item>To build an AIC pattern. Just use one parent of property <see cref="Parents"/> to build the whole chain.</item>
/// <item>To build a dynamic forcing chain branch. Use multiple parents to build a multi-parent node.</item>
/// </list>
/// </summary>
/// <param name="map"><inheritdoc cref="_map" path="/summary"/></param>
/// <param name="isOn"><inheritdoc cref="IsOn" path="/summary"/></param>
/// <param name="parents"><inheritdoc cref="Parents" path="/summary"/></param>
[TypeImpl(TypeImplFlags.AllObjectMethods | TypeImplFlags.AllEqualityComparisonOperators)]
public sealed partial class Node(in CandidateMap map, bool isOn, NodeSet? parents = null) :
	IComparable<Node>,
	IComparisonOperators<Node, Node, bool>,
	ICloneable,
	IParentLinkedNode<Node>
{
	/// <summary>
	/// Indicates the map format string.
	/// </summary>
	internal const string MapFormatString = "m";

	/// <summary>
	/// Indicates the property <see cref="IsOn"/> format string.
	/// </summary>
	internal const string IsOnFormatString = "s";


	/// <summary>
	/// Indicates the backing map.
	/// </summary>
	[HashCodeMember]
	private readonly CandidateMap _map = map;


	/// <summary>
	/// Indicates whether the node is a grouped node.
	/// </summary>
	public bool IsGroupedNode => _map.Count >= 2;

	/// <summary>
	/// Indicates whether the node is on.
	/// </summary>
	[HashCodeMember]
	public bool IsOn { get; } = isOn;

	/// <summary>
	/// Indicates the map of candidates the node uses.
	/// </summary>
	public ref readonly CandidateMap Map => ref _map;

	/// <inheritdoc/>
	public Node Root
	{
		get
		{
			var (result, p) = (this, (Node?)Parents);
			while (p is not null)
			{
				_ = (result = p, p = (Node?)p.Parents);
			}
			return result;
		}
	}

	/// <summary>
	/// <para>Indicates the parent node. The value can be <see langword="null"/> in handling.</para>
	/// <para>Please note that <i>this value doesn't participate in equality comparison.</i></para>
	/// </summary>
	public NodeSet? Parents { get; set; } = parents;

	/// <inheritdoc/>
	ComponentType IComponent.Type => ComponentType.ChainNode;

	/// <inheritdoc/>
	Node? IParentLinkedNode<Node>.Parent => (Node?)Parents;

	/// <summary>
	/// The backing comparing value on <see cref="IsOn"/> property.
	/// </summary>
	/// <seealso cref="IsOn"/>
	private int IsOnPropertyValue => IsOn ? 1 : 0;


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out bool isGroupedNode, out CandidateMap map) => (isGroupedNode, map) = (IsGroupedNode, _map);

	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out bool isGroupedNode, out CandidateMap map, out NodeSet? parents)
		=> ((isGroupedNode, map), parents) = (this, Parents);

	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] Node? other) => Equals(other, NodeComparison.IncludeIsOn);

	/// <summary>
	/// Compares with two <see cref="Node"/> instances, based on the specified comparison rule.
	/// </summary>
	/// <param name="other">The other instance to be compared.</param>
	/// <param name="comparison">The comparison rule.</param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the argument <paramref name="comparison"/> is not defined.
	/// </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals([NotNullWhen(true)] Node? other, NodeComparison comparison)
		=> other is not null
		&& comparison switch
		{
			NodeComparison.IncludeIsOn => _map == other._map && IsOn == other.IsOn,
			NodeComparison.IgnoreIsOn => _map == other._map,
			_ => throw new ArgumentOutOfRangeException(nameof(comparison))
		};

	/// <summary>
	/// Determines whether the current node is an ancestor of the specified node. 
	/// </summary>
	/// <param name="childNode">The node to be checked.</param>
	/// <param name="nodeComparison">The comparison rule on nodes.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	/// <remarks><b>This method only checks for the first element of all parents.</b></remarks>
	public bool IsAncestorOf(Node childNode, NodeComparison nodeComparison)
	{
		for (var node = childNode; node is not null; node = (Node?)node.Parents)
		{
			if (Equals(node, nodeComparison))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Calculates the hash code on the current instance.
	/// </summary>
	/// <param name="comparison">The comparison rule.</param>
	/// <returns>An <see cref="int"/> value as the result.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the argument <paramref name="comparison"/> is not defined.
	/// </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetHashCode(NodeComparison comparison)
		=> comparison switch
		{
			NodeComparison.IncludeIsOn => HashCode.Combine(_map.GetHashCode(), IsOn),
			NodeComparison.IgnoreIsOn => HashCode.Combine(_map.GetHashCode()),
			_ => throw new ArgumentOutOfRangeException(nameof(comparison))
		};

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int CompareTo(Node? other) => CompareTo(other, NodeComparison.IgnoreIsOn);

	/// <inheritdoc cref="CompareTo(Node?)"/>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the argument <paramref name="comparison"/> is not defined.
	/// </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int CompareTo(Node? other, NodeComparison comparison)
		=> other is null
			? -1
			: comparison switch
			{
				NodeComparison.IncludeIsOn => IsOnPropertyValue.CompareTo(other.IsOnPropertyValue) switch
				{
					var r and not 0 => r,
					_ => _map.CompareTo(in other._map)
				},
				NodeComparison.IgnoreIsOn => _map.CompareTo(in other._map),
				_ => throw new ArgumentOutOfRangeException(nameof(comparison))
			};

	/// <inheritdoc/>
	public string ToString(IFormatProvider? formatProvider)
	{
		var converter = CoordinateConverter.GetInstance(formatProvider);
		return $"{converter.CandidateConverter(_map)}: {IsOn}";
	}

	/// <inheritdoc/>
	/// <remarks>
	/// Format description:
	/// <list type="table">
	/// <listheader>
	/// <term>Format character</term>
	/// <description>Description</description>
	/// </listheader>
	/// <item>
	/// <term><c>m</c></term>
	/// <description>The map text. For example, <c>r1c23(4)</c></description>
	/// </item>
	/// <item>
	/// <term><c>S</c> and <c>s</c></term>
	/// <description>
	/// The <see cref="IsOn"/> property value (<see langword="true"/> or <see langword="false"/>).
	/// If the character <c>s</c> is upper-cased, the result text will be upper-cased on initial letter.
	/// </description>
	/// </item>
	/// </list>
	/// For example, format value <c>"m: S"</c> will be replaced with value <c>"r1c23(4): True"</c>.
	/// </remarks>
	public string ToString(string? format, IFormatProvider? formatProvider)
		=> (format ?? $"{MapFormatString}: {IsOnFormatString}")
			.Replace(MapFormatString, _map.ToString(formatProvider))
			.Replace(IsOnFormatString, IsOn.ToString().ToLower());

	/// <inheritdoc cref="ICloneable.Clone"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Node Clone() => new(_map, IsOn) { Parents = Parents };

	/// <inheritdoc/>
	object ICloneable.Clone() => Clone();


	/// <summary>
	/// Negates the node with <see cref="IsOn"/> property value.
	/// </summary>
	/// <param name="value">The current node.</param>
	/// <returns>The node negated.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Node operator ~(Node value) => new(value._map, !value.IsOn) { Parents = value.Parents };

	/// <summary>
	/// Creates a <see cref="Node"/> instance with parent node.
	/// </summary>
	/// <param name="current">The current node.</param>
	/// <param name="parent">The parent node.</param>
	/// <returns>The new node created.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Node operator >>(Node current, Node? parent) => new(in current._map, current.IsOn, parent);

	/// <summary>
	/// Creates a <see cref="Node"/> instance with a list of parent nodes.
	/// </summary>
	/// <param name="current">The current node.</param>
	/// <param name="parents">The parent nodes.</param>
	/// <returns>The new node created.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Node operator >>(Node current, NodeSet? parents) => new(in current._map, current.IsOn, parents);
}
