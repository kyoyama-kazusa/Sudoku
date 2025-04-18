namespace Sudoku.Concepts.Supersymmetry;

/// <summary>
/// Represents a set of <see cref="Space"/> instances.
/// </summary>
/// <seealso cref="Space"/>
[TypeImpl(
	TypeImplFlags.Object_Equals | TypeImplFlags.Equatable
		| TypeImplFlags.EqualityOperators | TypeImplFlags.TrueAndFalseOperators | TypeImplFlags.LogicalNotOperator,
	IsLargeStructure = true)]
public partial struct SpaceSet :
	IAdditionOperators<SpaceSet, Space, SpaceSet>,
	IBitwiseOperators<SpaceSet, SpaceSet, SpaceSet>,
	ICollection<Space>,
	IEnumerable<Space>,
	IEquatable<SpaceSet>,
	IEqualityOperators<SpaceSet, SpaceSet, bool>,
	IFiniteSet<SpaceSet, Space>,
	IInfiniteSet<SpaceSet, Space>,
	ILogicalOperators<SpaceSet>,
	IReadOnlyList<Space>,
	IReadOnlySet<Space>,
	ISet<Space>,
	ISubtractionOperators<SpaceSet, Space, SpaceSet>
{
	/// <summary>
	/// Indicates the empty instance.
	/// </summary>
	public static readonly SpaceSet Empty;


	/// <summary>
	/// Indicates the buffer entry field.
	/// </summary>
	[EquatableMember]
	private BackingBuffer _field;


	/// <inheritdoc/>
	public readonly int Count => _field[0].Count + _field[1].Count + _field[2].Count + _field[3].Count;

	/// <inheritdoc/>
	readonly bool ICollection<Space>.IsReadOnly => false;


	/// <inheritdoc/>
	readonly Space IReadOnlyList<Space>.this[int index] => ToArray()[index];


	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly void CopyTo(Space[] array, int arrayIndex)
	{
		ArgumentException.ThrowIfAssertionFailed(array.Length >= Count);

		ToArray().AsReadOnlySpan().CopyTo(array.AsSpan()[arrayIndex..]);
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool Contains(Space space)
	{
		var (type, primary, secondary) = space;
		var id = primary * 9 + secondary;
		return _field[(int)type].Contains(id);
	}

	/// <inheritdoc/>
	public override readonly int GetHashCode() => HashCode.Combine(_field[0], _field[1], _field[2], _field[3]);

	/// <summary>
	/// Returns a <see cref="BitmapEnumerator"/> instance that can iterate on each bit state.
	/// </summary>
	/// <returns>A <see cref="BitmapEnumerator"/> instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[UnscopedRef]
	public readonly BitmapEnumerator EnumerateBitStates() => new(in this);

	/// <summary>
	/// Converts the current object into an array of <see cref="Space"/> instances.
	/// </summary>
	/// <returns>An array of <see cref="Space"/> instances.</returns>
	public readonly Space[] ToArray()
		=> [
			.. from id in _field[0] select Space.RowColumn(id / 9, id % 9),
			.. from id in _field[1] select Space.BlockNumber(id / 9, id % 9),
			.. from id in _field[2] select Space.RowNumber(id / 9, id % 9),
			.. from id in _field[3] select Space.ColumnNumber(id / 9, id % 9)
		];

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Clear() => _field[0] = _field[1] = _field[2] = _field[3] = CellMap.Empty;

	/// <summary>
	/// Adds a new space into the collection.
	/// </summary>
	/// <param name="space">The space.</param>
	/// <returns>A <see cref="bool"/> result indicating whether the adding operation is success.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Add(Space space)
	{
		var (type, primary, secondary) = space;
		var id = primary * 9 + secondary;
		if (_field[(int)type].Contains(id))
		{
			return false;
		}

		_field[(int)type].Add(id);
		return true;
	}

	/// <summary>
	/// Removes a space from the current collection.
	/// </summary>
	/// <param name="space">The space.</param>
	/// <returns>A <see cref="bool"/> result indicating whether the removing operation is success.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Remove(Space space)
	{
		var (type, primary, secondary) = space;
		var id = primary * 9 + secondary;
		if (!_field[(int)type].Contains(id))
		{
			return false;
		}

		_field[(int)type].Remove(id);
		return true;
	}

	/// <summary>
	/// Adds a list of new spaces into the collection.
	/// </summary>
	/// <param name="spaces">The spaces.</param>
	/// <returns>An <see cref="int"/> value indicating how many spaces adding successfully.</returns>
	public int AddRange(params ReadOnlySpan<Space> spaces)
	{
		var result = 0;
		foreach (var space in spaces)
		{
			result += Add(space) ? 1 : 0;
		}
		return result;
	}

	/// <inheritdoc cref="object.ToString"/>
	public override readonly string ToString()
	{
		var values = ToArray();
		return string.Join(' ', from element in values select element.ToString());
	}

	/// <inheritdoc/>
	readonly bool IReadOnlySet<Space>.IsProperSubsetOf(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		return (otherSet & this) == this && this != otherSet;
	}

	/// <inheritdoc/>
	readonly bool IReadOnlySet<Space>.IsProperSupersetOf(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		return (this & otherSet) == otherSet && this != otherSet;
	}

	/// <inheritdoc/>
	readonly bool IReadOnlySet<Space>.IsSubsetOf(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		return (otherSet & this) == this;
	}

	/// <inheritdoc/>
	readonly bool IReadOnlySet<Space>.IsSupersetOf(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		return (this & otherSet) == otherSet;
	}

	/// <inheritdoc/>
	readonly bool IReadOnlySet<Space>.Overlaps(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		return !!(this & otherSet);
	}

	/// <inheritdoc/>
	readonly bool IReadOnlySet<Space>.SetEquals(IEnumerable<Space> other) => this == [.. other];

	/// <inheritdoc/>
	readonly bool ISet<Space>.IsProperSubsetOf(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		return (otherSet & this) == this && this != otherSet;
	}

	/// <inheritdoc/>
	readonly bool ISet<Space>.IsProperSupersetOf(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		return (this & otherSet) == otherSet && this != otherSet;
	}

	/// <inheritdoc/>
	readonly bool ISet<Space>.IsSubsetOf(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		return (otherSet & this) == this;
	}

	/// <inheritdoc/>
	readonly bool ISet<Space>.IsSupersetOf(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		return (this & otherSet) == otherSet;
	}

	/// <inheritdoc/>
	readonly bool ISet<Space>.Overlaps(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		return !!(this & otherSet);
	}

	/// <inheritdoc/>
	readonly bool ISet<Space>.SetEquals(IEnumerable<Space> other) => this == [.. other];

	/// <inheritdoc/>
	readonly SpaceSet IFiniteSet<SpaceSet, Space>.Negate() => ~this;

	/// <inheritdoc/>
	readonly IEnumerator IEnumerable.GetEnumerator() => ToArray().GetEnumerator();

	/// <inheritdoc/>
	readonly IEnumerator<Space> IEnumerable<Space>.GetEnumerator() => ToArray().AsEnumerable().GetEnumerator();

	/// <inheritdoc/>
	void ISet<Space>.ExceptWith(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		this &= ~otherSet;
	}

	/// <inheritdoc/>
	void ISet<Space>.IntersectWith(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		this &= otherSet;
	}

	/// <inheritdoc/>
	void ISet<Space>.SymmetricExceptWith(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		this = (this & ~otherSet) | (otherSet & ~this);
	}

	/// <inheritdoc/>
	void ISet<Space>.UnionWith(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		this |= otherSet;
	}

	/// <inheritdoc/>
	void ICollection<Space>.Add(Space item) => Add(item);

	/// <inheritdoc/>
	SpaceSet IInfiniteSet<SpaceSet, Space>.ExceptWith(SpaceSet other) => this &= ~other;


	/// <inheritdoc cref="IBitwiseOperators{TSelf, TOther, TResult}.op_OnesComplement(TSelf)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SpaceSet operator ~(in SpaceSet value)
	{
		var result = value;
		result._field[0] = ~result._field[0];
		result._field[1] = ~result._field[1];
		result._field[2] = ~result._field[2];
		result._field[3] = ~result._field[3];
		return result;
	}

	/// <inheritdoc cref="IAdditionOperators{TSelf, TOther, TResult}.op_Addition(TSelf, TOther)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SpaceSet operator +(in SpaceSet left, Space right)
	{
		var result = left;
		result.Add(right);
		return result;
	}

	/// <inheritdoc cref="ISubtractionOperators{TSelf, TOther, TResult}.op_Subtraction(TSelf, TOther)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SpaceSet operator -(in SpaceSet left, Space right)
	{
		var result = left;
		result.Remove(right);
		return result;
	}

	/// <inheritdoc cref="IBitwiseOperators{TSelf, TOther, TResult}.op_BitwiseAnd(TSelf, TOther)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SpaceSet operator &(in SpaceSet left, in SpaceSet right)
	{
		var result = left;
		result._field[0] &= right._field[0];
		result._field[1] &= right._field[1];
		result._field[2] &= right._field[2];
		result._field[3] &= right._field[3];
		return result;
	}

	/// <inheritdoc cref="IBitwiseOperators{TSelf, TOther, TResult}.op_BitwiseOr(TSelf, TOther)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SpaceSet operator |(in SpaceSet left, in SpaceSet right)
	{
		var result = left;
		result._field[0] |= right._field[0];
		result._field[1] |= right._field[1];
		result._field[2] |= right._field[2];
		result._field[3] |= right._field[3];
		return result;
	}

	/// <inheritdoc cref="IBitwiseOperators{TSelf, TOther, TResult}.op_ExclusiveOr(TSelf, TOther)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SpaceSet operator ^(in SpaceSet left, in SpaceSet right)
	{
		var result = left;
		result._field[0] ^= right._field[0];
		result._field[1] ^= right._field[1];
		result._field[2] ^= right._field[2];
		result._field[3] ^= right._field[3];
		return result;
	}

	/// <inheritdoc/>
	static SpaceSet IBitwiseOperators<SpaceSet, SpaceSet, SpaceSet>.operator ~(SpaceSet value) => ~value;

	/// <inheritdoc/>
	static SpaceSet IBitwiseOperators<SpaceSet, SpaceSet, SpaceSet>.operator &(SpaceSet left, SpaceSet right) => left & right;

	/// <inheritdoc/>
	static SpaceSet IBitwiseOperators<SpaceSet, SpaceSet, SpaceSet>.operator |(SpaceSet left, SpaceSet right) => left | right;

	/// <inheritdoc/>
	static SpaceSet IBitwiseOperators<SpaceSet, SpaceSet, SpaceSet>.operator ^(SpaceSet left, SpaceSet right) => left ^ right;

	/// <inheritdoc/>
	static SpaceSet IAdditionOperators<SpaceSet, Space, SpaceSet>.operator +(SpaceSet left, Space right) => left + right;

	/// <inheritdoc/>
	static SpaceSet ISubtractionOperators<SpaceSet, Space, SpaceSet>.operator -(SpaceSet left, Space right) => left - right;
}
