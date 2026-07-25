namespace Sudoku.Theories.SetTheory;

/// <summary>
/// Represents rank result for a pattern.
/// </summary>
[DebuggerDisplay($$"""{{{nameof(ToString)}}(),nq}""")]
[Union]
public readonly struct Rank : IEquatable<Rank>, IEqualityOperators<Rank, Rank, bool>, IUnion
{
	/// <summary>
	/// Represents illegal logic rank.
	/// </summary>
	public static readonly Rank Illegal = -1;


	/// <summary>
	/// Represents rank value for consistent cases.
	/// </summary>
	private readonly int? _consistentRank;

	/// <summary>
	/// Represents rank values for inconsistent cases.
	/// </summary>
	private readonly int[]? _inconsistentRank;


	/// <summary>
	/// Initializes a <see cref="Rank"/> for a rank value.
	/// </summary>
	/// <param name="value">A rank value.</param>
	public Rank(int value)
	{
		IsConsistent = true;
		_consistentRank = value;
	}

	/// <summary>
	/// Initializes a <see cref="Rank"/> for a sequence of rank values.
	/// </summary>
	/// <param name="values">A sequence of rank values.</param>
	public Rank(int[] values)
	{
		IsConsistent = false;
		_inconsistentRank = values;
	}


	/// <inheritdoc/>
	public object? Value => IsConsistent ? _consistentRank.Value : _inconsistentRank;

	/// <summary>
	/// Represents a flag, meaning whether the rank is consistent or not.
	/// </summary>
	[MemberNotNullWhen(true, nameof(_consistentRank))]
	[MemberNotNullWhen(false, nameof(_inconsistentRank), nameof(InconsistentRanksOrdered))]
	public bool IsConsistent { get; }

	/// <summary>
	/// Represents ordered collection of inconsistent rank values.
	/// </summary>
	private SortedSet<int>? InconsistentRanksOrdered => _inconsistentRank is null ? null : new(_inconsistentRank);


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is Rank comparer && Equals(comparer);

	/// <inheritdoc/>
	public bool Equals(Rank other)
		=> IsConsistent == other.IsConsistent
		&& (
			IsConsistent
				? _consistentRank == other._consistentRank
				: InconsistentRanksOrdered.SetEquals(other.InconsistentRanksOrdered!)
		);

	/// <summary>
	/// Check whether the current type holds consistent rank or not.
	/// </summary>
	/// <param name="value">The target rank value.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public bool TryGetValue(out int value)
	{
		if (IsConsistent)
		{
			value = _consistentRank.Value;
			return true;
		}
		value = default;
		return false;
	}

	/// <summary>
	/// Check whether the current type holds inconsistent rank or not.
	/// </summary>
	/// <param name="value">The target rank value.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public bool TryGetValue([NotNullWhen(true)] out int[]? value)
	{
		if (IsConsistent)
		{
			value = default;
			return false;
		}
		value = _inconsistentRank[..];
		return true;
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		if (IsConsistent)
		{
			return _consistentRank.Value;
		}
		else
		{
			var hashCode = new HashCode();
			foreach (var value in InconsistentRanksOrdered)
			{
				hashCode.Add(value);
			}
			return hashCode.ToHashCode();
		}
	}

	/// <summary>
	/// Converts the current instance into an array.
	/// </summary>
	/// <returns>An array of values.</returns>
	public int[] ToArray() => IsConsistent ? [_consistentRank.Value] : [.. _inconsistentRank];

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString()
		=> IsConsistent ? _consistentRank.Value.ToString() : $"[{string.Join(", ", InconsistentRanksOrdered)}]";


	/// <inheritdoc/>
	public static bool operator ==(Rank left, Rank right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(Rank left, Rank right) => !(left == right);
}
