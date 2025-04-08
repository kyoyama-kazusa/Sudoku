namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Represents an instance that describes for cell forcing chains or region forcing chains (i.e. house forcing chains).
/// </summary>
/// <param name="conclusions">Indicates the conclusions.</param>
/// <remarks>
/// Please note that the concept "Forcing Chains" is always represented as a plural noun,
/// i.e. the concept represents a list of chains, rather than "Forcing Chain".
/// </remarks>
/// <seealso cref="SortedDictionary{TKey, TValue}"/>
/// <seealso cref="Candidate"/>
/// <seealso cref="StrongForcingChain"/>
/// <seealso cref="WeakForcingChain"/>
/// <seealso cref="Node"/>
[TypeImpl(
	TypeImplFlags.Object_Equals | TypeImplFlags.Object_ToString | TypeImplFlags.AllEqualityComparisonOperators,
	OtherModifiersOnEquals = "sealed",
	OtherModifiersOnToString = "sealed")]
public partial class MultipleForcingChains([Property(Setter = PropertySetters.InternalSet)] params Conclusion[] conclusions) :
	SortedDictionary<Candidate, UnnamedChain>,
	IAnyAllMethod<MultipleForcingChains, KeyValuePair<Candidate, UnnamedChain>>,
	IComparable<MultipleForcingChains>,
	IComparisonOperators<MultipleForcingChains, MultipleForcingChains, bool>,
	IDynamicForcingChains,
	IEquatable<MultipleForcingChains>,
	IEqualityOperators<MultipleForcingChains, MultipleForcingChains, bool>,
	IMultipleForcingChains<MultipleForcingChains, UnnamedChain>
{
	/// <inheritdoc/>
	public virtual bool IsCellMultiple => Candidates.Cells.Count == 1;

	/// <inheritdoc/>
	public virtual bool IsHouseMultiple => Mask.IsPow2(Candidates.Digits);

	/// <inheritdoc/>
	public virtual bool IsAdvancedMultiple => false;

	/// <inheritdoc/>
	public bool IsDynamic => this.AnyValue(static branch => branch.IsDynamic);

	/// <inheritdoc/>
	public bool IsGrouped => Values.Any(static v => v.IsGrouped);

	/// <inheritdoc/>
	public bool IsStrictlyGrouped => Values.Any(static v => v.IsStrictlyGrouped);

	/// <inheritdoc/>
	public int Complexity => BranchedComplexity.Sum();

	/// <inheritdoc/>
	public Mask DigitsMask
	{
		get
		{
			var result = (Mask)0;
			foreach (var element in this)
			{
				foreach (var value in element.Value)
				{
					result |= value.Map.Digits;
				}
			}
			return result;
		}
	}

	/// <inheritdoc/>
	public ReadOnlySpan<int> BranchedComplexity => (from v in Values select v.Length).ToArray();

	/// <inheritdoc/>
	public CandidateMap Candidates => [.. Keys];

	/// <inheritdoc/>
	ComponentType IComponent.Type => ComponentType.MultipleForcingChains;

	/// <inheritdoc/>
	StepConclusions IForcingChains.Conclusions => Conclusions;

	/// <inheritdoc/>
	ReadOnlySpan<UnnamedChain> IForcingChains.Branches => Values.ToArray();


	/// <inheritdoc/>
	public bool Exists(Func<UnnamedChain, bool> predicate)
	{
		foreach (var element in Values)
		{
			if (predicate(element))
			{
				return true;
			}
		}
		return false;
	}

	/// <inheritdoc/>
	public bool TrueForAll(Func<UnnamedChain, bool> predicate)
	{
		foreach (var element in Values)
		{
			if (!predicate(element))
			{
				return false;
			}
		}
		return true;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int CompareTo(MultipleForcingChains? other) => CompareTo(other, NodeComparison.IgnoreIsOn);

	/// <summary>
	/// Compares the value with the other one, to get which one is greater.
	/// </summary>
	/// <param name="other">The other instance to be compared.</param>
	/// <param name="nodeComparison">The node comparison rule.</param>
	/// <returns>An <see cref="int"/> value indicating which instance is better.</returns>
	public int CompareTo(MultipleForcingChains? other, NodeComparison nodeComparison)
	{
		if (other is null)
		{
			return -1;
		}

		if (Count.CompareTo(other.Count) is var r1 and not 0)
		{
			return r1;
		}

		var (map1, map2) = (Candidates, other.Candidates);
		if (map1.CompareTo(map2) is var r2 and not 0)
		{
			return r2;
		}

		foreach (var candidate in map1)
		{
			switch (this[candidate], other[candidate])
			{
				case (StrongForcingChain c, StrongForcingChain d) when c.CompareTo(d, nodeComparison) is var r3 and not 0:
				{
					return r3;
				}
				case (WeakForcingChain c, WeakForcingChain d) when c.CompareTo(d, nodeComparison) is var r3 and not 0:
				{
					return r3;
				}
			}
		}
		return 0;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals([NotNullWhen(true)] MultipleForcingChains? other)
		=> Equals(other, NodeComparison.IgnoreIsOn, ChainComparison.Undirected);

	/// <summary>
	/// Determines whether two <see cref="MultipleForcingChains"/> are considered equal.
	/// </summary>
	/// <param name="other">The other instance to be checked.</param>
	/// <param name="nodeComparison">The node comparison rule.</param>
	/// <param name="patternComparison">The chain comparison rule.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool Equals([NotNullWhen(true)] MultipleForcingChains? other, NodeComparison nodeComparison, ChainComparison patternComparison)
	{
		if (other is null)
		{
			return false;
		}

		if (Count != other.Count)
		{
			return false;
		}

		var (map1, map2) = (Candidates, other.Candidates);
		if (map1 != map2)
		{
			return false;
		}

		foreach (var candidate in map1)
		{
			var (chain1, chain2) = (this[candidate], other[candidate]);
			if (!chain1.Equals(chain2, nodeComparison, patternComparison))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Try to cast the current instance into a finned chain pattern.
	/// </summary>
	/// <param name="result">The chain pattern result.</param>
	/// <param name="fins">The fins of the chain.</param>
	/// <returns>
	/// A <see cref="bool"/> result indicating whether the current instance can convert into a valid finned chain pattern.
	/// </returns>
	public bool TryCastToFinnedChain([NotNullWhen(true)] out NamedChain? result, [NotNullWhen(true)] out CandidateMap? fins)
	{
		// Determine whether the chain only contains one elimination.
		if (Conclusions is not [(Elimination, var elimination) conclusion])
		{
			goto ReturnFalse;
		}

		var branches = Values.ToArray();

		// Iterate on each branch, to get whether they can directly points to conclusion.
		var finsFound = CandidateMap.Empty;
		var krakenBranches = new List<UnnamedChain>(2);
		foreach (var branch in branches)
		{
			if (branch is [.. { Length: 1 }, { Map: { PeerIntersection: var p } lastMap }] && p.Contains(elimination))
			{
				finsFound |= lastMap;
				continue;
			}

			if (krakenBranches.Count >= 2)
			{
				goto ReturnFalse;
			}

			krakenBranches.Add(branch);
		}

		// Only if the number of kraken branches is exact 2.
		if (krakenBranches is not [[_, .. var firstKraken], [.. var secondKraken]])
		{
			goto ReturnFalse;
		}

		// If so, a finned chain is found. Now we should merge two branches into one, by rotating one of two branches,
		// and concat two branches.
		(result, fins) = (c([.. firstKraken.Reverse(), .. secondKraken], conclusion), finsFound);
		return true;

	ReturnFalse:
		(result, fins) = (null, null);
		return false;


		static AlternatingInferenceChain c(ReadOnlySpan<Node> nodes, Conclusion conclusion)
		{
			// Find the node at the specified position in nodes.
			var i = 0;
			for (; i < nodes.Length; i++)
			{
				if (nodes[i].Map is [var c] && c == conclusion.Candidate)
				{
					break;
				}
			}
			if (i == nodes.Length)
			{
				throw new InvalidOperationException();
			}

			var currentNode = new Node(in nodes[i].Map, false);
			(i, var (lastNode, isOn)) = ((i + 1) % nodes.Length, (currentNode, true));
			for (var x = 0; x < nodes.Length; i = (i + 1) % nodes.Length, x++)
			{
				currentNode.Parents = new Node(in nodes[i].Map, isOn);
				currentNode = (Node)currentNode.Parents!;
				isOn = !isOn;
			}
			return new(lastNode);
		}
	}

	/// <inheritdoc/>
	public sealed override int GetHashCode() => GetHashCode(NodeComparison.IgnoreIsOn, ChainComparison.Undirected);

	/// <summary>
	/// Calculates a hash code value used for comparison.
	/// </summary>
	/// <param name="nodeComparison">The node comparison rule.</param>
	/// <param name="patternComparison">The chain comparison rule.</param>
	/// <returns>Hash code value.</returns>
	public int GetHashCode(NodeComparison nodeComparison, ChainComparison patternComparison)
	{
		var hashCode = default(HashCode);
		foreach (var (candidate, chain) in this)
		{
			hashCode.Add(candidate);
			hashCode.Add(chain.GetHashCode(nodeComparison, patternComparison));
		}
		return hashCode.ToHashCode();
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToString(IFormatProvider? formatProvider)
	{
		var converter = CoordinateConverter.GetInstance(formatProvider);
		return string.Join(
			", ",
			from kvp in this
			let candidate = kvp.Key
			let pattern = kvp.Value
			select $"{converter.CandidateConverter(candidate.AsCandidateMap())}: {pattern.ToString(converter)}"
		);
	}

	/// <summary>
	/// Try to get all possible conclusions of the multiple forcing chains.
	/// </summary>
	/// <param name="grid">Indicates the current grid to be checked.</param>
	/// <returns>A list of conclusions found.</returns>
	public Conclusion[] GetThoroughConclusions(in Grid grid)
	{
		var map = CandidateMap.Empty;
		foreach (var branch in Values)
		{
			map |= branch[1].Map;
		}

		var newConclusions = new List<Conclusion>();
		foreach (var candidate in map.PeerIntersection)
		{
			if (grid.Exists(candidate) is true)
			{
				newConclusions.Add(new(Elimination, candidate));
			}
		}
		return newConclusions.Count == 0 ? [] : [.. newConclusions];
	}

	/// <summary>
	/// Cast the current instance into a valid finned chain, or throws <see cref="InvalidOperationException"/>
	/// if the current instance cannot be converted.
	/// </summary>
	/// <param name="fins">The fins converted.</param>
	/// <returns>The pattern converted.</returns>
	/// <exception cref="InvalidOperationException">Throws when the current instance cannot be converted.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Chain CastToFinnedChain(out CandidateMap fins)
	{
		if (TryCastToFinnedChain(out var result, out var f))
		{
			fins = f.Value;
			return result;
		}
		throw new InvalidOperationException(SR.ExceptionMessage("CannotCastFinnedChain"));
	}

	/// <summary>
	/// Try to prepare initial view nodes that will be displayed as a finned chain.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="cachedAlsIndex">Indicates currently-used ALS index.</param>
	/// <param name="cachedUrIndex">Indicates currently-used UR index.</param>
	/// <param name="supportedRules">Indicates the supported rules.</param>
	/// <param name="finnedChain">Indicates the finned chain.</param>
	/// <param name="fins">Indicates the fins.</param>
	/// <param name="views">The views.</param>
	protected internal virtual void PrepareFinnedChainViewNodes(
		NamedChain finnedChain,
		ref int cachedAlsIndex,
		ref int cachedUrIndex,
		ChainingRuleCollection supportedRules,
		in Grid grid,
		in CandidateMap fins,
		out View[] views
	)
	{
		views = [
			[
				.. from candidate in fins select new CandidateViewNode(ColorIdentifier.Auxiliary1, candidate),
				.. finnedChain.GetViews_Monoparental(grid, supportedRules, ref cachedAlsIndex, ref cachedUrIndex)[0]
			]
		];

		// Change nodes into fin-like view nodes.
		foreach (var node in (ViewNode[])[.. views[0]])
		{
			if (node is CandidateViewNode { Candidate: var candidate } && fins.Contains(candidate))
			{
				views[0].Remove(node);
				views[0].Add(new CandidateViewNode(ColorIdentifier.Auxiliary2, candidate));
			}
		}
	}

	/// <summary>
	/// Try to create a list of initial view nodes.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <returns>A list of initial view nodes.</returns>
	protected virtual ReadOnlySpan<ViewNode> GetInitialViewNodes(in Grid grid)
		=> (ViewNode[])[
			IsCellMultiple
				? new CellViewNode(ColorIdentifier.Normal, this.First().Key / 9)
				: new HouseViewNode(ColorIdentifier.Normal, HouseMask.TrailingZeroCount(Candidates.Cells.SharedHouses))
		];

	/// <inheritdoc/>
	bool IAnyAllMethod<MultipleForcingChains, KeyValuePair<Candidate, UnnamedChain>>.Any() => Count != 0;

	/// <inheritdoc/>
	bool IAnyAllMethod<MultipleForcingChains, KeyValuePair<Candidate, UnnamedChain>>.Any(Func<KeyValuePair<Candidate, UnnamedChain>, bool> predicate)
	{
		foreach (var kvp in this)
		{
			if (predicate(kvp))
			{
				return true;
			}
		}
		return false;
	}

	/// <inheritdoc/>
	bool IAnyAllMethod<MultipleForcingChains, KeyValuePair<Candidate, UnnamedChain>>.All(Func<KeyValuePair<Candidate, UnnamedChain>, bool> predicate)
	{
		foreach (var kvp in this)
		{
			if (!predicate(kvp))
			{
				return false;
			}
		}
		return true;
	}

	/// <inheritdoc/>
	ReadOnlySpan<ViewNode[]> IForcingChains.GetViewsCore(in Grid grid, ChainingRuleCollection rules, Conclusion[] newConclusions)
	{
		var result = new ViewNode[Count + 1][];
		var initialViewNodes = GetInitialViewNodes(grid);
		var i = 0;
		var isDynamicChaining = IsDynamic;
		var globalView = new List<ViewNode>();
		foreach (var key in Keys)
		{
			var chain = this[key];
			var subview = View.Empty;
			var j = 0;
			foreach (var node in chain)
			{
				// This is a compatible way to fix on and off state -
				// because all forcing chains are reverted by its strong and weak links,
				// so if we use unified way 'node.IsOn' to determine their own color identifiers,
				// the coloring will be wrong due to lack of an reversion-back operation.
				// I'll adjust the backing logic in the future.
				var id = isDynamicChaining
					? node.IsOn ? ColorIdentifier.Normal : ColorIdentifier.Auxiliary1
					: (++j & 1) == 0 ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal;
				foreach (var candidate in node.Map)
				{
					var currentViewNode = new CandidateViewNode(id, candidate);
					globalView.Add(currentViewNode);
					subview.Add(currentViewNode);
				}
			}

			j = 0;
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
			result[++i] = [.. initialViewNodes, .. subview];
		}
		result[0] = [.. initialViewNodes, .. globalView];
		return result;
	}
}
