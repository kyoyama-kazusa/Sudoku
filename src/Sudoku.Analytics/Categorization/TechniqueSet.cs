namespace Sudoku.Categorization;

/// <summary>
/// Represents a set of <see cref="Technique"/> fields.
/// </summary>
/// <remarks>
/// This type uses a <see cref="BitArray"/> to construct the data structure. Because <see cref="BitArray"/> is a reference type,
/// we cannot directly copy a <see cref="TechniqueSet"/> value. If you want to clone a <see cref="TechniqueSet"/>,
/// please use range syntax:
/// <code><![CDATA[
/// var techniques = TechniqueSets.All;
/// var another = techniques[..]; // Copy all elements.
/// ]]></code>
/// </remarks>
/// <seealso cref="Technique"/>
/// <seealso cref="BitArray"/>
/// <completionlist cref="TechniqueSets"/>
[JsonConverter(typeof(Converter))]
[TypeImpl(
	TypeImplFlags.Object_Equals | TypeImplFlags.EqualityOperators | TypeImplFlags.TrueAndFalseOperators
		| TypeImplFlags.LogicalNotOperator)]
public sealed partial class TechniqueSet() :
	IAdditionOperators<TechniqueSet, Technique, TechniqueSet>,
	IAnyAllMethod<TechniqueSet, Technique>,
	IContainsMethod<TechniqueSet, Technique>,
	IEquatable<TechniqueSet>,
	IEqualityOperators<TechniqueSet, TechniqueSet, bool>,
	IFiniteSet<TechniqueSet, Technique>,
	IFormattable,
	IInfiniteSet<TechniqueSet, Technique>,
	ILogicalOperators<TechniqueSet>,
	IReadOnlySet<Technique>,
	ISelectMethod<TechniqueSet, Technique>,
	ISliceMethod<TechniqueSet, Technique>,
	ISubtractionOperators<TechniqueSet, Technique, TechniqueSet>,
	IToArrayMethod<TechniqueSet, Technique>,
	IWhereMethod<TechniqueSet, Technique>
{
	/// <summary>
	/// Indicates the information for the techniques, can look up the relation via its belonging technique group.
	/// This field will be used in extension method
	/// <see cref="TechniqueGroupExtensions.GetTechniques(TechniqueGroup, Func{Technique, bool}?)"/>.
	/// </summary>
	/// <seealso cref="TechniqueGroupExtensions.GetTechniques(TechniqueGroup, Func{Technique, bool}?)"/>
	public static readonly FrozenDictionary<TechniqueGroup, TechniqueSet> TechniqueRelationGroups;

	/// <summary>
	/// Indicates the technique groups and its containing techniques that supports customization on difficulty rating and level.
	/// Call <see cref="TechniqueExtensions.get_SupportsCustomizingDifficulty(Technique)"/>
	/// to check whether a technique supports configuration.
	/// </summary>
	/// <seealso cref="TechniqueExtensions.get_SupportsCustomizingDifficulty(Technique)"/>
	public static readonly FrozenDictionary<TechniqueGroup, TechniqueSet> ConfigurableTechniqueRelationGroups;

	/// <summary>
	/// Indicates the number of techniques included in this solution.
	/// </summary>
	private static readonly int TechniquesCount = Technique.Length - 1;


	/// <summary>
	/// The internal bits to store techniques.
	/// </summary>
	private readonly BitArray _bitArray = new(TechniquesCount);


	/// <summary>
	/// Copies a <see cref="TechniqueSet"/> instance, and adds it to the current collection.
	/// </summary>
	/// <param name="other">The other collection to be added.</param>
	public TechniqueSet(TechniqueSet other) : this()
	{
		foreach (var technique in other)
		{
			Add(technique);
		}
	}


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static TechniqueSet()
	{
		var dic = new Dictionary<TechniqueGroup, TechniqueSet>();
		foreach (var technique in Technique.Values)
		{
			if (technique != Technique.None && technique.TryGetGroup() is { } group && !dic.TryAdd(group, [technique]))
			{
				dic[group].Add(technique);
			}
		}
		TechniqueRelationGroups = dic.ToFrozenDictionary();

		var configurableDic = new Dictionary<TechniqueGroup, TechniqueSet>();
		foreach (var technique in Technique.Values)
		{
			if (technique.SupportsCustomizingDifficulty
				&& technique.TryGetGroup() is { } group && !configurableDic.TryAdd(group, [technique]))
			{
				configurableDic[group].Add(technique);
			}
		}
		ConfigurableTechniqueRelationGroups = configurableDic.ToFrozenDictionary();
	}


	/// <summary>
	/// Indicates the length of the technique.
	/// </summary>
	public int Count => _bitArray.Cardinality;

	/// <summary>
	/// Indicates the range of difficulty that the current collection contains.
	/// </summary>
	/// <remarks>
	/// This property returns a list of <see cref="DifficultyLevel"/> flags, merged into one instance.
	/// If you want to get the internal fields of flags the return value contains, use <see langword="foreach"/> loop to iterate them,
	/// or use method <see cref="EnumExtensions.get_AllFlags{T}(T)"/>.
	/// </remarks>
	/// <seealso cref="EnumExtensions.get_AllFlags{T}(T)"/>
	public DifficultyLevel DifficultyRange
	{
		get
		{
			var result = DifficultyLevel.Unknown;
			if (Count == 0)
			{
				return result;
			}

			foreach (var technique in this)
			{
				result |= technique.GetDifficultyLevel();
			}
			return result;
		}
	}

	/// <summary>
	/// Indicates the groups the current collection covered.
	/// </summary>
	public ReadOnlyMemory<TechniqueGroup> TechniqueGroups => (from technique in this select technique.GetGroup()).ToArray();

	/// <inheritdoc/>
	bool ICollection<Technique>.IsReadOnly => false;


	/// <summary>
	/// Try to get the <see cref="Technique"/> at the specified index.
	/// </summary>
	/// <param name="index">The index to be checked.</param>
	/// <returns>The found <see cref="Technique"/> instance.</returns>
	/// <exception cref="IndexOutOfRangeException">Throws when the index is out of range.</exception>
	public Technique this[int index]
	{
		get
		{
			var i = -1;
			foreach (var technique in this)
			{
				if (++i == index)
				{
					return technique;
				}
			}
			throw new IndexOutOfRangeException();
		}
	}

	/// <summary>
	/// Checks the index of the specified technique.
	/// </summary>
	/// <param name="technique">The technique to be checked.</param>
	/// <returns>The index that the technique is at. If none found, -1.</returns>
	public int this[Technique technique]
	{
		get
		{
			var result = 0;
			foreach (var currentTechnique in this)
			{
				if (currentTechnique == technique)
				{
					return result;
				}
				result++;
			}
			return -1;
		}
	}


	/// <summary>
	/// Clears the collection, making all techniques to be removed.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Clear() => _bitArray.SetAll(false);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals([NotNullWhen(true)] TechniqueSet? other) => other is not null && _bitArray.SequenceEqual(other._bitArray);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Contains(Technique item) => _bitArray[TechniqueProjection(item)];

	/// <summary>
	/// Determines whether at least one <see cref="Technique"/> instance satisfies the specified condition.
	/// </summary>
	/// <param name="match">The match method to be used.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool Exists(Func<Technique, bool> match)
	{
		foreach (var technique in this)
		{
			if (match(technique))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Try to add a new technique.
	/// </summary>
	/// <param name="item">A technique to be added.</param>
	/// <returns>A <see cref="bool"/> result indicating whether the current technique is successfully added.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Add(Technique item)
	{
		if (_bitArray[TechniqueProjection(item)])
		{
			return false;
		}

		_bitArray.Set(TechniqueProjection(item), true);
		return true;
	}

	/// <summary>
	/// Try to add a list of <see cref="Technique"/> instances into the collection.
	/// </summary>
	/// <param name="techniques">A list of <see cref="Technique"/> instances to be added.</param>
	/// <returns>An <see cref="int"/> value indicating the number of <see cref="Technique"/> instances to be added.</returns>
	public int AddRange(params ReadOnlySpan<Technique> techniques)
	{
		var result = 0;
		foreach (var technique in techniques)
		{
			if (Add(technique))
			{
				result++;
			}
		}
		return result;
	}

	/// <summary>
	/// Try to remove a technique from the collection.
	/// </summary>
	/// <param name="item">A technique to be removed.</param>
	/// <returns>A <see cref="bool"/> result indicating whether the current technique is successfully removed.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Remove(Technique item)
	{
		if (!_bitArray[TechniqueProjection(item)])
		{
			return false;
		}

		_bitArray.Set(TechniqueProjection(item), false);
		return true;
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		var result = 0;
		var flag = false;
		foreach (var technique in this)
		{
			var target = (int)technique.GetGroup() * 1000000 + (int)technique;
			result |= flag ? target >> 10 : target;
			result += flag ? 7 : 31;
			flag = !flag;
		}
		return result;
	}

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString() => ToString(null);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public string ToString(IFormatProvider? formatProvider)
	{
		var culture = formatProvider as CultureInfo ?? CultureInfo.CurrentUICulture;
		var currentCountryOrRegionName = culture.Parent.Name;
		var isCurrentCountryOrRegionUseEnglish = currentCountryOrRegionName.Equals(SR.EnglishLanguage, StringComparison.OrdinalIgnoreCase);
		return string.Join(
			SR.Get("Comma", culture),
			from technique in this
			let name = technique.GetName(culture)
			select isCurrentCountryOrRegionUseEnglish ? $"{name}" : $"{name} ({technique.EnglishName})"
		);
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Technique[] ToArray() => [.. this];

	/// <inheritdoc cref="ISliceMethod{TSelf, TSource}.Slice(int, int)"/>
	public TechniqueSet Slice(int start, int count)
	{
		var result = TechniqueSets.None;
		var i = start - 1;
		foreach (var technique in this)
		{
			if (++i < start + count)
			{
				result.Add(technique);
			}
		}
		return result;
	}

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Enumerator GetEnumerator() => new(_bitArray);

	/// <inheritdoc/>
	void ICollection<Technique>.CopyTo(Technique[] array, int arrayIndex)
		=> Array.Copy(this[arrayIndex..].ToArray(), array, Count - arrayIndex);

	/// <inheritdoc/>
	void ICollection<Technique>.Add(Technique item) => Add(item);

	/// <inheritdoc/>
	void ISet<Technique>.ExceptWith(IEnumerable<Technique> other)
	{
		foreach (var technique in other)
		{
			Remove(technique);
		}
	}

	/// <inheritdoc/>
	void ISet<Technique>.IntersectWith(IEnumerable<Technique> other)
	{
		foreach (var technique in other)
		{
			if (!Contains(technique))
			{
				Remove(technique);
			}
		}
	}

	/// <inheritdoc/>
	void ISet<Technique>.SymmetricExceptWith(IEnumerable<Technique> other)
	{
		TechniqueSet otherSet = [.. other];

		Clear();
		foreach (var technique in this & ~otherSet | otherSet & ~this)
		{
			Add(technique);
		}
	}

	/// <inheritdoc/>
	void ISet<Technique>.UnionWith(IEnumerable<Technique> other)
	{
		foreach (var technique in other)
		{
			Add(technique);
		}
	}

	/// <inheritdoc/>
	bool ISet<Technique>.IsProperSubsetOf(IEnumerable<Technique> other) => ((IReadOnlySet<Technique>)this).IsProperSubsetOf(other);

	/// <inheritdoc/>
	bool ISet<Technique>.IsProperSupersetOf(IEnumerable<Technique> other) => ((IReadOnlySet<Technique>)this).IsProperSupersetOf(other);

	/// <inheritdoc/>
	bool ISet<Technique>.IsSubsetOf(IEnumerable<Technique> other) => ((IReadOnlySet<Technique>)this).IsSubsetOf(other);

	/// <inheritdoc/>
	bool ISet<Technique>.IsSupersetOf(IEnumerable<Technique> other) => ((IReadOnlySet<Technique>)this).IsSupersetOf(other);

	/// <inheritdoc/>
	bool ISet<Technique>.Overlaps(IEnumerable<Technique> other) => ((IReadOnlySet<Technique>)this).Overlaps(other);

	/// <inheritdoc/>
	bool ISet<Technique>.SetEquals(IEnumerable<Technique> other) => ((IReadOnlySet<Technique>)this).SetEquals(other);

	/// <inheritdoc/>
	bool IReadOnlySet<Technique>.IsProperSubsetOf(IEnumerable<Technique> other)
	{
		TechniqueSet otherSet = [.. other];
		return (otherSet & this) == this && this != otherSet;
	}

	/// <inheritdoc/>
	bool IReadOnlySet<Technique>.IsProperSupersetOf(IEnumerable<Technique> other)
	{
		TechniqueSet otherSet = [.. other];
		return (this & otherSet) == otherSet && this != otherSet;
	}

	/// <inheritdoc/>
	bool IReadOnlySet<Technique>.IsSubsetOf(IEnumerable<Technique> other) => ([.. other] & this) == this;

	/// <inheritdoc/>
	bool IReadOnlySet<Technique>.IsSupersetOf(IEnumerable<Technique> other)
	{
		TechniqueSet otherSet = [.. other];
		return (this & otherSet) == otherSet;
	}

	/// <inheritdoc/>
	bool IReadOnlySet<Technique>.Overlaps(IEnumerable<Technique> other) => (this & [.. other]).Count != 0;

	/// <inheritdoc/>
	bool IReadOnlySet<Technique>.SetEquals(IEnumerable<Technique> other) => this == [.. other];

	/// <inheritdoc/>
	bool IAnyAllMethod<TechniqueSet, Technique>.Any() => Count != 0;

	/// <inheritdoc/>
	bool IAnyAllMethod<TechniqueSet, Technique>.Any(Func<Technique, bool> predicate) => Exists(predicate);

	/// <inheritdoc/>
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(formatProvider);

	/// <inheritdoc/>
	TechniqueSet IFiniteSet<TechniqueSet, Technique>.Negate() => ~this;

	/// <inheritdoc/>
	TechniqueSet IInfiniteSet<TechniqueSet, Technique>.ExceptWith(TechniqueSet other) => this & ~other;

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => _bitArray.GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<Technique> IEnumerable<Technique>.GetEnumerator()
	{
		var index = 0;
		foreach (bool techniqueBit in _bitArray)
		{
			if (techniqueBit)
			{
				yield return TechniqueProjectionBack(index);
			}
			index++;
		}
	}

	/// <inheritdoc/>
	IEnumerable<Technique> ISliceMethod<TechniqueSet, Technique>.Slice(int start, int count) => Slice(start, count);

	/// <inheritdoc/>
	IEnumerable<Technique> IWhereMethod<TechniqueSet, Technique>.Where(Func<Technique, bool> predicate) => this.Where(predicate);

	/// <inheritdoc/>
	IEnumerable<TResult> ISelectMethod<TechniqueSet, Technique>.Select<TResult>(Func<Technique, TResult> selector)
		=> this.Select(selector).ToArray();


	/// <summary>
	/// Project the <see cref="Technique"/> instance into an <see cref="int"/> value as an index of <see cref="BitArray"/> field.
	/// </summary>
	/// <param name="technique">The technique.</param>
	/// <returns>The index value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int TechniqueProjection(Technique technique) => (int)technique - 1;

	/// <summary>
	/// Projects the <see cref="int"/> value into a <see cref="Technique"/> field back.
	/// </summary>
	/// <param name="index">The index.</param>
	/// <returns>The technique field.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Technique TechniqueProjectionBack(int index) => (Technique)index + 1;


	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TechniqueSet operator +(TechniqueSet left, Technique right) => [.. left, right];

	/// <inheritdoc cref="IAdditionOperators{TSelf, TOther, TResult}.op_Addition(TSelf, TOther)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TechniqueSet operator +(TechniqueSet left, TechniqueGroup right) => [.. left, .. TechniqueRelationGroups[right]];

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TechniqueSet operator -(TechniqueSet left, Technique right)
	{
		var result = left[..];
		result.Remove(right);
		return result;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TechniqueSet operator ~(TechniqueSet value)
	{
		var result = value[..];
		result._bitArray.Not();
		return result;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TechniqueSet operator &(TechniqueSet left, TechniqueSet right)
	{
		var result = left[..];
		result._bitArray.And(right._bitArray);
		return result;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TechniqueSet operator |(TechniqueSet left, TechniqueSet right)
	{
		var result = left[..];
		result._bitArray.Or(right._bitArray);
		return result;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TechniqueSet operator ^(TechniqueSet left, TechniqueSet right)
	{
		var result = left[..];
		result._bitArray.Xor(right._bitArray);
		return result;
	}
}
