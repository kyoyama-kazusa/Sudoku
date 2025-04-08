namespace Sudoku.Concepts;

using CandidateMapBase = ICellMapOrCandidateMap<CandidateMap, Candidate, CandidateMap.Enumerator>;

/// <summary>
/// Encapsulates a binary series of candidate state table.
/// </summary>
/// <remarks>
/// <include file="../../global-doc-comments.xml" path="/g/large-structure"/>
/// </remarks>
[JsonConverter(typeof(Converter))]
[StructLayout(LayoutKind.Auto)]
[CollectionBuilder(typeof(CandidateMap), nameof(Create))]
[DebuggerStepThrough]
[TypeImpl(
	TypeImplFlags.Object_Equals | TypeImplFlags.Object_ToString
		| TypeImplFlags.AllEqualityComparisonOperators | TypeImplFlags.TrueAndFalseOperators
		| TypeImplFlags.LogicalNotOperator | TypeImplFlags.Equatable,
	IsLargeStructure = true)]
public partial struct CandidateMap : CandidateMapBase, IDrawableItem
{
	/// <summary>
	/// Indicates the length of the backing buffer.
	/// The size of the buffer is 12 <c><![CDATA[floor(729 / sizeof(long) << 6)]]></c>.
	/// </summary>
	private const int Length = 12;


	/// <inheritdoc cref="CandidateMapBase.Empty"/>
	public static readonly CandidateMap Empty = [];

	/// <inheritdoc cref="CandidateMapBase.Full"/>
	public static readonly CandidateMap Full = ~default(CandidateMap);


	/// <summary>
	/// Indicates the internal field that provides the visit entry for fixed-sized buffer type <see cref="BackingBuffer"/>.
	/// </summary>
	/// <seealso cref="BackingBuffer"/>
	[EquatableMember]
	private BackingBuffer _bits;


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
		foreach (var c in PeersMap[cell])
		{
			Add(c * 9 + digit);
		}
		for (var d = 0; d < 9; d++)
		{
			if (d != digit || d == digit && withItself)
			{
				Add(cell * 9 + d);
			}
		}
	}


	/// <inheritdoc/>
	public readonly int Count
	{
		get
		{
			var result = 0;
			foreach (ref readonly var vector in _bits.Vectors)
			{
				for (var i = 0; i < 4; i++)
				{
					result += BitOperations.PopCount(vector[i]);
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
			for (var digit = 0; digit < 9; digit++)
			{
				if (this / digit)
				{
					result |= (Mask)(1 << digit);
				}
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
	/// Returns a <see cref="FrozenDictionary{TKey, TValue}"/> that describes the distribution of digits appeared in cells, grouped by digit.
	/// </summary>
	public readonly FrozenDictionary<Digit, CellMap> DigitDistribution
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
			return dictionary.ToFrozenDictionary();
		}
	}

	/// <summary>
	/// Returns a <see cref="FrozenDictionary{TKey, TValue}"/> that describes the distribution of digits appeared in cells, grouped by cell.
	/// </summary>
	public readonly FrozenDictionary<Cell, Mask> CellDistribution
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
			return dictionary.ToFrozenDictionary();
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
			for (var i = 0; i < Length; i++)
			{
				for (
					var value = _bits[i];
					value != 0;
					arr[pos++] = (i << 6) + BitOperations.TrailingZeroCount(value), value &= value - 1
				) ;
			}
			return arr;
		}
	}

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
			for (var i = 0; i < Length; i++)
			{
				var bits = _bits[i];
				var z = bmi2IsSupported
					? BitOperations.TrailingZeroCount(Bmi2.X64.ParallelBitDeposit(1UL << index - popCountSum, bits))
					: bits.SetAt(index - popCountSum);
				switch (bmi2IsSupported)
				{
					case true when z != 64:
					case false when z != -1:
					{
						return z + (i << 6); // * 64
					}
				}

				popCountSum += BitOperations.PopCount(bits);
			}
			return -1;
		}
	}


	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly void CopyTo(ref Candidate sequence, Candidate length)
		=> Offsets.AsReadOnlySpan().TryCopyTo(@ref.AsSpan(ref sequence, length));

	/// <summary>
	/// Determine whether the map contains the specified offset.
	/// </summary>
	/// <param name="item">The offset.</param>
	/// <returns>A <see cref="bool"/> value indicating that.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool Contains(Candidate item) => (_bits[item >> 6] >> (item & 63) & 1) != 0;

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
	{
		var targetString = ToString(provider);
		if (destination.Length < targetString.Length)
		{
			goto ReturnFalse;
		}

		if (targetString.TryCopyTo(destination))
		{
			charsWritten = targetString.Length;
			return true;
		}

	ReturnFalse:
		charsWritten = 0;
		return false;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool TryFormat(Span<byte> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
	{
		var targetString = ToString(provider);
		if (destination.Length < targetString.Length)
		{
			goto ReturnFalse;
		}

		if ((from character in targetString select (byte)character).TryCopyTo(destination))
		{
			charsWritten = targetString.Length;
			return true;
		}

	ReturnFalse:
		charsWritten = 0;
		return false;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly int CompareTo(in CandidateMap other)
	{
		return Count > other.Count ? 1 : Count < other.Count ? -1 : -Math.Sign($"{b(this)}".CompareTo($"{b(other)}"));


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static string b(in CandidateMap f) => f.ToString(new BitmapCandidateMapFormatInfo());
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

	/// <inheritdoc/>
	public readonly void ForEach(Action<Candidate> action)
	{
		foreach (var element in this)
		{
			action(element);
		}
	}

	/// <inheritdoc cref="object.GetHashCode"/>
	public override readonly int GetHashCode() => _bits.GetHashCode();

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly string ToString(IFormatProvider? formatProvider)
		=> formatProvider switch
		{
			CandidateMapFormatInfo i => i.FormatCore(this),
			_ => CoordinateConverter.GetInstance(formatProvider).CandidateConverter(this)
		};

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly Candidate[] ToArray() => Offsets[..];

	/// <inheritdoc cref="ToArray"/>
	/// <remarks><b><i>
	/// Although the return value is an array, you cannot modify them to keep data-safety (read-only array).
	/// Returning other types of such value will raise extra conversion, which is slower.
	/// </i></b></remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly Candidate[] ToArrayUnsafe() => Offsets;

	/// <summary>
	/// Try to get digits that is in the current collection.
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

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly Enumerator GetEnumerator() => new(Offsets);

	/// <summary>
	/// Try to enumerate cells on each candidate.
	/// </summary>
	/// <returns>The enumerator instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly CellEnumerator EnumerateCells() => new(Offsets);

	/// <summary>
	/// Try to enumerate cell and digit value on each candidate.
	/// </summary>
	/// <returns>The enumerator instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly CellDigitEnumerator EnumerateCellDigit() => new(Offsets);

	/// <inheritdoc/>
	public readonly CandidateMap Slice(int start, int count)
	{
		var result = Empty;
		var offsets = Offsets;
		for (int i = start, end = start + count; i < end; i++)
		{
			result.Add(offsets[i]);
		}
		return result;
	}

	/// <summary>
	/// Add a new <see cref="Candidate"/> into the collection.
	/// </summary>
	/// <param name="item">The offset to be added.</param>
	/// <returns>A <see cref="bool"/> value indicating whether the collection has already contained the offset.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Add(Candidate item)
	{
		if (Contains(item))
		{
			return false;
		}

		_bits[item >> 6] |= 1UL << (item & 63);
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Remove(Candidate item)
	{
		if (!Contains(item))
		{
			return false;
		}

		_bits[item >> 6] &= ~(1UL << (item & 63));
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Toggle(Candidate offset) => _ = Contains(offset) ? Remove(offset) : Add(offset);

	/// <summary>
	/// Remove all elements stored in the current collection, and set the property <see cref="Count"/> to zero.
	/// </summary>
	/// <seealso cref="Count"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Clear() => this = Empty;

	/// <inheritdoc/>
	readonly bool IAnyAllMethod<CandidateMap, Candidate>.Any() => Count != 0;

	/// <inheritdoc/>
	readonly bool IAnyAllMethod<CandidateMap, Candidate>.Any(Func<Candidate, bool> predicate) => this.Any(predicate);

	/// <inheritdoc/>
	readonly bool IAnyAllMethod<CandidateMap, Candidate>.All(Func<Candidate, bool> predicate) => this.All(predicate);

	/// <inheritdoc/>
	readonly string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(formatProvider as CultureInfo);

	/// <inheritdoc/>
	readonly Candidate IFirstLastMethod<CandidateMap, Candidate>.First() => this[0];

	/// <inheritdoc/>
	readonly Candidate IFirstLastMethod<CandidateMap, Candidate>.First(Func<Candidate, bool> predicate) => this.First(predicate);

	/// <inheritdoc/>
	readonly IEnumerable<Candidate> IWhereMethod<CandidateMap, Candidate>.Where(Func<Candidate, bool> predicate)
		=> this.Where(predicate);

	/// <inheritdoc/>
	readonly IEnumerable<IGrouping<TKey, Candidate>> IGroupByMethod<CandidateMap, Candidate>.GroupBy<TKey>(Func<Candidate, TKey> keySelector)
		=> this.GroupBy(keySelector).ToArray().Select(static element => (IGrouping<TKey, Candidate>)element);

	/// <inheritdoc/>
	readonly IEnumerable<TResult> ISelectMethod<CandidateMap, Candidate>.Select<TResult>(Func<Candidate, TResult> selector)
		=> this.Select(selector).ToArray();


	/// <inheritdoc cref="IParsable{TSelf}.TryParse(string?, IFormatProvider?, out TSelf)"/>
	public static bool TryParse(string str, out CandidateMap result)
	{
		try
		{
			result = Parse(str);
			return true;
		}
		catch (FormatException)
		{
			result = default;
			return false;
		}
	}

	/// <inheritdoc/>
	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out CandidateMap result)
	{
		try
		{
			if (s is null)
			{
				result = [];
				return false;
			}

			result = Parse(s, provider);
			return true;
		}
		catch (FormatException)
		{
			result = [];
			return false;
		}
	}

	/// <inheritdoc cref="TryParse(ReadOnlySpan{char}, IFormatProvider?, out CandidateMap)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryParse(ReadOnlySpan<char> s, out CandidateMap result) => TryParse(s, null, out result);

	/// <inheritdoc/>
	public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out CandidateMap result)
	{
		try
		{
			result = Parse(s, provider);
			return true;
		}
		catch (FormatException)
		{
			result = [];
			return false;
		}
	}

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
			result.Add(candidate);
		}
		return result;
	}

	/// <inheritdoc/>
	public static CandidateMap Parse(string str)
	{
		foreach (var parser in
			from element in Enum.GetValues<CoordinateType>()
			let parser = element.GetParser()
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CandidateMap Parse(string s, IFormatProvider? provider)
		=> provider switch
		{
			CandidateMapFormatInfo c => c.ParseCore(s),
			_ => CoordinateParser.GetInstance(provider).CandidateParser(s)
		};

	/// <inheritdoc cref="Parse(ReadOnlySpan{char}, IFormatProvider?)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CandidateMap Parse(ReadOnlySpan<char> s) => Parse(s, null);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CandidateMap Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s.ToString(), provider);

	/// <summary>
	/// Creates a <see cref="CandidateMap"/> via a triplet of <see cref="Vector256{T}"/> of <see cref="ulong"/> values.
	/// </summary>
	/// <param name="e0">The lower 256 bits.</param>
	/// <param name="e1">The middle 256 bits.</param>
	/// <param name="e2">The higher 256 bits.</param>
	/// <returns>A <see cref="CandidateMap"/> instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CandidateMap CreateByVectors(in Vector256<ulong> e0, in Vector256<ulong> e1, in Vector256<ulong> e2)
	{
		Unsafe.SkipInit(out CandidateMap result);
		e0.CopyTo(result._bits[..4]);
		e1.CopyTo(result._bits[4..8]);
		e2.CopyTo(result._bits[8..]);
		return result;
	}


	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CandidateMap operator ~(in CandidateMap offsets)
	{
		var vectors = offsets._bits.Vectors;
		return CreateByVectors(~vectors[0], ~vectors[1], ~vectors[2]);
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CandidateMap operator +(in CandidateMap collection, Candidate offset)
	{
		var result = collection;
		result.Add(offset);
		return result;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CandidateMap operator -(in CandidateMap collection, Candidate offset)
	{
		var result = collection;
		result.Remove(offset);
		return result;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CandidateMap operator %(in CandidateMap @base, in CandidateMap template)
		=> (@base & template).PeerIntersection & template;

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CandidateMap operator &(in CandidateMap left, in CandidateMap right)
	{
		var l = left._bits.Vectors;
		var r = right._bits.Vectors;
		return CreateByVectors(l[0] & r[0], l[1] & r[1], l[2] & r[2]);
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CandidateMap operator |(in CandidateMap left, in CandidateMap right)
	{
		var l = left._bits.Vectors;
		var r = right._bits.Vectors;
		return CreateByVectors(l[0] | r[0], l[1] | r[1], l[2] | r[2]);
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CandidateMap operator ^(in CandidateMap left, in CandidateMap right)
	{
		var l = left._bits.Vectors;
		var r = right._bits.Vectors;
		return CreateByVectors(l[0] ^ r[0], l[1] ^ r[1], l[2] ^ r[2]);
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
			var result = new CandidateMap[Combinatorial.PascalTriangle[n - 1][subsetSize - 1]];
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
			desiredSize += Combinatorial.PascalTriangle[n - 1][i - 1];
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
				result.Add(element / 9);
			}
		}
		return result;
	}
}
