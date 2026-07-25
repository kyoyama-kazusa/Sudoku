namespace Sudoku.Concepts;

using CandidateMapBase = IBitStateMap<CandidateMap, Candidate>;

/// <summary>
/// Encapsulates a binary series of candidate state table.
/// </summary>
/// <remarks>
/// This type uses 96 bytes.
/// </remarks>
[JsonConverter(typeof(Converter))]
[StructLayout(LayoutKind.Auto)]
[CollectionBuilder(typeof(CandidateMap), nameof(Create))]
public partial struct CandidateMap : CandidateMapBase
{
	/// <summary>
	/// Indicates the length of the backing buffer.
	/// The size of the buffer is 12 <c><![CDATA[floor(729 / sizeof(long) << 6)]]></c>.
	/// </summary>
	private const int InlineArrayLength = 12;


	/// <inheritdoc cref="CandidateMapBase.Empty"/>
	public static readonly CandidateMap Empty = [];

	/// <inheritdoc cref="CandidateMapBase.Full"/>
	public static readonly CandidateMap Full = ~default(CandidateMap);


	/// <summary>
	/// Indicates the internal field that represents a list of <see cref="long"/> bits.
	/// </summary>
	private InlineArray12<long> _bits;


	/// <summary>
	/// Initializes a <see cref="CandidateMap"/> instance via a list of candidate offsets represented as a RxCy notation.
	/// </summary>
	/// <param name="segments">The candidate offsets, represented as a RxCy notation.</param>
	[JsonConstructor]
	public CandidateMap(string[] segments) : this(segments, new RxCyParser())
	{
	}

	/// <summary>
	/// Initializes a <see cref="CandidateMap"/> instance via a list of candidate offsets represented as the specified notation.
	/// </summary>
	/// <param name="segments">The candidate offsets, represented as a RxCy notation.</param>
	/// <param name="parser">The parser.</param>
	internal CandidateMap(ReadOnlySpan<string> segments, CoordinateParser parser)
	{
		this = [];
		foreach (var segment in segments)
		{
			this |= Parse(segment, parser);
		}
	}

	/// <summary>
	/// Indicates a <see cref="CandidateMap"/> instance with the peer candidates of the specified candidate and a <see cref="bool"/>
	/// value indicating whether the map will process itself with <see langword="true"/> value.
	/// </summary>
	/// <param name="candidate">The candidate.</param>
	/// <param name="withItself">Indicates whether the map will process itself with <see langword="true"/> value.</param>
	private CandidateMap(Candidate candidate, bool withItself)
	{
		(this, var (cell, digit)) = ([], (candidate / 9, candidate % 9));
		foreach (var c in Peer.PeersMap[cell])
		{
			this += c * 9 + digit;
		}
		for (var d = 0; d < 9; d++)
		{
			if (d != digit || d == digit && withItself)
			{
				this += cell * 9 + d;
			}
		}
	}


	/// <inheritdoc/>
	public readonly int Count
	{
		get
		{
			var result = 0;
			foreach (ref readonly var vector in Vectors)
			{
				for (var i = 0; i < 4; i++)
				{
					result += PopCount((ulong)vector[i]);
				}
			}
			return result;
		}
	}

	/// <inheritdoc/>
	[JsonInclude]
	public readonly ReadOnlySpan<string> StringChunks
		=> this ? ToString().Split([',', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) : [];

	/// <summary>
	/// Indicates the digits used in this pattern.
	/// </summary>
	public readonly Mask Digits
	{
		get
		{
			var result = (Mask)0;
			foreach (var candidate in this)
			{
				result |= (Mask)(1 << candidate % 9);
			}
			return result;
		}
	}

	/// <summary>
	/// Indicates the cells used in this pattern.
	/// </summary>
	public readonly CellMap Cells => [.. CellDistribution.Keys];

	/// <inheritdoc/>
	public readonly CandidateMap PeerIntersection
	{
		get
		{
			if (Count == 0)
			{
				// Empty list can't contain any peer intersections.
				return [];
			}

			var result = Full;
			foreach (var candidate in Offsets)
			{
				result &= new CandidateMap(candidate, false);
			}
			return result;
		}
	}

	/// <summary>
	/// Returns a <see cref="IReadOnlyDictionary{TKey, TValue}"/> that describes the distribution of digits appeared in cells, grouped by digit.
	/// </summary>
	public readonly IReadOnlyDictionary<Digit, CellMap> DigitDistribution
	{
		get
		{
			var dictionary = new Dictionary<Digit, CellMap>(9);
			for (var digit = 0; digit < 9; digit++)
			{
				var map = this / digit;
				if (map)
				{
					dictionary.Add(digit, map);
				}
			}
			return dictionary;
		}
	}

	/// <summary>
	/// Returns a <see cref="IReadOnlyDictionary{TKey, TValue}"/> that describes the distribution of digits appeared in cells, grouped by cell.
	/// </summary>
	public readonly IReadOnlyDictionary<Cell, Mask> CellDistribution
	{
		get
		{
			var dictionary = new Dictionary<Cell, Mask>(81);
			foreach (var element in Offsets)
			{
				if (!dictionary.TryAdd(element / 9, (Mask)(1 << element % 9)))
				{
					dictionary[element / 9] |= (Mask)(1 << element % 9);
				}
			}
			return dictionary;
		}
	}

	/// <summary>
	/// Indicates the cell offsets in this collection.
	/// </summary>
	internal readonly Candidate[] Offsets
	{
		get
		{
			if (!this)
			{
				return [];
			}

			var (pos, arr) = (0, new Candidate[Count]);
			for (var i = 0; i < InlineArrayLength; i++)
			{
				for (
					var value = _bits[i];
					value != 0;
					arr[pos++] = (i << 6) + TrailingZeroCount(value), value &= value - 1
				) ;
			}
			return arr;
		}
	}

	/// <summary>
	/// Returns a sequence of <see cref="Vector256{T}"/> of <see cref="long"/> values that can be used in SIMD scenarios.
	/// </summary>
	internal readonly ReadOnlySpan<Vector256<long>> Vectors
		=> (Vector256<long>[])[Vector256.Create(_bits[..4]), Vector256.Create(_bits[4..8]), Vector256.Create(_bits[8..])];

	/// <inheritdoc/>
	readonly int CandidateMapBase.Shifting => sizeof(long) << 3;

	/// <inheritdoc/>
	readonly Candidate[] CandidateMapBase.Offsets => Offsets;


	/// <inheritdoc/>
	static Candidate CandidateMapBase.MaxCount => 9 * 9 * 9;

	/// <inheritdoc/>
	static ref readonly CandidateMap CandidateMapBase.Empty => ref Empty;

	/// <inheritdoc/>
	static ref readonly CandidateMap CandidateMapBase.Full => ref Full;


	/// <summary>
	/// Get the offset at the specified position index.
	/// </summary>
	/// <param name="index">The index.</param>
	/// <returns>
	/// The offset at the specified position index. If the value is invalid, the return value will be <c>-1</c>.
	/// </returns>
	public readonly Candidate this[Candidate index]
	{
		get
		{
			if (!this)
			{
				return -1;
			}

			var bmi2IsSupported = Bmi2.X64.IsSupported;
			var popCountSum = 0;
			for (var i = 0; i < InlineArrayLength; i++)
			{
				var bits = _bits[i];
				var z = bmi2IsSupported
					? TrailingZeroCount(Bmi2.X64.ParallelBitDeposit(1UL << index - popCountSum, (ulong)bits))
					: bits[index - popCountSum];
				switch (bmi2IsSupported)
				{
					case true when z != FallbackConstants.@long:
					case false when z != -1:
					{
						return z + (i << 6); // * 64
					}
				}

				popCountSum += PopCount((ulong)bits);
			}
			return -1;
		}
	}


	/// <inheritdoc/>
	public readonly void CopyTo(ref Candidate sequence, Candidate length)
		=> Offsets.AsReadOnlySpan().TryCopyTo(Span.Create(ref sequence, length));

	/// <inheritdoc/>
	public readonly void ForEach(Action<Candidate> action)
	{
		foreach (var element in this)
		{
			action(element);
		}
	}

	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	public readonly bool Equals(in CandidateMap other)
	{
		var leftVectors = Vectors;
		var rightVectors = other.Vectors;
		return leftVectors[0] == rightVectors[0] && leftVectors[1] == rightVectors[1] && leftVectors[2] == rightVectors[2];
	}

	/// <summary>
	/// Determine whether the map contains the specified offset.
	/// </summary>
	/// <param name="item">The offset.</param>
	/// <returns>A <see cref="bool"/> value indicating that.</returns>
	public readonly bool Contains(Candidate item) => (_bits[item >> 6] >> (item & 63) & 1) != 0;

	/// <inheritdoc/>
	public readonly override bool Equals([NotNullWhen(true)] object? obj) => obj is CandidateMap comparer && Equals(comparer);

	/// <summary>
	/// Indicates whether at least one element satisfies the specified condition.
	/// </summary>
	/// <param name="match">The match method.</param>
	/// <returns>A <see cref="bool"/> result indicating whether at least one element satisfies the specified condition.</returns>
	public readonly bool Exists(Func<Candidate, bool> match)
	{
		foreach (var candidate in this)
		{
			if (match(candidate))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Determine whether all <see cref="Candidate"/>s satisfy the specified condition.
	/// </summary>
	/// <param name="match">The match method.</param>
	/// <returns>A <see cref="bool"/> result indicating whether all elements satisfy the specified condition.</returns>
	public readonly bool TrueForAll(Func<Candidate, bool> match)
	{
		foreach (var candidate in this)
		{
			if (!match(candidate))
			{
				return false;
			}
		}
		return true;
	}

	/// <inheritdoc/>
	public readonly int CompareTo(in CandidateMap other)
	{
		return Count > other.Count ? 1 : Count < other.Count ? -1 : -Math.Sign($"{b(this)}".CompareTo($"{b(other)}"));


		static string b(in CandidateMap f) => f.ToString(new BitmapCandidateMapConverter());
	}

	/// <inheritdoc/>
	public readonly int IndexOf(Candidate offset)
	{
		for (var index = 0; index < Count; index++)
		{
			if (this[index] == offset)
			{
				return index;
			}
		}
		return -1;
	}

	/// <inheritdoc cref="object.GetHashCode"/>
	public readonly override int GetHashCode()
	{
		var hashCode = default(HashCode);
		for (var i = 0; i < InlineArrayLength; i++)
		{
			hashCode.Add(this[i]);
		}
		return hashCode.ToHashCode();
	}

	/// <inheritdoc cref="object.ToString"/>
	public readonly override string ToString() => ToString(CoordinateConverter.InvariantCulture);

	/// <inheritdoc/>
	public readonly string ToString(CultureInfo culture) => ToString(CoordinateConverter.GetInstance(culture));

	/// <inheritdoc/>
	public readonly string ToString(CoordinateConverter converter) => converter.CandidateConverter(this);

	/// <inheritdoc cref="CandidateMapBase.ToString(IValueConverter{CandidateMap})"/>
	public readonly string ToString(ICandidateMapConverter converter)
		=> converter.TryFormat(in this, null, out var result) ? result : throw new FormatException();

	/// <inheritdoc cref="CandidateMapBase.ToString(IValueConverter{CandidateMap}, IFormatProvider?)"/>
	public readonly string ToString(ICandidateMapConverter converter, IFormatProvider? formatProvider)
		=> converter.TryFormat(in this, formatProvider, out var result) ? result : throw new FormatException();

	/// <inheritdoc/>
	public readonly Candidate[] ToArray() => Offsets[..];

	/// <inheritdoc cref="ToArray"/>
	/// <remarks><b><i>
	/// Although the return value is an array, you cannot modify them to keep data-safety (read-only array).
	/// Returning other types of such value will raise extra conversion, which is slower.
	/// </i></b></remarks>
	public readonly Candidate[] ToArrayUnsafe() => Offsets;

	/// <summary>
	/// Try to get digits that are in the current collection.
	/// </summary>
	/// <param name="cell">The desired cell.</param>
	/// <returns>The digits.</returns>
	public readonly Mask GetDigitsFor(Cell cell)
	{
		var result = (Mask)0;
		for (var (candidate, digit) = (cell * 9, 0); digit < 9; candidate++, digit++)
		{
			if (Contains(candidate))
			{
				result |= (Mask)(1 << digit);
			}
		}
		return result;
	}

	/// <summary>
	/// Try to get positions that are in the current collection.
	/// </summary>
	/// <param name="house">The desired house.</param>
	/// <param name="digit">The desired digit.</param>
	/// <returns>The positions.</returns>
	public readonly Mask GetPositionsFor(House house, Digit digit)
	{
		var result = (Mask)0;
		for (var i = 0; i < 9; i++)
		{
			if (Contains(HousesCells[house][i] * 9 + digit))
			{
				result |= (Mask)(1 << i);
			}
		}
		return result;
	}

	/// <inheritdoc/>
	public readonly AnonymousSpanEnumerator<Candidate> GetEnumerator() => new(Offsets);

	/// <summary>
	/// Try to enumerate cells on each candidate.
	/// </summary>
	/// <returns>The enumerator instance.</returns>
	public readonly CellEnumerator EnumerateCells() => new(Offsets);

	/// <summary>
	/// Try to enumerate cell and digit value on each candidate.
	/// </summary>
	/// <returns>The enumerator instance.</returns>
	public readonly CellDigitEnumerator EnumerateCellDigit() => new(Offsets);

	/// <inheritdoc/>
	public readonly CandidateMap Slice(int start, int count)
	{
		var result = Empty;
		var offsets = Offsets;
		for (int i = start, end = start + count; i < end; i++)
		{
			result += offsets[i];
		}
		return result;
	}

	/// <summary>
	/// Finds the first candidate that satisfies the specified condition.
	/// </summary>
	/// <param name="match">The condition to be used.</param>
	/// <returns>The first found candidate or -1 if none found.</returns>
	public readonly Candidate First(Func<Candidate, bool> match)
	{
		foreach (var candidate in Offsets)
		{
			if (match(candidate))
			{
				return candidate;
			}
		}
		return -1;
	}

	/// <summary>
	/// Finds the first candidate that satisfies the specified condition.
	/// </summary>
	/// <param name="grid">The grid to be used.</param>
	/// <param name="match">The condition to be used.</param>
	/// <returns>The first found candidate or -1 if none found.</returns>
	public readonly Candidate First(in Grid grid, BitStateMapPredicate<CandidateMap, Candidate> match)
	{
		foreach (var candidate in Offsets)
		{
			if (match(candidate, grid))
			{
				return candidate;
			}
		}
		return -1;
	}

	/// <inheritdoc cref="CellMap.Select{TResult}(Func{Cell, TResult})"/>
	public readonly ReadOnlySpan<TResult> Select<TResult>(Func<Candidate, TResult> selector)
	{
		var offsets = Offsets;
		var result = new TResult[offsets.Length];
		for (var i = 0; i < offsets.Length; i++)
		{
			result[i] = selector(offsets[i]);
		}
		return result;
	}

	/// <summary>
	/// Filters a <see cref="CandidateMap"/> collection based on a predicate.
	/// </summary>
	/// <param name="predicate">A function to test each element for a condition.</param>
	/// <returns>
	/// A <see cref="CandidateMap"/> that contains elements from the input <see cref="CandidateMap"/> satisfying the condition.
	/// </returns>
	public readonly CandidateMap Where(Func<Candidate, bool> predicate)
	{
		var result = this;
		foreach (var cell in Offsets)
		{
			if (!predicate(cell))
			{
				result -= cell;
			}
		}
		return result;
	}

	/// <inheritdoc cref="CellMap.GroupBy{TKey}(Func{Cell, TKey})"/>
	public readonly ReadOnlySpan<BitStateMapGrouping<CandidateMap, Candidate, TKey>> GroupBy<TKey>(Func<Candidate, TKey> keySelector)
		where TKey : notnull
	{
		var dictionary = new Dictionary<TKey, CandidateMap>();
		foreach (var candidate in this)
		{
			var key = keySelector(candidate);
			if (!dictionary.TryAdd(key, [candidate]))
			{
				dictionary.GetValueRef(key) += candidate;
			}
		}

		var result = new BitStateMapGrouping<CandidateMap, Candidate, TKey>[dictionary.Count];
		var i = 0;
		foreach (var kvp in dictionary)
		{
			ref readonly var key = ref kvp.KeyRef;
			ref readonly var value = ref kvp.ValueRef;
			result[i++] = new(key, value);
		}
		return result;
	}

	/// <summary>
	/// Projects each candidate (of type <see cref="Candidate"/>) of a <see cref="CandidateMap"/> to a mask (of type <see cref="Mask"/>),
	/// flattens the resulting sequence into one sequence, and invokes a result selector function on each element therein.
	/// </summary>
	/// <typeparam name="TResult">The type of the elements of the resulting sequence.</typeparam>
	/// <param name="collectionSelector">A transform function to apply to each element of the input <see cref="CandidateMap"/>.</param>
	/// <param name="resultSelector">A transform function to apply to each element of the intermediate mask (of type <see cref="Mask"/>).</param>
	/// <returns>
	/// A <see cref="ReadOnlySpan{T}"/> of <typeparamref name="TResult"/> whose elements are the result
	/// of invoking the one-to-many transform function <paramref name="collectionSelector"/> on each element
	/// of the current instance and then mapping each of those sequences and their corresponding source element to a result element.
	/// </returns>
	public readonly ReadOnlySpan<TResult> SelectMany<TResult>(Func<Candidate, Mask> collectionSelector, Func<Candidate, Digit, TResult> resultSelector)
	{
		var result = new List<TResult>(Count << 1);
		foreach (var candidate in this)
		{
			foreach (var digit in collectionSelector(candidate))
			{
				result.AddRef(resultSelector(candidate, digit));
			}
		}
		return result.AsSpan();
	}

	/// <summary>
	/// Add a new <see cref="Candidate"/> into the collection.
	/// </summary>
	/// <param name="item">The offset to be added.</param>
	/// <returns>A <see cref="bool"/> value indicating whether the collection has already contained the offset.</returns>
	public bool Add(Candidate item)
	{
		if (Contains(item))
		{
			return false;
		}

		_bits[item >> 6] |= 1L << (item & 63);
		return true;
	}

	/// <inheritdoc/>
	public int AddRange(ReadOnlySpan<Candidate> offsets)
	{
		var result = 0;
		foreach (var offset in offsets)
		{
			if (Add(offset))
			{
				result++;
			}
		}
		return result;
	}

	/// <summary>
	/// Removes the specified offset from the current collection.
	/// </summary>
	/// <param name="item">An offset to be removed.</param>
	/// <returns>A <see cref="bool"/> value indicating whether the collection has already contained the specified offset.</returns>
	public bool Remove(Candidate item)
	{
		if (!Contains(item))
		{
			return false;
		}

		_bits[item >> 6] &= ~(1L << (item & 63));
		return true;
	}

	/// <summary>
	/// Remove all <see cref="Candidate"/> instances that is equal to the argument <paramref name="cell"/>.
	/// </summary>
	/// <param name="cell">The cell to be removed.</param>
	public int RemoveCell(Cell cell)
	{
		var result = 0;
		for (var digit = 0; digit < 9; digit++)
		{
			result += Remove(cell * 9 + digit) ? 1 : 0;
		}
		return result;
	}

	/// <inheritdoc/>
	public int RemoveRange(ReadOnlySpan<Candidate> offsets)
	{
		var result = 0;
		foreach (var offset in offsets)
		{
			if (Remove(offset))
			{
				result++;
			}
		}
		return result;
	}

	/// <inheritdoc/>
	public void Toggle(Candidate offset)
	{
		if (!Add(offset))
		{
			this -= offset;
		}
	}

	/// <summary>
	/// Remove all elements stored in the current collection, and set the property <see cref="Count"/> to zero.
	/// </summary>
	/// <seealso cref="Count"/>
	public void Clear() => this = Empty;

	/// <inheritdoc/>
	readonly bool IEquatable<CandidateMap>.Equals(CandidateMap other) => Equals(other);

	/// <inheritdoc/>
	readonly bool IAnyAllMethod<CandidateMap, Candidate>.Any() => Count != 0;

	/// <inheritdoc/>
	readonly bool IAnyAllMethod<CandidateMap, Candidate>.Any(Func<Candidate, bool> predicate) => Exists(predicate);

	/// <inheritdoc/>
	readonly bool IAnyAllMethod<CandidateMap, Candidate>.All(Func<Candidate, bool> predicate) => TrueForAll(predicate);

	/// <inheritdoc/>
	readonly string CandidateMapBase.ToString(IValueConverter<CandidateMap> converter)
		=> converter.TryFormat(in this, null, out var result) ? result : throw new FormatException();

	/// <inheritdoc/>
	readonly string CandidateMapBase.ToString(IValueConverter<CandidateMap> converter, IFormatProvider? formatProvider)
		=> converter.TryFormat(in this, formatProvider, out var result) ? result : throw new FormatException();

	/// <inheritdoc/>
	readonly Candidate IFirstLastMethod<CandidateMap, Candidate>.First() => this[0];

	/// <inheritdoc/>
	readonly Candidate IFirstLastMethod<CandidateMap, Candidate>.First(Func<Candidate, bool> predicate) => First(predicate);

	/// <inheritdoc/>
	readonly IEnumerable<Candidate> IWhereMethod<CandidateMap, Candidate>.Where(Func<Candidate, bool> predicate)
		=> Where(predicate);

	/// <inheritdoc/>
	readonly IEnumerable<IGrouping<TKey, Candidate>> IGroupByMethod<CandidateMap, Candidate>.GroupBy<TKey>(Func<Candidate, TKey> keySelector)
		=> GroupBy(keySelector).ToArray().Select(static element => (IGrouping<TKey, Candidate>)element);

	/// <inheritdoc/>
	readonly IEnumerable<TResult> ISelectMethod<CandidateMap, Candidate>.Select<TResult>(Func<Candidate, TResult> selector)
		=> Select(selector).ToArray();


	/// <inheritdoc/>
	public static bool TryParse(string str, out CandidateMap result)
		=> TryParse(str, CoordinateParser.InvariantCulture, out result);

	/// <inheritdoc/>
	public static bool TryParse(string str, CultureInfo culture, out CandidateMap result)
		=> TryParse(str, CoordinateParser.GetInstance(culture), out result);

	/// <inheritdoc/>
	public static bool TryParse(string str, CoordinateParser converter, out CandidateMap result)
	{
		try
		{
			result = converter.CandidateParser(str);
			return true;
		}
		catch (FormatException)
		{
			result = Empty;
			return false;
		}
	}

	/// <inheritdoc/>
	public static bool TryParse(string str, IValueConverter<CandidateMap> converter, out CandidateMap result)
		=> converter.TryParse(str, null, out result);

	/// <inheritdoc/>
	public static bool TryParse(string str, IValueConverter<CandidateMap> converter, IFormatProvider? formatProvider, out CandidateMap result)
		=> converter.TryParse(str, formatProvider, out result);

	/// <inheritdoc/>
	public static bool TryParse(ReadOnlySpan<char> str, out CandidateMap result)
		=> TryParse(str, CoordinateParser.InvariantCulture, out result);

	/// <inheritdoc/>
	public static bool TryParse(ReadOnlySpan<char> str, CultureInfo culture, out CandidateMap result)
		=> TryParse(str, CoordinateParser.GetInstance(culture), out result);

	/// <inheritdoc/>
	public static bool TryParse(ReadOnlySpan<char> str, CoordinateParser converter, out CandidateMap result)
		=> TryParse(str.ToString(), converter, out result);

	/// <inheritdoc/>
	public static bool TryParse(ReadOnlySpan<char> str, IValueConverter<CandidateMap> converter, out CandidateMap result)
		=> converter.TryParse(str, null, out result);

	/// <inheritdoc/>
	public static bool TryParse(ReadOnlySpan<char> str, IValueConverter<CandidateMap> converter, IFormatProvider? formatProvider, out CandidateMap result)
		=> converter.TryParse(str, formatProvider, out result);

	/// <summary>
	/// Creates a <see cref="CandidateMap"/> instance via the specified candidates.
	/// </summary>
	/// <param name="candidates">The candidates.</param>
	/// <returns>A <see cref="CandidateMap"/> instance.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static CandidateMap Create(ReadOnlySpan<Candidate> candidates)
	{
		if (candidates.IsEmpty)
		{
			return default;
		}

		var result = default(CandidateMap);
		foreach (var candidate in candidates)
		{
			result += candidate;
		}
		return result;
	}

	/// <summary>
	/// Parses the current string into target instance.
	/// </summary>
	/// <param name="str">The string.</param>
	/// <returns>The instance.</returns>
	/// <exception cref="FormatException">Throws when any invalid characters encountered.</exception>
	public static CandidateMap Parse(string str)
	{
		foreach (var parser in
			from element in CoordinateType.AllValues
			let parser = element.Parser
			where parser is not null
			select parser)
		{
			if (parser.CandidateParser(str) is { Count: not 0 } result)
			{
				return result;
			}
		}

		throw new FormatException(SR.ExceptionMessage("StringValueInvalidToBeParsed"));
	}

	/// <inheritdoc/>
	public static CandidateMap Parse(string str, CultureInfo culture) => CoordinateParser.GetInstance(culture).CandidateParser(str);

	/// <inheritdoc/>
	public static CandidateMap Parse(string str, CoordinateParser converter) => converter.CandidateParser(str);

	/// <inheritdoc/>
	public static CandidateMap Parse(string str, IValueConverter<CandidateMap> converter)
		=> converter.TryParse(str, null, out var result) ? result : throw new FormatException();

	/// <inheritdoc/>
	public static CandidateMap Parse(string str, IValueConverter<CandidateMap> converter, IFormatProvider? formatProvider)
		=> converter.TryParse(str, formatProvider, out var result) ? result : throw new FormatException();

	/// <inheritdoc/>
	public static CandidateMap Parse(ReadOnlySpan<char> str) => Parse(str.ToString());

	/// <inheritdoc/>
	public static CandidateMap Parse(ReadOnlySpan<char> str, CultureInfo culture) => Parse(str.ToString(), culture);

	/// <inheritdoc/>
	public static CandidateMap Parse(ReadOnlySpan<char> str, CoordinateParser converter)
		=> converter.CandidateParser(str.ToString());

	/// <inheritdoc/>
	public static CandidateMap Parse(ReadOnlySpan<char> str, IValueConverter<CandidateMap> converter)
		=> converter.TryParse(str, null, out var result) ? result : throw new FormatException();

	/// <inheritdoc/>
	public static CandidateMap Parse(ReadOnlySpan<char> str, IValueConverter<CandidateMap> converter, IFormatProvider? formatProvider)
		=> converter.TryParse(str, formatProvider, out var result) ? result : throw new FormatException();

	/// <summary>
	/// Creates a <see cref="CandidateMap"/> via a triplet of <see cref="Vector256{T}"/> of <see cref="long"/> values.
	/// </summary>
	/// <param name="e0">The lower 256 bits.</param>
	/// <param name="e1">The middle 256 bits.</param>
	/// <param name="e2">The higher 256 bits.</param>
	/// <returns>A <see cref="CandidateMap"/> instance.</returns>
	public static CandidateMap CreateByVectors(in Vector256<long> e0, in Vector256<long> e1, in Vector256<long> e2)
	{
		Unsafe.SkipInit<CandidateMap>(out var result);
		e0.CopyTo(result._bits[..4]);
		e1.CopyTo(result._bits[4..8]);
		e2.CopyTo(result._bits[8..]);
		return result;
	}


	/// <inheritdoc/>
	public void operator &=(in CandidateMap value)
	{
		for (var i = 0; i < InlineArrayLength; i++)
		{
			_bits[i] &= value._bits[i];
		}
	}

	/// <inheritdoc/>
	public void operator |=(in CandidateMap value)
	{
		for (var i = 0; i < InlineArrayLength; i++)
		{
			_bits[i] |= value._bits[i];
		}
	}

	/// <inheritdoc/>
	public void operator ^=(in CandidateMap value)
	{
		for (var i = 0; i < InlineArrayLength; i++)
		{
			_bits[i] ^= value._bits[i];
		}
	}

	/// <inheritdoc/>
	public void operator +=(Candidate offset) => _bits[offset >> 6] |= 1L << (offset & 63);

	/// <inheritdoc/>
	public void operator -=(Candidate offset) => _bits[offset >> 6] &= ~(1L << (offset & 63));


	/// <inheritdoc/>
	public static bool operator !(in CandidateMap value) => value.Count == 0;

	/// <inheritdoc/>
	public static bool operator true(in CandidateMap value) => value.Count != 0;

	/// <inheritdoc/>
	public static bool operator false(in CandidateMap value) => value.Count == 0;

	/// <inheritdoc/>
	public static bool operator ==(in CandidateMap left, in CandidateMap right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(in CandidateMap left, in CandidateMap right) => !(left == right);

	/// <inheritdoc/>
	public static bool operator >(in CandidateMap left, in CandidateMap right) => left.CompareTo(right) > 0;

	/// <inheritdoc/>
	public static bool operator <(in CandidateMap left, in CandidateMap right) => left.CompareTo(right) < 0;

	/// <inheritdoc/>
	public static bool operator >=(in CandidateMap left, in CandidateMap right) => left.CompareTo(right) >= 0;

	/// <inheritdoc/>
	public static bool operator <=(in CandidateMap left, in CandidateMap right) => left.CompareTo(right) <= 0;

	/// <inheritdoc/>
	public static CandidateMap operator ~(in CandidateMap offsets)
	{
		var vectors = offsets.Vectors;
		return CreateByVectors(~vectors[0], ~vectors[1], ~vectors[2]);
	}

	/// <inheritdoc/>
	public static CandidateMap operator +(in CandidateMap collection, Candidate offset)
	{
		var result = collection;
		result += offset;
		return result;
	}

	/// <inheritdoc/>
	public static CandidateMap operator -(in CandidateMap collection, Candidate offset)
	{
		var result = collection;
		result -= offset;
		return result;
	}

	/// <inheritdoc/>
	public static CandidateMap operator &(in CandidateMap left, in CandidateMap right)
	{
		var l = left.Vectors;
		var r = right.Vectors;
		return CreateByVectors(l[0] & r[0], l[1] & r[1], l[2] & r[2]);
	}

	/// <inheritdoc/>
	public static CandidateMap operator |(in CandidateMap left, in CandidateMap right)
	{
		var l = left.Vectors;
		var r = right.Vectors;
		return CreateByVectors(l[0] | r[0], l[1] | r[1], l[2] | r[2]);
	}

	/// <inheritdoc/>
	public static CandidateMap operator ^(in CandidateMap left, in CandidateMap right)
	{
		var l = left.Vectors;
		var r = right.Vectors;
		return CreateByVectors(l[0] ^ r[0], l[1] ^ r[1], l[2] ^ r[2]);
	}

	/// <inheritdoc/>
	public static unsafe ReadOnlySpan<CandidateMap> operator &(in CandidateMap map, int subsetSize)
	{
		if (subsetSize == 0 || subsetSize > map.Count)
		{
			return [];
		}

		if (subsetSize == map.Count)
		{
			return (CandidateMap[])[map];
		}

		var n = map.Count;
		var buffer = stackalloc int[subsetSize];
		if (n <= CandidateMapBase.MaxLimit && subsetSize <= CandidateMapBase.MaxLimit)
		{
			// Optimization: Use table to get the total number of result elements.
			var totalIndex = 0;
			var result = new CandidateMap[Combinatorics.PascalTriangle[n - 1][subsetSize - 1]];
			e(result, subsetSize, n, subsetSize, map.Offsets, (r, c) => r[totalIndex++] = c.AsCandidateMap());
			return result;
		}
		else
		{
			if (n > CandidateMapBase.MaxLimit && subsetSize > CandidateMapBase.MaxLimit)
			{
				throw new NotSupportedException(SR.ExceptionMessage("SubsetsExceeded"));
			}
			var result = new List<CandidateMap>();
			e(result, subsetSize, n, subsetSize, map.Offsets, static (r, c) => r.AddRef(c.AsCandidateMap()));
			return result.AsSpan();
		}


		void e<T>(T result, int size, int last, int index, Candidate[] offsets, Action<T, ReadOnlySpan<Candidate>> addingAction)
			where T : allows ref struct
		{
			for (var i = last; i >= index; i--)
			{
				buffer[index - 1] = i - 1;
				if (index > 1)
				{
					e(result, size, i - 1, index - 1, offsets, addingAction);
				}
				else
				{
					var temp = new Candidate[size];
					for (var j = 0; j < size; j++)
					{
						temp[j] = offsets[buffer[j]];
					}
					addingAction(result, temp);
				}
			}
		}
	}

	/// <inheritdoc/>
	public static ReadOnlySpan<CandidateMap> operator |(in CandidateMap map, int subsetSize)
	{
		if (subsetSize == 0 || !map)
		{
			return [];
		}

		var (n, desiredSize) = (map.Count, 0);
		var length = Math.Min(n, subsetSize);
		for (var i = 1; i <= length; i++)
		{
			desiredSize += Combinatorics.PascalTriangle[n - 1][i - 1];
		}

		var result = new List<CandidateMap>(desiredSize);
		for (var i = 1; i <= length; i++)
		{
			result.AddRangeRef(map & i);
		}
		return result.AsSpan();
	}

	/// <inheritdoc/>
	public static ReadOnlySpan<CandidateMap> operator |(in CandidateMap map, Range subsetSizeRange)
	{
		if (!map)
		{
			return [];
		}

		var (s, e) = subsetSizeRange;
		var result = new List<CandidateMap>();
		for (var i = s.GetOffset(map.Count); i <= e.GetOffset(map.Count); i++)
		{
			result.AddRangeRef(map & i);
		}
		return result.AsSpan();
	}

	/// <summary>
	/// Reduces the <see cref="CandidateMap"/> instance, only checks for candidates
	/// whose digit is equal to argument <paramref name="digit"/>,
	/// and merge into a <see cref="CellMap"/> value.
	/// </summary>
	/// <param name="candidates">The candidates to be checked.</param>
	/// <param name="digit">The digit to be checked.</param>
	/// <returns>A <see cref="CellMap"/> instance.</returns>
	public static CellMap operator /(in CandidateMap candidates, Digit digit)
	{
		var result = CellMap.Empty;
		foreach (var element in candidates)
		{
			if (element % 9 == digit)
			{
				result += element / 9;
			}
		}
		return result;
	}
}

/// <summary>
/// Represents JSON serialization rules on type <see cref="CandidateMap"/>.
/// </summary>
file sealed class Converter : JsonConverter<CandidateMap>
{
	/// <inheritdoc/>
	public override bool HandleNull => false;


	/// <inheritdoc/>
	[SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "<Pending>")]
	public override CandidateMap Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> new(JsonSerializer.Deserialize<string[]>(ref reader, options)!);

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, CandidateMap value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		foreach (var element in value.StringChunks)
		{
			writer.WriteStringValue(element);
		}
		writer.WriteEndArray();
	}
}
