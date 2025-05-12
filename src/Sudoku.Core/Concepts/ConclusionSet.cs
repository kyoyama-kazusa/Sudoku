namespace Sudoku.Concepts;

/// <summary>
/// Represents a list of conclusions.
/// </summary>
[TypeImpl(
	TypeImplFlags.Object_Equals | TypeImplFlags.Object_ToString
		| TypeImplFlags.EqualityOperators | TypeImplFlags.TrueAndFalseOperators
		| TypeImplFlags.LogicalNotOperator | TypeImplFlags.Equatable)]
public sealed partial class ConclusionSet :
	IAnyAllMethod<ConclusionSet, Conclusion>,
	IContainsMethod<ConclusionSet, Conclusion>,
	IEquatable<ConclusionSet>,
	IEqualityOperators<ConclusionSet, ConclusionSet, bool>,
	IFiniteSet<ConclusionSet, Conclusion>,
	IFormattable,
	IInfiniteSet<ConclusionSet, Conclusion>,
	ILogicalOperators<ConclusionSet>,
	IParsable<ConclusionSet>,
	IReadOnlySet<Conclusion>,
	ISliceMethod<ConclusionSet, Conclusion>,
	IToArrayMethod<ConclusionSet, Conclusion>
{
	/// <summary>
	/// The total length of bits.
	/// </summary>
	private const int BitsCount = HalfBitsCount << 1;

	/// <summary>
	/// The maximum number of candidates can exist in a grid.
	/// </summary>
	private const int HalfBitsCount = 729;

	/// <summary>
	/// Indicates the total length of bit array.
	/// </summary>
	private const int Length = 45;


	/// <summary>
	/// The prime numbers below 100.
	/// </summary>
	private static readonly int[] PrimeNumbers = [2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97];


	/// <summary>
	/// The internal bit array.
	/// </summary>
	[EquatableMember]
	private readonly BitArray _bitArray = new(BitsCount);


	/// <summary>
	/// Indicates whether the collection contains any assignment conclusions.
	/// </summary>
	public bool ContainsAssignment
	{
		get
		{
			for (var i = 0; i < HalfBitsCount; i++)
			{
				if (_bitArray[i])
				{
					return true;
				}
			}
			return false;
		}
	}

	/// <summary>
	/// Indicates whether the collection contains any elimination conclusions.
	/// </summary>
	public bool ContainsElimination
	{
		get
		{
			for (var i = HalfBitsCount; i < BitsCount; i++)
			{
				if (_bitArray[i])
				{
					return true;
				}
			}
			return false;
		}
	}

	/// <summary>
	/// Indicates the number of bit array elements.
	/// </summary>
	public int Count
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _bitArray.Cardinality;
	}

	/// <inheritdoc/>
	bool ICollection<Conclusion>.IsReadOnly => false;

	/// <inheritdoc/>
	int ICollection<Conclusion>.Count => Count;


	/// <summary>
	/// An empty instance.
	/// </summary>
	public static ConclusionSet Empty => [];


	/// <summary>
	/// Try to get n-th element stored in the collection.
	/// </summary>
	/// <param name="index">The desired index to be checked.</param>
	/// <returns>The found <see cref="Conclusion"/> instance at the specified index.</returns>
	/// <exception cref="IndexOutOfRangeException">Throws when the index is out of range.</exception>
	public Conclusion this[int index]
	{
		get
		{
			if (index < 0 || index >= Count)
			{
				throw new IndexOutOfRangeException();
			}

			var bmi2IsSupported = Bmi2.IsSupported;
			var popCountSum = 0;
			var internalField = _bitArray.GetInternalArrayField();
			for (var i = 0; i < Length; i++)
			{
				var bits = (uint)internalField[i];
				var z = bmi2IsSupported
					? BitOperations.TrailingZeroCount(Bmi2.ParallelBitDeposit(1U << index - popCountSum, bits))
					: bits.SetAt(index - popCountSum);
				switch (bmi2IsSupported)
				{
					case true when z != FallbackConstants.@int:
					case false when z != -1:
					{
						return new((short)(z + (i << 5))); // * 32
					}
				}

				popCountSum += BitOperations.PopCount(bits);
			}
			return default;
		}
	}


	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(Conclusion item)
	{
		var (type, cell, digit) = item;
		_bitArray[(int)type * HalfBitsCount + cell * 9 + digit] = true;
	}

	/// <summary>
	/// Add a list of conclusions into the collection.
	/// </summary>
	/// <param name="conclusions">The conclusions to be added.</param>
	public void AddRange(params ReadOnlySpan<Conclusion> conclusions)
	{
		foreach (var conclusion in conclusions)
		{
			Add(conclusion);
		}
	}

	/// <summary>
	/// Remove a conclusion, represented as a global index (between 0 and 1458), from the collection.
	/// </summary>
	/// <param name="item">The item to be removed.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Remove(Conclusion item)
	{
		var (type, cell, digit) = item;
		_bitArray[(int)type * HalfBitsCount + cell * 9 + digit] = false;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Clear() => _bitArray.SetAll(false);

	/// <summary>
	/// Removes all elements in the collection and add all elements from <paramref name="conclusions"/>.
	/// </summary>
	/// <param name="conclusions">The conclusions provider to replace with the current instance.</param>
	public void Replace(ConclusionSet conclusions)
	{
		Clear();
		foreach (var conclusion in conclusions)
		{
			Add(conclusion);
		}
	}

	/// <inheritdoc cref="ICollection{T}.CopyTo(T[], int)"/>
	public void CopyTo(Span<Conclusion> span)
	{
		if (span.Length < Count)
		{
			throw new InvalidOperationException();
		}

		var i = 0;
		foreach (var conclusion in this)
		{
			span[i++] = conclusion;
		}
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Contains(Conclusion value) => _bitArray[value.GetHashCode()];

	/// <summary>
	/// Indicates whether the collection contains the specified cell.
	/// </summary>
	/// <param name="cell">The cell to be checked.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool ContainsCell(Cell cell)
	{
		for (var bit = 0; bit < 9; bit++)
		{
			if (_bitArray[cell * 9 + bit] || _bitArray[HalfBitsCount + cell * 9 + bit])
			{
				return true;
			}
		}
		return false;
	}

	/// <inheritdoc cref="IAnyAllMethod{TSelf, TSource}.Any(Func{TSource, bool})"/>
	public bool Exists(Func<Conclusion, bool> predicate)
	{
		foreach (var conclusion in this)
		{
			if (predicate(conclusion))
			{
				return true;
			}
		}
		return false;
	}

	/// <inheritdoc cref="IAnyAllMethod{TSelf, TSource}.All(Func{TSource, bool})"/>
	public bool TrueForAll(Func<Conclusion, bool> predicate)
	{
		foreach (var conclusion in this)
		{
			if (!predicate(conclusion))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Determine whether the conclusion set contains valid conclusions that can be applied to grid.
	/// </summary>
	/// <param name="grid">The grid to be checked.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool IsWorthFor(in Grid grid)
	{
		foreach (var element in this)
		{
			if (grid.Exists(element.Candidate) is true)
			{
				return true;
			}
		}
		return false;
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		var (result, i) = (default(HashCode), 0);
		foreach (bool element in _bitArray)
		{
			if (element)
			{
				result.Add(PrimeNumbers[i % PrimeNumbers.Length] * i);
			}
			i++;
		}
		return result.ToHashCode();
	}

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToString(IFormatProvider? formatProvider)
		=> CoordinateConverter.GetInstance(formatProvider).ConclusionConverter(ToArray());

	/// <inheritdoc/>
	public Conclusion[] ToArray()
	{
		var result = new Conclusion[Count];
		for (var (i, z) = ((short)0, 0); i < BitsCount; i++)
		{
			if (_bitArray[i])
			{
				result[z++] = new(i >= HalfBitsCount ? Elimination : Assignment, i % HalfBitsCount);
			}
		}
		return result;
	}

	/// <summary>
	/// Converts the current collection into a <see cref="ReadOnlySpan{T}"/> instance.
	/// </summary>
	/// <returns>A <see cref="ReadOnlySpan{T}"/> of <see cref="Conclusion"/> instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ReadOnlySpan<Conclusion> AsSpan() => ToArray();

	/// <summary>
	/// Try to get an enumerator type that iterates on each conclusion.
	/// </summary>
	/// <returns>An enumerator type that iterates on each conclusion.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AnonymousSpanEnumerator<Conclusion> GetEnumerator() => new(ToArray());

	/// <inheritdoc cref="ISliceMethod{TSelf, TSource}.Slice(int, int)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ConclusionSet Slice(int start, int count) => [.. ToArray().AsReadOnlySpan()[start..(start + count)]];

	/// <inheritdoc/>
	void ICollection<Conclusion>.CopyTo(Conclusion[] array, int arrayIndex) => CopyTo(array.AsSpan()[arrayIndex..]);

	/// <inheritdoc/>
	void ISet<Conclusion>.ExceptWith(IEnumerable<Conclusion> other)
	{
		foreach (var element in other)
		{
			if (Contains(element))
			{
				Remove(element);
			}
		}
	}

	/// <inheritdoc/>
	void ISet<Conclusion>.IntersectWith(IEnumerable<Conclusion> other)
	{
		foreach (var element in other)
		{
			if (!Contains(element))
			{
				Remove(element);
			}
		}
	}

	/// <inheritdoc/>
	void ISet<Conclusion>.SymmetricExceptWith(IEnumerable<Conclusion> other)
	{
		ConclusionSet p = [.. other];
		Replace(this & ~p | p & ~this);
	}

	/// <inheritdoc/>
	void ISet<Conclusion>.UnionWith(IEnumerable<Conclusion> other)
	{
		foreach (var element in other)
		{
			if (!Contains(element))
			{
				Add(element);
			}
		}
	}

	/// <inheritdoc/>
	bool IAnyAllMethod<ConclusionSet, Conclusion>.Any() => Count != 0;

	/// <inheritdoc/>
	bool IAnyAllMethod<ConclusionSet, Conclusion>.Any(Func<Conclusion, bool> predicate) => Exists(predicate);

	/// <inheritdoc/>
	bool IAnyAllMethod<ConclusionSet, Conclusion>.All(Func<Conclusion, bool> predicate) => TrueForAll(predicate);

	/// <inheritdoc/>
	bool ICollection<Conclusion>.Remove(Conclusion item)
	{
		if (!Contains(item))
		{
			return false;
		}

		Remove(item);
		return true;
	}

	/// <inheritdoc/>
	bool IReadOnlySet<Conclusion>.IsProperSubsetOf(IEnumerable<Conclusion> other)
	{
		ConclusionSet p = [.. other];
		return (p & this) == this && p != this;
	}

	/// <inheritdoc/>
	bool IReadOnlySet<Conclusion>.IsProperSupersetOf(IEnumerable<Conclusion> other)
	{
		ConclusionSet p = [.. other];
		return (this & p) == p && p != this;
	}

	/// <inheritdoc/>
	bool IReadOnlySet<Conclusion>.IsSubsetOf(IEnumerable<Conclusion> other) => ([.. other] & this) == this;

	/// <inheritdoc/>
	bool IReadOnlySet<Conclusion>.IsSupersetOf(IEnumerable<Conclusion> other)
	{
		ConclusionSet p = [.. other];
		return (this & p) == p;
	}

	/// <inheritdoc/>
	bool IReadOnlySet<Conclusion>.Overlaps(IEnumerable<Conclusion> other) => this & [.. other] ? true : false;

	/// <inheritdoc/>
	bool IReadOnlySet<Conclusion>.SetEquals(IEnumerable<Conclusion> other) => this == [.. other];

	/// <inheritdoc/>
	bool ISet<Conclusion>.Add(Conclusion item)
	{
		if (Contains(item))
		{
			return false;
		}

		Add(item);
		return true;
	}

	/// <inheritdoc/>
	bool ISet<Conclusion>.IsProperSubsetOf(IEnumerable<Conclusion> other) => ((IReadOnlySet<Conclusion>)this).IsProperSubsetOf(other);

	/// <inheritdoc/>
	bool ISet<Conclusion>.IsProperSupersetOf(IEnumerable<Conclusion> other) => ((IReadOnlySet<Conclusion>)this).IsProperSupersetOf(other);

	/// <inheritdoc/>
	bool ISet<Conclusion>.IsSubsetOf(IEnumerable<Conclusion> other) => ((IReadOnlySet<Conclusion>)this).IsSubsetOf(other);

	/// <inheritdoc/>
	bool ISet<Conclusion>.IsSupersetOf(IEnumerable<Conclusion> other) => ((IReadOnlySet<Conclusion>)this).IsSupersetOf(other);

	/// <inheritdoc/>
	bool ISet<Conclusion>.Overlaps(IEnumerable<Conclusion> other) => ((IReadOnlySet<Conclusion>)this).Overlaps(other);

	/// <inheritdoc/>
	bool ISet<Conclusion>.SetEquals(IEnumerable<Conclusion> other) => ((IReadOnlySet<Conclusion>)this).SetEquals(other);

	/// <inheritdoc/>
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(formatProvider);

	/// <inheritdoc/>
	ConclusionSet IFiniteSet<ConclusionSet, Conclusion>.Negate() => ~this;

	/// <inheritdoc/>
	ConclusionSet IInfiniteSet<ConclusionSet, Conclusion>.ExceptWith(ConclusionSet other) => this & ~other;

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => ToArray().GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<Conclusion> IEnumerable<Conclusion>.GetEnumerator() => ToArray().AsEnumerable().GetEnumerator();

	/// <inheritdoc/>
	IEnumerable<Conclusion> ISliceMethod<ConclusionSet, Conclusion>.Slice(int start, int count) => Slice(start, count);


	/// <inheritdoc cref="IParsable{TSelf}.TryParse(string?, IFormatProvider?, out TSelf)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryParse(string str, [NotNullWhen(true)] out ConclusionSet? result) => TryParse(str, null, out result);

	/// <inheritdoc/>
	public static bool TryParse(string? s, IFormatProvider? provider, [NotNullWhen(true)] out ConclusionSet? result)
	{
		try
		{
			if (s is null)
			{
				throw new FormatException();
			}

			result = Parse(s, null);
			return true;
		}
		catch (FormatException)
		{
			result = null;
			return false;
		}
	}

	/// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ConclusionSet Parse(string str) => Parse(str, null);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ConclusionSet Parse(string s, IFormatProvider? provider)
		=> CoordinateParser.GetInstance(provider).ConclusionParser(s);


	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ConclusionSet operator ~(ConclusionSet value)
	{
		var result = value[..];
		result._bitArray.Not();
		return result;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ConclusionSet operator &(ConclusionSet left, ConclusionSet right)
	{
		var result = left[..];
		result._bitArray.And(right._bitArray);
		return result;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ConclusionSet operator |(ConclusionSet left, ConclusionSet right)
	{
		var result = left[..];
		result._bitArray.Or(right._bitArray);
		return result;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ConclusionSet operator ^(ConclusionSet left, ConclusionSet right)
	{
		var result = left[..];
		result._bitArray.Xor(right._bitArray);
		return result;
	}
}
