namespace Sudoku.Concepts;

/// <summary>
/// Represents a set of <see cref="Segment"/> instances.
/// </summary>
/// <seealso cref="Segment"/>
public partial struct SegmentCollection :
	ILogicalOperators<SegmentCollection>,
	IReadOnlyList<Segment>,
	IReadOnlySet<Segment>,
	ISet<Segment>,
	IEquatable<SegmentCollection>,
	IEqualityOperators<SegmentCollection, SegmentCollection, bool>
{
	/// <summary>
	/// Indicates the full mask.
	/// </summary>
	private const long FullMask = (1L << 54) - 1;


	/// <summary>
	/// Indicates the empty instance.
	/// </summary>
	public static readonly SegmentCollection Empty;

	/// <summary>
	/// Indicates the instance that contains all possible values.
	/// </summary>
	public static readonly SegmentCollection Full;


	/// <summary>
	/// Indicates the backing mask. The mask holds 54 of 64 bits indicating numbered segments.
	/// </summary>
	private long _mask;


	/// <include file="../../global-doc-comments.xml" path="g/static-constructor"/>
	static SegmentCollection()
	{
		Empty = default;
		Full._mask = FullMask;
	}


	/// <summary>
	/// Indicates the number of items stored in this collection.
	/// </summary>
	public readonly int Count => PopCount((ulong)_mask);

	/// <inheritdoc/>
	readonly bool ICollection<Segment>.IsReadOnly => false;


	/// <summary>
	/// Gets the item at the specified index.
	/// </summary>
	/// <param name="index">The desired index.</param>
	/// <returns>The segment instance at the specified index.</returns>
	public readonly Segment this[int index]
	{
		get
		{
			var targetIndex = _mask[index];
			return targetIndex == -1 ? throw new IndexOutOfRangeException() : (Segment)targetIndex;
		}
	}


	/// <inheritdoc/>
	public readonly override bool Equals([NotNullWhen(true)] object? obj) => obj is SegmentCollection comparer && Equals(comparer);

	/// <inheritdoc/>
	public readonly bool Equals(SegmentCollection other) => _mask == other._mask;

	/// <summary>
	/// Determine whether the specified value is in the collection or not.
	/// </summary>
	/// <param name="value">The value to check.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public readonly bool Contains(Segment value) => (_mask >> (int)value & 1) != 0;

	/// <inheritdoc cref="object.GetHashCode"/>
	public readonly override int GetHashCode() => _mask.GetHashCode();

	/// <inheritdoc cref="object.ToString"/>
	public readonly override string ToString() => ToString(CoordinateConverter.InvariantCulture);

	/// <summary>
	/// Converts the current instance into <see cref="string"/> representation, via the specified converter.
	/// </summary>
	/// <param name="converter">The converter.</param>
	/// <returns>The string.</returns>
	public readonly string ToString(CoordinateConverter converter) => converter.SegmentConverter(this);

	/// <summary>
	/// Converts the current instance into <see cref="string"/> representation, via the specified culture.
	/// </summary>
	/// <param name="culture">The culture.</param>
	/// <returns>The string.</returns>
	public readonly string ToString(CultureInfo culture) => ToString(CoordinateConverter.GetInstance(culture));

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public readonly Enumerator GetEnumerator() => new(in this);

	/// <summary>
	/// Clears the whole collection.
	/// </summary>
	public void Clear() => _mask = 0;

	/// <summary>
	/// Adds a new item into the current collection, and return a <see cref="bool"/> indicating
	/// whether the value dpesn't exist in the collection or not.
	/// </summary>
	/// <param name="value">The value to add.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public bool Add(Segment value)
	{
		var order = (int)value;
		if ((_mask >> order & 1) != 0)
		{
			return false;
		}
		_mask |= 1L << order;
		return true;
	}

	/// <summary>
	/// Removes the specified item out of the collection, and return a <see cref="bool"/> result indicating
	/// whether the value exists in the collection or not.
	/// </summary>
	/// <param name="value">The value to remove.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public bool Remove(Segment value)
	{
		var order = (int)value;
		if ((_mask >> order & 1) == 0)
		{
			return false;
		}
		_mask &= ~(1L << order);
		return true;
	}

	/// <inheritdoc/>
	readonly void ICollection<Segment>.CopyTo(Segment[] array, int arrayIndex)
	{
		ArgumentException.Assert(array.Length >= Count);
		var i = 0;
		foreach (var segment in this)
		{
			array[arrayIndex + i++] = segment;
		}
	}

	/// <inheritdoc/>
	readonly bool IReadOnlySet<Segment>.IsProperSubsetOf(IEnumerable<Segment> other)
	{
		SegmentCollection o = [.. other];
		return (o & this) == this && o != this;
	}

	/// <inheritdoc/>
	readonly bool IReadOnlySet<Segment>.IsProperSupersetOf(IEnumerable<Segment> other)
	{
		SegmentCollection o = [.. other];
		return (this & o) == o && o != this;
	}

	/// <inheritdoc/>
	readonly bool IReadOnlySet<Segment>.IsSubsetOf(IEnumerable<Segment> other)
	{
		SegmentCollection o = [.. other];
		return (o & this) == this;
	}

	/// <inheritdoc/>
	readonly bool IReadOnlySet<Segment>.IsSupersetOf(IEnumerable<Segment> other)
	{
		SegmentCollection o = [.. other];
		return (this & o) == o;
	}

	/// <inheritdoc/>
	readonly bool IReadOnlySet<Segment>.Overlaps(IEnumerable<Segment> other)
	{
		SegmentCollection o = [.. other];
		return (this & o).Count != 0;
	}

	/// <inheritdoc/>
	readonly bool IReadOnlySet<Segment>.SetEquals(IEnumerable<Segment> other)
	{
		SegmentCollection o = [.. other];
		return this == o;
	}

	/// <inheritdoc/>
	readonly bool ISet<Segment>.Overlaps(IEnumerable<Segment> other) => ((IReadOnlySet<Segment>)this).Overlaps(other);

	/// <inheritdoc/>
	readonly bool ISet<Segment>.SetEquals(IEnumerable<Segment> other) => ((IReadOnlySet<Segment>)this).SetEquals(other);

	/// <inheritdoc/>
	readonly bool ISet<Segment>.IsProperSubsetOf(IEnumerable<Segment> other)
		=> ((IReadOnlySet<Segment>)this).IsProperSubsetOf(other);

	/// <inheritdoc/>
	readonly bool ISet<Segment>.IsProperSupersetOf(IEnumerable<Segment> other)
		=> ((IReadOnlySet<Segment>)this).IsProperSupersetOf(other);

	/// <inheritdoc/>
	readonly bool ISet<Segment>.IsSubsetOf(IEnumerable<Segment> other) => ((IReadOnlySet<Segment>)this).IsSubsetOf(other);

	/// <inheritdoc/>
	readonly bool ISet<Segment>.IsSupersetOf(IEnumerable<Segment> other) => ((IReadOnlySet<Segment>)this).IsSupersetOf(other);

	/// <inheritdoc/>
	readonly IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<Segment>)this).GetEnumerator();

	/// <inheritdoc/>
	readonly IEnumerator<Segment> IEnumerable<Segment>.GetEnumerator()
	{
		for (var i = 0; i < 54; i++)
		{
			if ((_mask >> i & 1) != 0)
			{
				yield return (Segment)i;
			}
		}
	}

	/// <inheritdoc/>
	void ICollection<Segment>.Add(Segment value) => this += value;

	/// <inheritdoc/>
	bool ICollection<Segment>.Remove(Segment value) => Remove(value);

	/// <inheritdoc/>
	void ISet<Segment>.ExceptWith(IEnumerable<Segment> other)
	{
		SegmentCollection o = [.. other];
		this &= ~o;
	}

	/// <inheritdoc/>
	void ISet<Segment>.IntersectWith(IEnumerable<Segment> other)
	{
		SegmentCollection o = [.. other];
		this &= o;
	}

	/// <inheritdoc/>
	void ISet<Segment>.SymmetricExceptWith(IEnumerable<Segment> other)
	{
		SegmentCollection o = [.. other];
		this = this & ~o | o & ~this;
	}

	/// <inheritdoc/>
	void ISet<Segment>.UnionWith(IEnumerable<Segment> other)
	{
		SegmentCollection o = [.. other];
		this |= o;
	}


	/// <inheritdoc cref="TryParse(string?, CoordinateParser, out SegmentCollection)"/>
	public static bool TryParse([NotNullWhen(true)] string? s, out SegmentCollection result)
		=> TryParse(s, CoordinateParser.InvariantCulture, out result);

	/// <summary>
	/// Try to parse the specified string, converting it into target instance via the specified converter.
	/// </summary>
	/// <param name="s">The string to parse.</param>
	/// <param name="converter">The converter.</param>
	/// <param name="result">The result.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool TryParse([NotNullWhen(true)] string? s, CoordinateParser converter, out SegmentCollection result)
	{
		try
		{
			if (s is null)
			{
				goto ReturnFalse;
			}
			result = Parse(s, converter);
			return true;
		}
		catch (FormatException)
		{
		}

	ReturnFalse:
		result = default;
		return false;
	}

	/// <summary>
	/// Try to parse the specified string, converting it into target instance via the specified culture.
	/// </summary>
	/// <param name="s">The string to parse.</param>
	/// <param name="culture">The culture.</param>
	/// <param name="result">The result.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool TryParse([NotNullWhen(true)] string? s, CultureInfo culture, out SegmentCollection result)
		=> TryParse(s, CoordinateParser.GetInstance(culture), out result);

	/// <inheritdoc cref="Parse(string, CoordinateParser)"/>
	public static SegmentCollection Parse(string s) => Parse(s, CoordinateParser.InvariantCulture);

	/// <summary>
	/// Parses the specified string, converting it into target instance via the specified converter.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="converter">The converter.</param>
	/// <returns>The result.</returns>
	/// <exception cref="FormatException">Throws when invalid characters encountered.</exception>
	public static SegmentCollection Parse(string s, CoordinateParser converter) => converter.SegmentParser(s);

	/// <summary>
	/// Parses the specified string, converting it into target instance via the specified culture.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="culture">The culture.</param>
	/// <returns>The result.</returns>
	/// <exception cref="FormatException">Throws when invalid characters encountered.</exception>
	public static SegmentCollection Parse(string s, CultureInfo culture) => Parse(s, CoordinateParser.GetInstance(culture));


	/// <summary>
	/// Adds the value into the current collection.
	/// </summary>
	/// <param name="value">The value.</param>
	public void operator +=(Segment value) => _mask |= 1L << (int)value;

	/// <summary>
	/// Removes the value out of the current collection.
	/// </summary>
	/// <param name="value">The value.</param>
	public void operator -=(Segment value) => _mask &= ~(1L << (int)value);

	/// <summary>
	/// Performs compound assignment operation of <see langword="operator"/> &amp;.
	/// </summary>
	/// <param name="value">The value.</param>
	public void operator &=(SegmentCollection value) => _mask &= value._mask;

	/// <summary>
	/// Performs compound assignment operation of <see langword="operator"/> |.
	/// </summary>
	/// <param name="value">The value.</param>
	public void operator |=(SegmentCollection value) => _mask |= value._mask;

	/// <summary>
	/// Performs compound assignment operation of <see langword="operator"/> ^.
	/// </summary>
	/// <param name="value">The value.</param>
	public void operator ^=(SegmentCollection value) => _mask ^= value._mask;


	/// <inheritdoc/>
	public static bool operator !(SegmentCollection value) => value.Count == 0;

	/// <inheritdoc/>
	public static bool operator true(SegmentCollection value) => value.Count != 0;

	/// <inheritdoc/>
	public static bool operator false(SegmentCollection value) => value.Count == 0;

	/// <inheritdoc/>
	public static bool operator ==(SegmentCollection left, SegmentCollection right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(SegmentCollection left, SegmentCollection right) => !(left == right);

	/// <inheritdoc/>
	public static SegmentCollection operator ~(SegmentCollection value)
	{
		SegmentCollection result;
		result._mask = FullMask & ~value._mask;
		return result;
	}

	/// <inheritdoc/>
	public static SegmentCollection operator &(SegmentCollection left, SegmentCollection right)
	{
		SegmentCollection result;
		result._mask = left._mask & right._mask;
		return result;
	}

	/// <inheritdoc/>
	public static SegmentCollection operator |(SegmentCollection left, SegmentCollection right)
	{
		SegmentCollection result;
		result._mask = left._mask | right._mask;
		return result;
	}

	/// <inheritdoc/>
	public static SegmentCollection operator ^(SegmentCollection left, SegmentCollection right)
	{
		SegmentCollection result;
		result._mask = left._mask ^ right._mask;
		return result;
	}
}
