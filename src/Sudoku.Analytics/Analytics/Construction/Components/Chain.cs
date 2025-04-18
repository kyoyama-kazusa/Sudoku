namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Represents a node-based chain pattern,
/// and such pattern contains a list of <see cref="Node"/> instances indicating interim passing nodes.
/// </summary>
/// <seealso cref="Node"/>
[TypeImpl(
	TypeImplFlags.Object_Equals | TypeImplFlags.Object_ToString | TypeImplFlags.AllEqualityComparisonOperators,
	OtherModifiersOnEquals = "sealed",
	ToStringBehavior = ToStringBehavior.MakeAbstract)]
public abstract partial class Chain :
	IComparable<Chain>,
	IComparisonOperators<Chain, Chain, bool>,
	IChainOrForcingChains,
	IComponent,
	IEnumerable<Node>,
	IEquatable<Chain>,
	IEqualityOperators<Chain, Chain, bool>,
	IFormattable,
	IParsable<Chain>,
	IReadOnlyList<Node>,
	IReadOnlyCollection<Node>
{
	/// <summary>
	/// Indicates the possible inferences to be used.
	/// </summary>
	protected internal static readonly Inference[] Inferences = [Inference.Strong, Inference.Weak];


	/// <summary>
	/// The backing field of nodes stored. Such nodes may contain ones that are also be treated as conclusions.
	/// </summary>
	protected readonly Node[] _nodes;

	/// <summary>
	/// Indicates the links used in dynamic chaining rule.
	/// </summary>
	protected readonly HashSet<Link>? _dynamicLinks = [];


	/// <summary>
	/// Initializes <see cref="Chain"/> data.
	/// </summary>
	/// <param name="lastNode">The last node.</param>
	/// <param name="isLoop">Indicates whether is for loop initialization.</param>
	/// <param name="autoReversingOnComparison">
	/// <para>
	/// Indicates whether the constructor will automatically reverse the chain
	/// if the first node is greater than the last node, in order to make a good look.
	/// </para>
	/// <para>
	/// The default value is <see langword="true"/>. You can also set the value with <see langword="false"/>
	/// if you don't want to make the constructor reverse the whole chain.
	/// </para>
	/// </param>
	/// <param name="isDynamicChaining">Indicates whether the initialization will respect dynamic chaining rule.</param>
	protected Chain(Node lastNode, bool isLoop, bool autoReversingOnComparison = true, bool isDynamicChaining = false)
	{
		if (isDynamicChaining)
		{
			IsDynamic = true;

			var nodes = new HashSet<Node> { lastNode };
			var links = new HashSet<Link>();
			var queue = new LinkedList<Node>();
			queue.AddLast(lastNode);
			while (queue.Count != 0)
			{
				var currentNode = queue.RemoveFirstNode();
				foreach (var parentNode in currentNode.Parents ?? [])
				{
					queue.AddLast(parentNode);
					nodes.Add(parentNode);
					links.Add(new(parentNode, currentNode, !parentNode.IsOn, null));
				}
			}

			_nodes = [.. nodes];
			_dynamicLinks = links;
		}
		else
		{
			IsDynamic = false;

			var nodes = (List<Node>)[lastNode];
			for (var node = (Node)lastNode.Parents!; isLoop ? node != lastNode : node is not null; node = (Node)node.Parents!)
			{
				nodes.Add(node >> null);
			}
			_nodes = [.. nodes];

			// Now reverse the chain if worth.
			// If the last node is supposed 'on', it will be a normal elimination-typed chain, and can be reversed.
			// Now we should reverse the whole chain if the first node is greater than the last node in logic.
			if (autoReversingOnComparison && _nodes[^1].IsOn && nodes[1].CompareTo(nodes[^2], NodeComparison.IgnoreIsOn) >= 0)
			{
				Reverse();
			}
		}
	}


	/// <summary>
	/// Indicates whether the current pattern is bound with a technique (i.e. having a technique name).
	/// </summary>
	/// <remarks>
	/// Due to design of this type, the derived types may not be consumed by showing a pattern.
	/// It may be a "segment" of a whole pattern (for example, a branch inside a multiple forcing chains).
	/// In such cases, this property will return <see langword="false"/>, indicating there's no name corresponding to the pattern.
	/// </remarks>
	public abstract bool IsNamed { get; }

	/// <summary>
	/// Indicates whether the current chain is dynamic.
	/// </summary>
	public bool IsDynamic { get; }

	/// <inheritdoc/>
	public bool IsGrouped => ValidNodes.Any(static node => node.IsGroupedNode);

	/// <inheritdoc/>
	public bool IsStrictlyGrouped => IsStrongLinksStrictlyGrouped || IsWeakLinksStrictlyGrouped;

	/// <summary>
	/// Indicates whether the pattern only uses same digits.
	/// </summary>
	public bool IsX
	{
		get
		{
			var digitsMask = (Mask)0;
			foreach (var node in ValidNodes)
			{
				digitsMask |= node.Map.Digits;
			}
			return BitOperations.IsPow2(digitsMask);
		}
	}

	/// <summary>
	/// Indicates whether the pattern only uses cell strong links.
	/// </summary>
	public bool IsY
	{
		get
		{
			foreach (var link in StrongLinks)
			{
				if (link is ({ Map.Digits: var digits1 }, { Map.Digits: var digits2 }) && digits1 == digits2)
				{
					return false;
				}
			}
			return First.Map.Digits == Last.Map.Digits;
		}
	}

	/// <summary>
	/// Indicates whether at least one node in the whole pattern overlaps with a node.
	/// </summary>
	public bool IsOverlapped
	{
		get
		{
			foreach (var nodePair in (from node in ValidNodes select node.Map).GetSubsets(2))
			{
				ref readonly var map1 = ref nodePair[0];
				ref readonly var map2 = ref nodePair[1];
				if (map1 & map2)
				{
					return true;
				}
			}
			return false;
		}
	}

	/// <summary>
	/// Indicates the length of the pattern - the number of nodes used inside a chain.
	/// Use minus-one operation if you want to check the number of links inside it.
	/// </summary>
	public int Length => ValidNodes.Length;

	/// <summary>
	/// Indicates the complexity of the pattern.
	/// </summary>
	/// <remarks>
	/// The value is different with <see cref="Length"/> on a chain starting and ending with itself,
	/// both are by strong links;
	/// however it sometimes is equal to <see cref="Length"/>. It depends on the kind of the chain rule obeys.
	/// For example, a loop has a same complexity and length.
	/// </remarks>
	/// <seealso cref="Length"/>
	public virtual int Complexity => Length;

	/// <summary>
	/// Indicates all digits used in this pattern.
	/// </summary>
	public Mask DigitsMask
	{
		get
		{
			var result = (Mask)0;
			foreach (var node in this)
			{
				result |= node.Map.Digits;
			}
			return result;
		}
	}

	/// <summary>
	/// Indicates the links used.
	/// </summary>
	public ReadOnlySpan<Link> Links => LinksCore;

	/// <summary>
	/// Indicates the strong links.
	/// </summary>
	public ReadOnlySpan<Link> StrongLinks
	{
		get
		{
			var result = new List<Link>(ValidNodes.Length >> 1);
			foreach (var link in Links)
			{
				if (link.IsStrong)
				{
					result.Add(link);
				}
			}
			return result.AsSpan();
		}
	}

	/// <summary>
	/// Indicates the weak links.
	/// </summary>
	public ReadOnlySpan<Link> WeakLinks
	{
		get
		{
			var result = new List<Link>(Links.Length >> 1);
			foreach (var link in Links)
			{
				if (!link.IsStrong)
				{
					result.Add(link);
				}
			}
			return result.AsSpan();
		}
	}

	/// <summary>
	/// Indicates the head node.
	/// </summary>
	public Node First => ValidNodes[0];

	/// <summary>
	/// Indicates the tail node.
	/// </summary>
	public Node Last => ValidNodes[^1];

	/// <summary>
	/// Indicates the value as the start index of the chain link is from whether strong and weak.
	/// </summary>
	protected internal abstract int WeakStartIdentity { get; }

	/// <summary>
	/// Indicates the valid nodes to be used.
	/// </summary>
	protected internal abstract ReadOnlySpan<Node> ValidNodes { get; }

	/// <summary>
	/// Indicates whether at least one link in strong links contains grouped nodes,
	/// and is not advanced node (i.e. contains grouped pattern).
	/// </summary>
	internal bool IsStrongLinksStrictlyGrouped
	{
		get
		{
			foreach (var link in StrongLinks)
			{
				if (link is ({ Map.Count: var d1 }, { Map.Count: var d2 }, _, var groupedPattern)
					&& (d1 != 1 || d2 != 1 || groupedPattern is not null))
				{
					return true;
				}
			}
			return false;
		}
	}

	/// <summary>
	/// Indicates whether at least one link in weak links contains grouped nodes,
	/// and is not advanced node (i.e. contains grouped pattern).
	/// </summary>
	internal bool IsWeakLinksStrictlyGrouped
	{
		get
		{
			foreach (var link in WeakLinks)
			{
				if (link is ({ Map.Count: var d1 }, { Map.Count: var d2 }, _, var groupedPattern)
					&& (d1 != 1 || d2 != 1 || groupedPattern is not null))
				{
					return true;
				}
			}
			return false;
		}
	}

	/// <summary>
	/// Indicates the value on loop checking for link construction usages.
	/// </summary>
	protected abstract int LoopIdentity { get; }

	/// <inheritdoc/>
	int IReadOnlyCollection<Node>.Count => Length;

	/// <inheritdoc/>
	ComponentType IComponent.Type => ComponentType.Chain;

	/// <inheritdoc cref="Links"/>
	[field: MaybeNull]
	private Link[] LinksCore
	{
		get
		{
			if (field is null)
			{
				if (IsDynamic)
				{
					field = [.. _dynamicLinks?.ToArray() ?? []];
				}
				else
				{
					var span = ValidNodes;
					var resultLength = Length - LoopIdentity;
					var result = new Link[resultLength];
					for (var (linkIndex, i) = (WeakStartIdentity, 0); i < resultLength; linkIndex++, i++)
					{
						var isStrong = Inferences[linkIndex & 1] == Inference.Strong;
						var pool = (isStrong ? StrongLinkDictionary : WeakLinkDictionary).GroupedLinkPool;
						pool.TryGetValue(new(span[i], span[(i + 1) % Length], isStrong), out var pattern);
						result[i] = new(span[i], span[(i + 1) % Length], isStrong, pattern);
					}
					field = result;
				}
			}
			return field;
		}
	}


	/// <summary>
	/// Gets a <see cref="Node"/> instance at the specified index.
	/// </summary>
	/// <param name="index">The desired index.</param>
	/// <returns>The <see cref="Node"/> instance.</returns>
	/// <exception cref="IndexOutOfRangeException">Throws when the argument <paramref name="index"/> is out of range.</exception>
	public Node this[int index] => ValidNodes[index];


	/// <summary>
	/// Try to reverse the pattern, making all nodes negated its direction connected.
	/// </summary>
	public abstract void Reverse();

	/// <inheritdoc/>
	public bool Equals(Chain? other) => Equals(other, NodeComparison.IgnoreIsOn, ChainComparison.Undirected);

	/// <summary>
	/// Determine whether two <see cref="AlternatingInferenceChain"/> or <see cref="ContinuousNiceLoop"/> instances are same, by using the specified comparison rule.
	/// </summary>
	/// <param name="other">The other instance to be compared.</param>
	/// <param name="nodeComparison">The comparison rule on nodes.</param>
	/// <param name="patternComparison">The comparison rule on the whole chain.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the argument <paramref name="patternComparison"/> is not defined.
	/// </exception>
	public abstract bool Equals([NotNullWhen(true)] Chain? other, NodeComparison nodeComparison, ChainComparison patternComparison);

	/// <summary>
	/// Determines whether the current pattern (nodes) overlap with a list of conclusions,
	/// meaning at least one conclusion is used by a node appeared in the pattern.
	/// If so, the pattern will become a cannibalistic one.
	/// </summary>
	/// <param name="conclusions">The conclusions to be checked.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool OverlapsWithConclusions(ConclusionSet conclusions)
	{
		foreach (var conclusion in conclusions)
		{
			foreach (var node in ValidNodes)
			{
				if (node.Map.Contains(conclusion.Candidate))
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Determines whether the pattern has already used the specified candidate.
	/// </summary>
	/// <param name="candidate">The candidate.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool Contains(Candidate candidate)
	{
		foreach (var node in this)
		{
			if (node.Map.Contains(candidate))
			{
				return true;
			}
		}
		return false;
	}

	/// <inheritdoc cref="object.GetHashCode"/>
	/// <remarks>
	/// This method directly calls <see cref="GetHashCode(NodeComparison, ChainComparison)"/>
	/// with values <see cref="NodeComparison.IgnoreIsOn"/> and <see cref="ChainComparison.Undirected"/>.
	/// </remarks>
	/// <seealso cref="GetHashCode(NodeComparison, ChainComparison)"/>
	/// <seealso cref="NodeComparison.IgnoreIsOn"/>
	/// <seealso cref="ChainComparison.Undirected"/>
	public sealed override int GetHashCode() => GetHashCode(NodeComparison.IgnoreIsOn, ChainComparison.Undirected);

	/// <summary>
	/// Computes hash code based on the current instance.
	/// </summary>
	/// <param name="nodeComparison">The node comparison.</param>
	/// <param name="patternComparison">The pattern comparison.</param>
	/// <returns>An <see cref="int"/> as the result.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the argument <paramref name="patternComparison"/> is not defined.
	/// </exception>
	public abstract int GetHashCode(NodeComparison nodeComparison, ChainComparison patternComparison);

	/// <summary>
	/// Try to find a node satisfying the specified condition, and return its index. If none found, -1 will be returned.
	/// </summary>
	/// <param name="predicate">The condition that a node should satisfy.</param>
	/// <returns>The index of the node satisfied the condition.</returns>
	public int FindIndex(Predicate<Node> predicate)
	{
		for (var i = 0; i < Length; i++)
		{
			if (predicate(this[i]))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Try to find a node satisfying the specified condition from end, and return its index. If none found, -1 will be returned.
	/// </summary>
	/// <param name="predicate">The condition that a node should satisfy.</param>
	/// <returns>The index of the node satisfied the condition.</returns>
	public int FindLastIndex(Predicate<Node> predicate)
	{
		for (var i = Length - 1; i >= 0; i--)
		{
			if (predicate(this[i]))
			{
				return i;
			}
		}
		return -1;
	}

	/// <inheritdoc/>
	public abstract int CompareTo(Chain? other);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public string ToString(IFormatProvider? formatProvider)
	{
		return IsDynamic
			? $"{getDynamicSpaces(StrongLinks)}; {getDynamicSpaces(WeakLinks)}"
			: formatProvider switch
			{
				ChainFormatInfo f => ChainFormatInfo.FormatCoreUnsafeAccessor(f, this),
				_ => ToString(ChainFormatInfo.Standard)
			};


		static SpaceSet getDynamicSpaces(ReadOnlySpan<Link> links)
		{
			var spaces = SpaceSet.Empty;
			foreach (var link in links)
			{
				ref readonly var start = ref link.FirstNode.Map;
				ref readonly var end = ref link.SecondNode.Map;
				var startDigits = start.Digits;
				var endDigits = end.Digits;
				var startCells = start.Cells;
				var endCells = end.Cells;
				if (startDigits == endDigits && BitOperations.IsPow2(startDigits))
				{
					// Check for cells.
					var combined = startCells | endCells;
					if (combined.FirstSharedHouse is var house and not 32)
					{
						spaces.Add(
							house switch
							{
								< 9 => Space.BlockNumber(house, BitOperations.Log2(startDigits)),
								< 18 => Space.RowNumber(house - 9, BitOperations.Log2(startDigits)),
								_ => Space.ColumnNumber(house - 18, BitOperations.Log2(startDigits))
							}
						);
					}
				}
				else
				{
					// Check for digits.
					if (startCells == endCells && startCells is [var cell])
					{
						spaces.Add(Space.RowColumn(cell / 9, cell % 9));
					}
				}
			}
			return spaces;
		}
	}

	/// <summary>
	/// Slices the collection with the specified start node and its length.
	/// </summary>
	/// <param name="start">The start index.</param>
	/// <param name="length">The number of <see cref="Node"/> instances to slice.</param>
	/// <returns>A <see cref="ReadOnlySpan{T}"/> of <see cref="Node"/> instances returned.</returns>
	public ReadOnlySpan<Node> Slice(int start, int length) => ValidNodes[start..(start + length)];

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Enumerator GetEnumerator() => new(this);

	/// <inheritdoc/>
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(formatProvider);

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => _nodes.GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<Node> IEnumerable<Node>.GetEnumerator() => _nodes.AsEnumerable().GetEnumerator();


	/// <inheritdoc cref="TryParse(string?, IFormatProvider?, out Chain)"/>
	public static bool TryParse([NotNullWhen(true)] string? s, [NotNullWhen(true)] out Chain? result)
		=> TryParse(s, null, out result);

	/// <inheritdoc/>
	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Chain result)
	{
		try
		{
			if (s is null)
			{
				result = null;
				return false;
			}

			result = Parse(s, provider);
			return true;
		}
		catch (FormatException)
		{
			result = null;
			return false;
		}
	}

	/// <inheritdoc cref="Parse(string, IFormatProvider?)"/>
	public static Chain Parse(string s) => Parse(s, null);

	/// <inheritdoc/>
	public static Chain Parse(string s, IFormatProvider? provider)
	{
		var parser = (ChainFormatInfo)((provider as ChainFormatInfo) ?? ChainFormatInfo.Standard);
		return ChainFormatInfo.ParseCoreUnsafeAccessor(parser, s);
	}
}
