namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Represents a blossom loop.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Conclusions" path="/summary"/></param>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.Object_ToString | TypeImplFlags.AllEqualityComparisonOperators)]
public sealed partial class BlossomLoop(params ConclusionSet conclusions) :
	SortedDictionary<Candidate, StrongForcingChain>,
	IComparable<BlossomLoop>,
	IComparisonOperators<BlossomLoop, BlossomLoop, bool>,
	IChainOrForcingChains,
	IComponent,
	IEquatable<BlossomLoop>,
	IEqualityOperators<BlossomLoop, BlossomLoop, bool>,
	IFormattable
{
	/// <summary>
	/// Indicates whether the loop entry is cell type.
	/// </summary>
	public bool EntryIsCellType => !BitOperations.IsPow2(Entries.Digits);

	/// <summary>
	/// Indicates whether the loop exit is cell type.
	/// </summary>
	public bool ExitIsCellType => !BitOperations.IsPow2(Exits.Digits);

	/// <inheritdoc/>
	public bool IsGrouped => Values.Any(static v => v.IsGrouped);

	/// <inheritdoc/>
	public bool IsStrictlyGrouped => Values.Any(static v => v.IsStrictlyGrouped);

	/// <summary>
	/// Indicates the complexity of the whole pattern.
	/// </summary>
	public int Complexity => BranchedComplexity.Sum();

	/// <summary>
	/// Indicates the digits used in this pattern.
	/// </summary>
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

	/// <summary>
	/// Indicates the entry candidates that start each branch.
	/// </summary>
	public CandidateMap Entries => [.. Keys];

	/// <summary>
	/// Indicates the exit candidates that end each branch.
	/// </summary>
	public CandidateMap Exits => [.. from value in Values select value[0].Map[0]];

	/// <summary>
	/// Indicates the complexity of each branch.
	/// </summary>
	public ReadOnlySpan<int> BranchedComplexity => (from chain in Values select chain.Length).ToArray();

	/// <summary>
	/// Indicates the conclusions.
	/// </summary>
	public ConclusionSet Conclusions { get; } = conclusions;

	/// <inheritdoc/>
	ComponentType IComponent.Type => ComponentType.BlossomLoop;


	/// <summary>
	/// Determines whether at least one branch contains at least one element satisfying the specified condition.
	/// </summary>
	/// <param name="predicate">The condition to be checked.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool Exists(Func<Chain, bool> predicate)
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals([NotNullWhen(true)] BlossomLoop? other)
		=> Equals(other, NodeComparison.IgnoreIsOn, ChainComparison.Undirected);

	/// <summary>
	/// Determines whether two <see cref="BlossomLoop"/> are considered equal.
	/// </summary>
	/// <param name="other">The other instance to be checked.</param>
	/// <param name="nodeComparison">The node comparison rule.</param>
	/// <param name="patternComparison">The chain comparison rule.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool Equals([NotNullWhen(true)] BlossomLoop? other, NodeComparison nodeComparison, ChainComparison patternComparison)
	{
		if (other is null)
		{
			return false;
		}

		if (Count != other.Count)
		{
			return false;
		}

		using var e1 = Keys.GetEnumerator();
		using var e2 = other.Keys.GetEnumerator();
		while (e1.MoveNext() && e2.MoveNext())
		{
			var (kvp1, kvp2) = (e1.Current, e2.Current);
			if (kvp1 != kvp2)
			{
				return false;
			}

			var (chain1, chain2) = (this[kvp1], other[kvp2]);
			if (!chain1.Equals(chain2, nodeComparison, patternComparison))
			{
				return false;
			}
		}
		return true;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int CompareTo(BlossomLoop? other) => CompareTo(other, NodeComparison.IgnoreIsOn);

	/// <summary>
	/// Compares the value with the other one, to get which one is greater.
	/// </summary>
	/// <param name="other">The other instance to be compared.</param>
	/// <param name="nodeComparison">The node comparison rule.</param>
	/// <returns>An <see cref="int"/> value indicating which instance is better.</returns>
	public int CompareTo(BlossomLoop? other, NodeComparison nodeComparison)
	{
		if (other is null)
		{
			return -1;
		}

		if (Count.CompareTo(other.Count) is var r1 and not 0)
		{
			return r1;
		}

		using var e1 = Keys.GetEnumerator();
		using var e2 = other.Keys.GetEnumerator();
		while (e1.MoveNext() && e2.MoveNext())
		{
			if (e1.Current.CompareTo(e2.Current) is var r2 and not 0)
			{
				return r2;
			}
		}

		foreach (var key in Keys)
		{
			switch (this[key], other[key])
			{
				case var (c, d) when c.CompareTo(d, nodeComparison) is var r3 and not 0:
				{
					return r3;
				}
			}
		}
		return 0;
	}

	/// <inheritdoc/>
	public override int GetHashCode() => GetHashCode(NodeComparison.IgnoreIsOn, ChainComparison.Undirected);

	/// <summary>
	/// Calculates a hash code value used for comparison.
	/// </summary>
	/// <param name="nodeComparison">The node comparison rule.</param>
	/// <param name="patternComparison">The chain comparison rule.</param>
	/// <returns>Hash code value.</returns>
	public int GetHashCode(NodeComparison nodeComparison, ChainComparison patternComparison)
	{
		var hashCode = default(HashCode);
		foreach (var (branchStartNode, forcingChain) in this)
		{
			hashCode.Add(branchStartNode);
			hashCode.Add(forcingChain.GetHashCode(nodeComparison, patternComparison));
		}
		return hashCode.ToHashCode();
	}

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToString(IFormatProvider? formatProvider)
	{
		var converter = CoordinateConverter.GetInstance(formatProvider);
		return string.Join(
			", ",
			from kvp in this
			let candidate = kvp.Key
			let chain = kvp.Value
			select $"{converter.CandidateConverter(candidate.AsCandidateMap())}: {chain.ToString(converter)}"
		);
	}

	/// <inheritdoc/>
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(formatProvider);
}
