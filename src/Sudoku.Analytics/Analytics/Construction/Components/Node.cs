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
public sealed class Node(in CandidateMap map, bool isOn, NodeParents parents = default) :
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
	private readonly CandidateMap _map = map;


	/// <summary>
	/// Indicates whether the node is a grouped node.
	/// </summary>
	public bool IsGroupedNode => _map.Count >= 2;

	/// <summary>
	/// Indicates whether the node is on.
	/// </summary>
	public bool IsOn { get; } = isOn;

	/// <summary>
	/// Indicates the map of candidates the node uses.
	/// </summary>
	public ref readonly CandidateMap Map => ref _map;

	/// <summary>
	/// <para>
	/// Indicates the parent node. The value can be <see langword="default"/> if no parent specified.
	/// </para>
	/// <para><b>This property doesn't participate in equality comparison.</b></para>
	/// </summary>
	public NodeParents Parents { get; set; } = parents;

	/// <inheritdoc/>
	public Node Root
	{
		get
		{
			var (result, p) = (this, Parents.AsNode());
			while (p is not null)
			{
				result = p;
				p = p.Parents.AsNode();
			}
			return result;
		}
	}

	/// <inheritdoc/>
	ComponentType IComponent.Type => ComponentType.ChainNode;

	/// <inheritdoc/>
	Node? IParentLinkedNode<Node>.Parent => Parents.AsNode();

	/// <summary>
	/// The backing comparing value on <see cref="IsOn"/> property.
	/// </summary>
	/// <seealso cref="IsOn"/>
	private int IsOnPropertyValue => IsOn ? 1 : 0;


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruct-method']/target[@name='method']"/>
	public void Deconstruct(out bool isGroupedNode, out CandidateMap map) => (isGroupedNode, map) = (IsGroupedNode, _map);

	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruct-method']/target[@name='method']"/>
	public void Deconstruct(out bool isGroupedNode, out CandidateMap map, out NodeParents parents)
		=> ((isGroupedNode, map), parents) = (this, Parents);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as Node);

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
		for (var node = childNode; node is not null; node = node.Parents.AsNode())
		{
			if (Equals(node, nodeComparison))
			{
				return true;
			}
		}
		return false;
	}

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(_map, IsOn);

	/// <summary>
	/// Calculates the hash code on the current instance.
	/// </summary>
	/// <param name="comparison">The comparison rule.</param>
	/// <returns>An <see cref="int"/> value as the result.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the argument <paramref name="comparison"/> is not defined.
	/// </exception>
	public int GetHashCode(NodeComparison comparison)
		=> comparison switch
		{
			NodeComparison.IncludeIsOn => HashCode.Combine(_map.GetHashCode(), IsOn),
			NodeComparison.IgnoreIsOn => HashCode.Combine(_map.GetHashCode()),
			_ => throw new ArgumentOutOfRangeException(nameof(comparison))
		};

	/// <inheritdoc/>
	public int CompareTo(Node? other) => CompareTo(other, NodeComparison.IgnoreIsOn);

	/// <inheritdoc cref="CompareTo(Node?)"/>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the argument <paramref name="comparison"/> is not defined.
	/// </exception>
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
	public override string ToString() => ToString(CoordinateConverter.InvariantCulture);

	/// <summary>
	/// Converts the current instance into <see cref="string"/> representation.
	/// </summary>
	/// <param name="format">
	/// Format description:
	/// <list type="table">
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
	/// </param>
	/// <returns>The string.</returns>
	public string ToString(string? format)
		=> (format ?? $"{MapFormatString}: {IsOnFormatString}")
			.Replace(MapFormatString, _map.ToString())
			.Replace(IsOnFormatString, IsOn.ToString().ToLower());

	/// <inheritdoc/>
	public string ToString(CultureInfo culture)
	{
		var converter = CoordinateConverter.GetInstance(culture);
		return $"{converter.CandidateConverter(_map)}: {IsOn}";
	}

	/// <inheritdoc/>
	public string ToString(CoordinateConverter converter) => $"{converter.CandidateConverter(_map)}: {IsOn}";

	/// <inheritdoc/>
	public string ToString(ICandidateMapConverter converter) => ToString(converter, null);

	/// <inheritdoc/>
	public string ToString(ICandidateMapConverter converter, IFormatProvider? formatProvider)
	{
		var candidatesString = converter.TryFormat(in _map, formatProvider, out var result) ? result : throw new FormatException();
		return $"{candidatesString}: {IsOn}";
	}

	/// <inheritdoc cref="ICloneable.Clone"/>
	public Node Clone() => new(_map, IsOn) { Parents = Parents };

	/// <inheritdoc/>
	object ICloneable.Clone() => Clone();


	/// <summary>
	/// Creates a <see cref="Node"/> instance with a list of a single parent node or a list of parent nodes.
	/// </summary>
	/// <param name="node">The current node.</param>
	/// <param name="parents">The parent nodes.</param>
	/// <returns>The new node created.</returns>
	public static Node Create(Node node, NodeParents parents) => new(in node._map, node.IsOn, parents);

	/// <inheritdoc/>
	static Node IParentLinkedNode<Node>.Create(Node current, Node? parent) => Create(current, parent ?? default(NodeParents));


	/// <inheritdoc/>
	public static bool operator ==(Node? left, Node? right)
		=> (left, right) switch { (null, null) => true, (not null, not null) => left.Equals(right), _ => false };

	/// <inheritdoc/>
	public static bool operator !=(Node? left, Node? right) => !(left == right);

	/// <inheritdoc/>
	public static bool operator >(Node left, Node right) => left.CompareTo(right) > 0;

	/// <inheritdoc/>
	public static bool operator <(Node left, Node right) => left.CompareTo(right) < 0;

	/// <inheritdoc/>
	public static bool operator >=(Node left, Node right) => left.CompareTo(right) >= 0;

	/// <inheritdoc/>
	public static bool operator <=(Node left, Node right) => left.CompareTo(right) <= 0;

	/// <summary>
	/// Negates the node with <see cref="IsOn"/> property value.
	/// </summary>
	/// <param name="value">The current node.</param>
	/// <returns>The node negated.</returns>
	public static Node operator ~(Node value) => new(value._map, !value.IsOn) { Parents = value.Parents };
}
