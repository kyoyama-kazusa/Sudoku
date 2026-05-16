namespace Sudoku.Concepts;

using CellMapBase = IBitStateMap<CellMap, Cell>;

/// <summary>
/// Encapsulates a binary series of cell state table.
/// </summary>
/// <remarks>
/// This type uses 16 bytes.
/// </remarks>
/// <seealso cref="Vector128{T}"/>
[JsonConverter(typeof(Converter))]
[CollectionBuilder(typeof(CellMap), nameof(Create))]
public partial struct CellMap : CellMapBase
{
	/// <inheritdoc cref="CellMapBase.Shifting"/>
	private const int Shifting = 41;


	/// <summary>
	/// Indicates the constant that will be used on bitwise not operation.
	/// </summary>
	public static readonly Vector128<long> BitwiseNotConstant = CV(0xFF_FFFF_FFFFL, 0x1FF_FFFF_FFFFL);

	/// <summary>
	/// Indicates the <see cref="Vector128{T}"/> instances to be used for checking shared houses.
	/// </summary>
	/// <seealso cref="Vector128{T}"/>
	public static readonly ReadOnlyMemory<Vector128<long>> SharedHouseConstants = (Vector128<long>[])[
		CV(~0b_000000000_000000000_000000000_000000000_0000L, ~0b_00000_000000000_000000111_000000111_000000111L),
		CV(~0b_000000000_000000000_000000000_000000000_0000L, ~0b_00000_000000000_000111000_000111000_000111000L),
		CV(~0b_000000000_000000000_000000000_000000000_0000L, ~0b_00000_000000000_111000000_111000000_111000000L),
		CV(~0b_000000000_000000000_000000000_000000111_0000L, ~0b_00111_000000111_000000000_000000000_000000000L),
		CV(~0b_000000000_000000000_000000000_000111000_0001L, ~0b_11000_000111000_000000000_000000000_000000000L),
		CV(~0b_000000000_000000000_000000000_111000000_1110L, ~0b_00000_111000000_000000000_000000000_000000000L),
		CV(~0b_000000111_000000111_000000111_000000000_0000L, ~0b_00000_000000000_000000000_000000000_000000000L),
		CV(~0b_000111000_000111000_000111000_000000000_0000L, ~0b_00000_000000000_000000000_000000000_000000000L),
		CV(~0b_111000000_111000000_111000000_000000000_0000L, ~0b_00000_000000000_000000000_000000000_000000000L),
		CV(~0b_000000000_000000000_000000000_000000000_0000L, ~0b_00000_000000000_000000000_000000000_111111111L),
		CV(~0b_000000000_000000000_000000000_000000000_0000L, ~0b_00000_000000000_000000000_111111111_000000000L),
		CV(~0b_000000000_000000000_000000000_000000000_0000L, ~0b_00000_000000000_111111111_000000000_000000000L),
		CV(~0b_000000000_000000000_000000000_000000000_0000L, ~0b_00000_111111111_000000000_000000000_000000000L),
		CV(~0b_000000000_000000000_000000000_000000000_1111L, ~0b_11111_000000000_000000000_000000000_000000000L),
		CV(~0b_000000000_000000000_000000000_111111111_0000L, ~0b_00000_000000000_000000000_000000000_000000000L),
		CV(~0b_000000000_000000000_111111111_000000000_0000L, ~0b_00000_000000000_000000000_000000000_000000000L),
		CV(~0b_000000000_111111111_000000000_000000000_0000L, ~0b_00000_000000000_000000000_000000000_000000000L),
		CV(~0b_111111111_000000000_000000000_000000000_0000L, ~0b_00000_000000000_000000000_000000000_000000000L),
		CV(~0b_000000001_000000001_000000001_000000001_0000L, ~0b_00001_000000001_000000001_000000001_000000001L),
		CV(~0b_000000010_000000010_000000010_000000010_0000L, ~0b_00010_000000010_000000010_000000010_000000010L),
		CV(~0b_000000100_000000100_000000100_000000100_0000L, ~0b_00100_000000100_000000100_000000100_000000100L),
		CV(~0b_000001000_000001000_000001000_000001000_0000L, ~0b_01000_000001000_000001000_000001000_000001000L),
		CV(~0b_000010000_000010000_000010000_000010000_0000L, ~0b_10000_000010000_000010000_000010000_000010000L),
		CV(~0b_000100000_000100000_000100000_000100000_0001L, ~0b_00000_000100000_000100000_000100000_000100000L),
		CV(~0b_001000000_001000000_001000000_001000000_0010L, ~0b_00000_001000000_001000000_001000000_001000000L),
		CV(~0b_010000000_010000000_010000000_010000000_0100L, ~0b_00000_010000000_010000000_010000000_010000000L),
		CV(~0b_100000000_100000000_100000000_100000000_1000L, ~0b_00000_100000000_100000000_100000000_100000000L)
	];

	/// <inheritdoc cref="CellMapBase.Empty"/>
	public static readonly CellMap Empty = [];

	/// <inheritdoc cref="CellMapBase.Full"/>
	public static readonly CellMap Full = ~default(CellMap);

	/// <summary>
	/// Indicates the factor values for property <see cref="SharedHouses"/>.
	/// </summary>
	/// <seealso cref="SharedHouses"/>
	private static readonly HouseMask[] SharedHouseFactorValues = [
		0x1, 0x2, 0x4, 0x8, 0x10, 0x20, 0x40, 0x80, 0x100,
		0x200, 0x400, 0x800, 0x1000, 0x2000, 0x4000, 0x8000, 0x10000, 0x20000,
		0x40000, 0x80000, 0x100000, 0x200000, 0x400000, 0x800000, 0x1000000, 0x2000000, 0x4000000
	];


	/// <summary>
	/// Indicates the internal two <see cref="long"/> values,
	/// which represents 81 bits. Higher 40 bits and lower 41 bits, where each bit is:
	/// <list type="table">
	/// <item>
	/// <term><see langword="true"/> bit (1)</term>
	/// <description>The corresponding cell is contained in this collection</description>
	/// </item>
	/// <item>
	/// <term><see langword="false"/> bit (0)</term>
	/// <description>The corresponding cell is not contained in this collection</description>
	/// </item>
	/// </list>
	/// </summary>
	internal Vector128<long> _vector;


	/// <summary>
	/// Initializes a <see cref="CellMap"/> instance via a list of offsets represented as a RxCy notation.
	/// </summary>
	/// <param name="segments">The cell offsets, represented as a RxCy notation.</param>
	[JsonConstructor]
	public CellMap(string[] segments) : this(segments, new RxCyParser())
	{
	}

	/// <summary>
	/// Initializes a <see cref="CellMap"/> instance via a list of offsets represented as the specified notation.
	/// </summary>
	/// <param name="segments">The cell offsets, represented as a RxCy notation.</param>
	/// <param name="parser">The parser used.</param>
	internal CellMap(ReadOnlySpan<string> segments, CoordinateParser parser)
	{
		this = [];
		foreach (var segment in segments)
		{
			this |= Parse(segment, parser);
		}
	}


	/// <summary>
	/// Determines whether the current list of cells are all lie in an intersection area, i.e. a locked candidates.
	/// </summary>
	public readonly bool IsInIntersection => Count == 1 || Count <= 3 && PopCount((uint)SharedHouses) == 2;

	/// <summary>
	/// Indicates whether every cell in the current collection cannot see each other.
	/// </summary>
	/// <remarks><b>
	/// Please note that this property will return <see langword="false"/> if there's no cells or 1 cell in the current collection.
	/// </b></remarks>
	public readonly bool CanSeeEachOther
	{
		get
		{
			switch (Count)
			{
				case 0 or 1: { return false; }
				case 2: { return FirstSharedHouse != FallbackConstants.@int; }
				default:
				{
					foreach (ref readonly var pair in this & 2)
					{
						if (pair.FirstSharedHouse != FallbackConstants.@int)
						{
							return true;
						}
					}
					return false;
				}
			}
		}
	}

	/// <inheritdoc/>
	public readonly int Count => PopCount((ulong)_vector[1]) + PopCount((ulong)_vector[0]);

	/// <inheritdoc/>
	[JsonInclude]
	public readonly ReadOnlySpan<string> StringChunks
		=> this ? ToString().Split([',', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) : [];

	/// <summary>
	/// Indicates the mask of block that all cells in this collection spanned.
	/// </summary>
	/// <remarks>
	/// For example, if the cells are <c>[0, 1, 27, 28]</c>, all spanned blocks are 0 and 3, so the return mask is <c>0b000001001</c> (i.e. 9).
	/// </remarks>
	public readonly Mask BlockMask
	{
		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		get
		{
			var result = (Mask)0;
			if (this && HousesMap[0]) { result |= 1; }
			if (this && HousesMap[1]) { result |= 2; }
			if (this && HousesMap[2]) { result |= 4; }
			if (this && HousesMap[3]) { result |= 8; }
			if (this && HousesMap[4]) { result |= 16; }
			if (this && HousesMap[5]) { result |= 32; }
			if (this && HousesMap[6]) { result |= 64; }
			if (this && HousesMap[7]) { result |= 128; }
			if (this && HousesMap[8]) { result |= 256; }
			return result;
		}
	}

	/// <summary>
	/// Indicates the mask of row that all cells in this collection spanned.
	/// </summary>
	/// <remarks>
	/// For example, if the cells are <c>[0, 1, 27, 28]</c>, all spanned rows are 0 and 3, so the return mask is <c>0b000001001</c> (i.e. 9).
	/// </remarks>
	public readonly Mask RowMask
	{
		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		get
		{
			var result = (Mask)0;
			if (this && HousesMap[9]) { result |= 1; }
			if (this && HousesMap[10]) { result |= 2; }
			if (this && HousesMap[11]) { result |= 4; }
			if (this && HousesMap[12]) { result |= 8; }
			if (this && HousesMap[13]) { result |= 16; }
			if (this && HousesMap[14]) { result |= 32; }
			if (this && HousesMap[15]) { result |= 64; }
			if (this && HousesMap[16]) { result |= 128; }
			if (this && HousesMap[17]) { result |= 256; }
			return result;
		}
	}

	/// <summary>
	/// Indicates the mask of column that all cells in this collection spanned.
	/// </summary>
	/// <remarks>
	/// For example, if the cells are <c>[0, 1, 27, 28]</c>, all spanned columns are 0 and 1, so the return mask is <c>0b000000011</c> (i.e. 3).
	/// </remarks>
	public readonly Mask ColumnMask
	{
		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		get
		{
			var result = (Mask)0;
			if (this && HousesMap[18]) { result |= 1; }
			if (this && HousesMap[19]) { result |= 2; }
			if (this && HousesMap[20]) { result |= 4; }
			if (this && HousesMap[21]) { result |= 8; }
			if (this && HousesMap[22]) { result |= 16; }
			if (this && HousesMap[23]) { result |= 32; }
			if (this && HousesMap[24]) { result |= 64; }
			if (this && HousesMap[25]) { result |= 128; }
			if (this && HousesMap[26]) { result |= 256; }
			return result;
		}
	}

	/// <summary>
	/// Indicates the shared block.
	/// </summary>
	/// <remarks><b>
	/// Please note that the result value may be invalid if no shared houses can be found.
	/// In such case, the return value will be <see cref="FallbackConstants.@int"/> (32, not -1).
	/// </b></remarks>
	public readonly House SharedBlock => TrailingZeroCount(SharedHouses & Grid.MaxCandidatesMask);

	/// <summary>
	/// Indicates the shared line, i.e. a line of 9 cells that contain all possible cells stored in the current collection.
	/// </summary>
	/// <remarks><b>
	/// Please note that the result value may be invalid if no shared houses can be found.
	/// In such case, the return value will be <see cref="FallbackConstants.@int"/> (32, not -1).
	/// </b></remarks>
	public readonly House SharedLine => TrailingZeroCount(SharedHouses & ~Grid.MaxCandidatesMask);

	/// <summary>
	/// Indicates the first shared house returned.
	/// </summary>
	/// <remarks>
	/// A shared house is a house of 9 cells that contain all possible cells stored in the current collection.
	/// For example, cells <c>r1c25</c> is lying in row 1, the return value will be 9 (index of row 1).
	/// However, <b>if the collection has no cells, the return value will be <see cref="FallbackConstants.@int"/></b> (32, not -1).
	/// </remarks>
	public readonly House FirstSharedHouse => TrailingZeroCount(SharedHouses);

	/// <summary>
	/// Indicates all houses shared. This property is used to check all houses that all cells of this instance shared.
	/// For example, if the cells are <c>[0, 1]</c>, the property <see cref="SharedHouses"/> will return
	/// house indices 0 (block 1) and 9 (row 1); however, if cells span two houses or more (e.g. cells <c>[0, 1, 27]</c>),
	/// this property won't contain any houses.
	/// </summary>
	/// <remarks>
	/// The return value will be a <see cref="HouseMask"/> value indicating each houses. Bits set 1 are shared houses.
	/// </remarks>
	public readonly HouseMask SharedHouses
	{
		get
		{
			var result = 0;
			var tempSpan = SharedHouseConstants.Span;
			for (var i = 0; i < 27; i++)
			{
				if ((_vector & tempSpan[i]) == Vector128<long>.Zero)
				{
					result |= SharedHouseFactorValues[i];
				}
			}
			return result;
		}
	}

	/// <summary>
	/// All houses that the map spanned. This property is used to check all houses that all cells of
	/// this instance spanned. For example, if the cells are <c>[0, 1]</c>, the property
	/// <see cref="Houses"/> will return the house index 0 (block 1), 9 (row 1), 18 (column 1)
	/// and 19 (column 2).
	/// </summary>
	public readonly HouseMask Houses => (HouseMask)BlockMask | RowMask << 9 | ColumnMask << 18;

	/// <summary>
	/// Gets the expanded peers of the current map.
	/// </summary>
	/// <remarks>
	/// An <b>Expanded Peers</b> is a list of cells that contains all peer cells of each cell
	/// appeared in the current collection. For example, if a collection contains cells <c>r1c123</c>,
	/// this collection will be the result of the expression <c>Peer.PeersMap[r1c1] | Peer.PeersMap[r1c2] | Peer.PeersMap[r1c3]</c>,
	/// where the member <c>Peer.PeersMap</c> corresponds to the array <see cref="Peer.PeersMap"/>.
	/// </remarks>
	/// <seealso cref="Peer.PeersMap"/>
	public readonly CellMap ExpandedPeers
	{
		get
		{
			var result = Empty;
			foreach (var cell in Offsets)
			{
				result |= Peer.PeersMap[cell];
			}
			return result;
		}
	}

	/// <inheritdoc/>
	public readonly CellMap PeerIntersection
	{
		get
		{
			var (lowerBits, higherBits, i) = (0L, 0L, 0);
			foreach (var offset in Offsets)
			{
				var (low, high) = (0L, 0L);
				foreach (var peer in Peer.PeersMap[offset])
				{
					(peer / Shifting == 0 ? ref low : ref high) |= 1L << peer % Shifting;
				}

				if (i++ == 0)
				{
					lowerBits = low;
					higherBits = high;
				}
				else
				{
					lowerBits &= low;
					higherBits &= high;
				}
			}
			return CellMap.Create(CV(higherBits, lowerBits));
		}
	}

	/// <summary>
	/// Indicates the cell offsets in this collection.
	/// </summary>
	internal readonly Cell[] Offsets
	{
		get
		{
			if (!this)
			{
				return [];
			}

			var (pos, arr) = (0, new Cell[Count]);
			for (var value = _vector[0]; value != 0; arr[pos++] = TrailingZeroCount(value), value &= value - 1) ;
			for (var value = _vector[1]; value != 0; arr[pos++] = Shifting + TrailingZeroCount(value), value &= value - 1) ;
			return arr;
		}
	}

	/// <inheritdoc/>
	readonly int CellMapBase.Shifting => Shifting;

	/// <inheritdoc/>
	readonly Cell[] CellMapBase.Offsets => Offsets;


	/// <inheritdoc/>
	static Cell CellMapBase.MaxCount => 9 * 9;

	/// <inheritdoc/>
	static ref readonly CellMap CellMapBase.Empty => ref Empty;

	/// <inheritdoc/>
	static ref readonly CellMap CellMapBase.Full => ref Full;


	/// <summary>
	/// Get the offset at the specified position index.
	/// </summary>
	/// <param name="index">The index.</param>
	/// <returns>
	/// The offset at the specified position index. If the value is invalid, the return value will be <c>-1</c>.
	/// </returns>
	public readonly Cell this[Cell index]
	{
		get
		{
			if (!this || index >= Count)
			{
				return -1;
			}

			var (low, high) = (_vector[0], _vector[1]);
			if (Bmi2.X64.IsSupported)
			{
				// https://stackoverflow.com/questions/7669057/find-nth-set-bit-in-an-int
				return TrailingZeroCount(Bmi2.X64.ParallelBitDeposit(1UL << index, (ulong)low)) switch
				{
					var l and not FallbackConstants.@long => l,
					_ => TrailingZeroCount(Bmi2.X64.ParallelBitDeposit(1UL << index - PopCount((ulong)low), (ulong)high)) switch
					{
						var h and not FallbackConstants.@long => h + Shifting,
						_ => -1
					}
				};
			}

			var lowPopCount = PopCount((ulong)low);
			if (index < lowPopCount)
			{
				return low.SetAt(index);
			}

			var highIndex = index - lowPopCount;
			return high.SetAt(highIndex) + Shifting;
		}
	}


	/// <inheritdoc/>
	public readonly void CopyTo(ref Cell sequence, Cell length)
		=> Offsets.AsReadOnlySpan().TryCopyTo(Span.Create(ref sequence, length));

	/// <inheritdoc/>
	public readonly void ForEach(Action<Cell> action)
	{
		foreach (var element in this)
		{
			action(element);
		}
	}

	/// <summary>
	/// Determine whether the map contains the specified offset.
	/// </summary>
	/// <param name="item">The offset.</param>
	/// <returns>A <see cref="bool"/> value indicating that.</returns>
	public readonly bool Contains(Cell item) => (_vector[item < Shifting ? 0 : 1] >>> item % Shifting & 1) != 0;

	/// <inheritdoc/>
	public readonly override bool Equals([NotNullWhen(true)] object? obj) => obj is CellMap comparer && Equals(comparer);

	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	public readonly bool Equals(in CellMap other) => _vector.Equals(other._vector);

	/// <summary>
	/// Indicates whether at least one element satisfies the specified condition.
	/// </summary>
	/// <param name="match">The match method.</param>
	/// <returns>A <see cref="bool"/> result indicating whether at least one element satisfies the specified condition.</returns>
	public readonly bool Exists(Func<Cell, bool> match)
	{
		foreach (var cell in this)
		{
			if (match(cell))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Determine whether all <see cref="Cell"/>s satisfy the specified condition.
	/// </summary>
	/// <param name="match">The match method.</param>
	/// <returns>A <see cref="bool"/> result indicating whether all elements satisfy the specified condition.</returns>
	public readonly bool TrueForAll(Func<Cell, bool> match)
	{
		foreach (var cell in this)
		{
			if (!match(cell))
			{
				return false;
			}
		}
		return true;
	}

	/// <inheritdoc cref="object.GetHashCode"/>
	public readonly override int GetHashCode() => _vector.GetHashCode();

	/// <inheritdoc/>
	public readonly int CompareTo(in CellMap other)
	{
		return Count > other.Count ? 1 : Count < other.Count ? -1 : -Math.Sign($"{b(this)}".CompareTo($"{b(other)}"));


		static string b(in CellMap f) => f.ToString(new BitmapCellMapConverter());
	}

	/// <inheritdoc/>
	public readonly int IndexOf(Cell offset)
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

	/// <inheritdoc cref="object.ToString"/>
	public readonly override string ToString() => ToString(CoordinateConverter.InvariantCulture);

	/// <inheritdoc/>
	public readonly string ToString(CultureInfo culture) => ToString(CoordinateConverter.GetInstance(culture));

	/// <inheritdoc/>
	public readonly string ToString(CoordinateConverter converter) => converter.CellConverter(this);

	/// <inheritdoc cref="CellMapBase.ToString(IValueConverter{CellMap})"/>
	public readonly string ToString(ICellMapConverter converter)
		=> converter.TryFormat(in this, null, out var result) ? result : throw new FormatException();

	/// <inheritdoc cref="CellMapBase.ToString(IValueConverter{CellMap}, IFormatProvider?)"/>
	public readonly string ToString(ICellMapConverter converter, IFormatProvider? formatProvider)
		=> converter.TryFormat(in this, formatProvider, out var result) ? result : throw new FormatException();

	/// <inheritdoc/>
	public readonly Cell[] ToArray() => Offsets[..];

	/// <inheritdoc cref="ToArray"/>
	/// <remarks><b><i>
	/// Although the return value is an array, you cannot modify them to keep data-safety (read-only array).
	/// Returning other types of such value will raise extra conversion, which is slower.
	/// </i></b></remarks>
	public readonly Cell[] ToArrayUnsafe() => Offsets;

	/// <inheritdoc/>
	public readonly AnonymousSpanEnumerator<Cell> GetEnumerator() => new(Offsets);

	/// <inheritdoc/>
	public readonly CellMap Slice(int start, int count)
	{
		var (result, offsets) = (Empty, Offsets);
		for (int i = start, end = start + count; i < end; i++)
		{
			result += offsets[i];
		}
		return result;
	}

	/// <summary>
	/// Finds the first cell that satisfies the specified condition.
	/// </summary>
	/// <param name="match">The condition to be used.</param>
	/// <returns>The first found cell or -1 if none found.</returns>
	public readonly Cell First(Func<Cell, bool> match)
	{
		foreach (var cell in Offsets)
		{
			if (match(cell))
			{
				return cell;
			}
		}
		return -1;
	}

	/// <summary>
	/// Finds the first cell that satisfies the specified condition.
	/// </summary>
	/// <param name="grid">The grid to be used.</param>
	/// <param name="match">The condition to be used.</param>
	/// <returns>The first found cell or -1 if none found.</returns>
	public readonly Cell First(in Grid grid, BitStateMapPredicate<CellMap, Cell> match)
	{
		foreach (var cell in Offsets)
		{
			if (match(cell, grid))
			{
				return cell;
			}
		}
		return -1;
	}

	/// <summary>
	/// Projects each element in the current instance into the target-typed <typeparamref name="TResult"/> array,
	/// using the specified function to convert.
	/// </summary>
	/// <typeparam name="TResult">The type of target value.</typeparam>
	/// <param name="selector">The selector.</param>
	/// <returns>An array of <typeparamref name="TResult"/> elements.</returns>
	public readonly ReadOnlySpan<TResult> Select<TResult>(Func<Cell, TResult> selector)
	{
		var (result, i) = (new TResult[Count], 0);
		foreach (var cell in Offsets)
		{
			result[i++] = selector(cell);
		}
		return result;
	}

	/// <summary>
	/// Filters a <see cref="CellMap"/> collection based on a predicate.
	/// </summary>
	/// <param name="predicate">A function to test each element for a condition.</param>
	/// <returns>
	/// A <see cref="CellMap"/> that contains elements from the input <see cref="CellMap"/> satisfying the condition.
	/// </returns>
	public readonly CellMap Where(Func<Cell, bool> predicate)
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

	/// <summary>
	/// Groups the elements of a sequence according to a specified key selector function.
	/// </summary>
	/// <typeparam name="TKey">
	/// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" path="/typeparam[@name='TKey']"/>
	/// </typeparam>
	/// <param name="keySelector">
	/// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" path="/param[@name='keySelector']"/>
	/// </param>
	/// <returns>
	/// A list of <see cref="BitStateMapGrouping{TMap, TElement, TKey}"/> instances where each value object contains a sequence of objects and a key.
	/// </returns>
	/// <seealso cref="BitStateMapGrouping{TMap, TElement, TKey}"/>
	public readonly ReadOnlySpan<BitStateMapGrouping<CellMap, Cell, TKey>> GroupBy<TKey>(Func<Cell, TKey> keySelector)
		where TKey : notnull
	{
		var dictionary = new Dictionary<TKey, CellMap>();
		foreach (var cell in this)
		{
			var key = keySelector(cell);
			if (!dictionary.TryAdd(key, cell.AsCellMap()))
			{
				dictionary.GetValueRef(key) += cell;
			}
		}

		var result = new BitStateMapGrouping<CellMap, Cell, TKey>[dictionary.Count];
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
	/// Projects each cell (of type <see cref="Cell"/>) of a <see cref="CellMap"/> to a mask (of type <see cref="Mask"/>),
	/// flattens the resulting sequence into one sequence, and invokes a result selector function on each element therein.
	/// </summary>
	/// <typeparam name="TResult">The type of the elements of the resulting sequence.</typeparam>
	/// <param name="collectionSelector">A transform function to apply to each element of the input <see cref="CellMap"/>.</param>
	/// <param name="resultSelector">A transform function to apply to each element of the intermediate mask (of type <see cref="Mask"/>).</param>
	/// <returns>
	/// A <see cref="ReadOnlySpan{T}"/> of <typeparamref name="TResult"/> whose elements are the result
	/// of invoking the one-to-many transform function <paramref name="collectionSelector"/> on each element
	/// of the current instance and then mapping each of those sequences and their corresponding source element to a result element.
	/// </returns>
	public readonly ReadOnlySpan<TResult> SelectMany<TResult>(Func<Cell, Mask> collectionSelector, Func<Cell, Digit, TResult> resultSelector)
	{
		var result = new List<TResult>(Count << 1);
		foreach (var cell in this)
		{
			foreach (var digit in collectionSelector(cell))
			{
				result.AddRef(resultSelector(cell, digit));
			}
		}
		return result.AsSpan();
	}

	/// <summary>
	/// Add a new <see cref="Cell"/> into the collection.
	/// </summary>
	/// <param name="item">The offset to be added.</param>
	/// <returns>A <see cref="bool"/> value indicating whether the collection has already contained the offset.</returns>
	public bool Add(Cell item)
	{
		if (Contains(item))
		{
			return false;
		}

		_vector = _vector.WithElement(item / Shifting, _vector[item / Shifting] | 1L << item % Shifting);
		return true;
	}

	/// <inheritdoc/>
	public int AddRange(ReadOnlySpan<Cell> offsets)
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
	public bool Remove(Cell item)
	{
		if (!Contains(item))
		{
			return false;
		}

		_vector = _vector.WithElement(item / Shifting, _vector[item / Shifting] & ~(1L << item % Shifting));
		return true;
	}

	/// <inheritdoc/>
	public int RemoveRange(ReadOnlySpan<Cell> offsets)
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
	public void Toggle(Cell offset)
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
	readonly bool IEquatable<CellMap>.Equals(CellMap other) => Equals(other);

	/// <inheritdoc/>
	readonly bool IAnyAllMethod<CellMap, Cell>.Any() => Count != 0;

	/// <inheritdoc/>
	readonly bool IAnyAllMethod<CellMap, Cell>.Any(Func<Cell, bool> predicate) => Exists(predicate);

	/// <inheritdoc/>
	readonly bool IAnyAllMethod<CellMap, Cell>.All(Func<Cell, bool> predicate) => TrueForAll(predicate);

	/// <inheritdoc/>
	readonly string CellMapBase.ToString(IValueConverter<CellMap> converter)
		=> converter.TryFormat(in this, null, out var result) ? result : throw new FormatException();

	/// <inheritdoc/>
	readonly string CellMapBase.ToString(IValueConverter<CellMap> converter, IFormatProvider? formatProvider)
		=> converter.TryFormat(in this, formatProvider, out var result) ? result : throw new FormatException();

	/// <inheritdoc/>
	readonly Cell IFirstLastMethod<CellMap, Cell>.First() => this[0];

	/// <inheritdoc/>
	readonly Cell IFirstLastMethod<CellMap, Cell>.First(Func<Cell, bool> predicate) => First(predicate);

	/// <inheritdoc/>
	readonly IEnumerable<Cell> IWhereMethod<CellMap, Cell>.Where(Func<Cell, bool> predicate) => Where(predicate);

	/// <inheritdoc/>
	readonly IEnumerable<IGrouping<TKey, Cell>> IGroupByMethod<CellMap, Cell>.GroupBy<TKey>(Func<Cell, TKey> keySelector)
		=> GroupBy(keySelector).ToArray().Select(static element => (IGrouping<TKey, Cell>)element);

	/// <inheritdoc/>
	readonly IEnumerable<TResult> ISelectMethod<CellMap, Cell>.Select<TResult>(Func<Cell, TResult> selector)
		=> Select(selector).ToArray();


	/// <inheritdoc/>
	public static bool TryParse(string str, out CellMap result)
		=> TryParse(str, CoordinateParser.InvariantCulture, out result);

	/// <inheritdoc/>
	public static bool TryParse(string str, CultureInfo culture, out CellMap result)
		=> TryParse(str, CoordinateParser.GetInstance(culture), out result);

	/// <inheritdoc/>
	public static bool TryParse(string str, CoordinateParser converter, out CellMap result)
	{
		try
		{
			result = converter.CellParser(str);
			return true;
		}
		catch (FormatException)
		{
			result = Empty;
			return false;
		}
	}

	/// <inheritdoc/>
	public static bool TryParse(string str, IValueConverter<CellMap> converter, out CellMap result)
		=> converter.TryParse(str, null, out result);

	/// <inheritdoc/>
	public static bool TryParse(string str, IValueConverter<CellMap> converter, IFormatProvider? formatProvider, out CellMap result)
		=> converter.TryParse(str, formatProvider, out result);

	/// <inheritdoc/>
	public static bool TryParse(ReadOnlySpan<char> str, out CellMap result)
		=> TryParse(str, CoordinateParser.InvariantCulture, out result);

	/// <inheritdoc/>
	public static bool TryParse(ReadOnlySpan<char> str, CultureInfo culture, out CellMap result)
		=> TryParse(str, CoordinateParser.GetInstance(culture), out result);

	/// <inheritdoc/>
	public static bool TryParse(ReadOnlySpan<char> str, CoordinateParser converter, out CellMap result)
		=> TryParse(str.ToString(), converter, out result);

	/// <inheritdoc/>
	public static bool TryParse(ReadOnlySpan<char> str, IValueConverter<CellMap> converter, out CellMap result)
		=> converter.TryParse(str, null, out result);

	/// <inheritdoc/>
	public static bool TryParse(ReadOnlySpan<char> str, IValueConverter<CellMap> converter, IFormatProvider? formatProvider, out CellMap result)
		=> converter.TryParse(str, formatProvider, out result);

	/// <summary>
	/// Creates a <see cref="CellMap"/> instance via the specified cells.
	/// </summary>
	/// <param name="cells">The cells.</param>
	/// <returns>A <see cref="CellMap"/> instance.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[OverloadResolutionPriority(-1)]
	public static CellMap Create(ReadOnlySpan<Cell> cells)
	{
		if (cells.IsEmpty)
		{
			return default;
		}

		var result = default(CellMap);
		foreach (var cell in cells)
		{
			result += cell;
		}
		return result;
	}

	/// <summary>
	/// Parses the current string into target instance.
	/// </summary>
	/// <param name="str">The string.</param>
	/// <returns>The instance.</returns>
	/// <exception cref="FormatException">Throws when any invalid characters encountered.</exception>
	public static CellMap Parse(string str)
	{
		foreach (var parser in
			from element in CoordinateType.AllValues
			let parser = element.Parser
			where parser is not null
			select parser)
		{
			if (parser.CellParser(str) is { Count: not 0 } result)
			{
				return result;
			}
		}
		throw new FormatException(SR.ExceptionMessage("StringValueInvalidToBeParsed"));
	}

	/// <inheritdoc/>
	public static CellMap Parse(string str, CultureInfo culture) => CoordinateParser.GetInstance(culture).CellParser(str);

	/// <inheritdoc/>
	public static CellMap Parse(string str, CoordinateParser converter) => converter.CellParser(str);

	/// <inheritdoc/>
	public static CellMap Parse(string str, IValueConverter<CellMap> converter)
		=> converter.TryParse(str, null, out var result) ? result : throw new FormatException();

	/// <inheritdoc/>
	public static CellMap Parse(string str, IValueConverter<CellMap> converter, IFormatProvider? formatProvider)
		=> converter.TryParse(str, formatProvider, out var result) ? result : throw new FormatException();

	/// <inheritdoc/>
	public static CellMap Parse(ReadOnlySpan<char> str) => Parse(str.ToString());

	/// <inheritdoc/>
	public static CellMap Parse(ReadOnlySpan<char> str, CultureInfo culture) => Parse(str.ToString(), culture);

	/// <inheritdoc/>
	public static CellMap Parse(ReadOnlySpan<char> str, CoordinateParser converter)
		=> converter.CellParser(str.ToString());

	/// <inheritdoc/>
	public static CellMap Parse(ReadOnlySpan<char> str, IValueConverter<CellMap> converter)
		=> converter.TryParse(str, null, out var result) ? result : throw new FormatException();

	/// <inheritdoc/>
	public static CellMap Parse(ReadOnlySpan<char> str, IValueConverter<CellMap> converter, IFormatProvider? formatProvider)
		=> converter.TryParse(str, formatProvider, out var result) ? result : throw new FormatException();

	/// <summary>
	/// Creates a <see cref="Vector128{T}"/> of <see cref="long"/> instance.
	/// </summary>
	/// <param name="e1">The higher 64 bits.</param>
	/// <param name="e0">The lower 64 bits.</param>
	/// <returns>A <see cref="Vector128{T}"/> of <see cref="long"/> instance.</returns>
	/// <remarks><b>
	/// This method will only be used in constant creation, just for readability on binary integers' positions.
	/// </b></remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static Vector128<long> CV(long e1, long e0) => Vector128.Create(e0, e1);


	/// <inheritdoc/>
	public void operator &=(in CellMap value) => _vector &= value._vector;

	/// <inheritdoc/>
	public void operator |=(in CellMap value) => _vector |= value._vector;

	/// <inheritdoc/>
	public void operator ^=(in CellMap value) => _vector ^= value._vector;

	/// <inheritdoc/>
	public void operator +=(Cell offset)
		=> _vector = _vector.WithElement(offset / Shifting, _vector[offset / Shifting] | 1L << offset % Shifting);

	/// <inheritdoc/>
	public void operator -=(Cell offset)
		=> _vector = _vector.WithElement(offset / Shifting, _vector[offset / Shifting] & ~(1L << offset % Shifting));


	/// <inheritdoc/>
	public static bool operator !(in CellMap value) => value.Count == 0;

	/// <inheritdoc/>
	public static bool operator true(in CellMap value) => value.Count != 0;

	/// <inheritdoc/>
	public static bool operator false(in CellMap value) => value.Count == 0;

	/// <inheritdoc/>
	public static bool operator ==(in CellMap left, in CellMap right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(in CellMap left, in CellMap right) => !(left == right);

	/// <inheritdoc/>
	public static bool operator >(in CellMap left, in CellMap right) => left.CompareTo(right) > 0;

	/// <inheritdoc/>
	public static bool operator <(in CellMap left, in CellMap right) => left.CompareTo(right) < 0;

	/// <inheritdoc/>
	public static bool operator >=(in CellMap left, in CellMap right) => left.CompareTo(right) >= 0;

	/// <inheritdoc/>
	public static bool operator <=(in CellMap left, in CellMap right) => left.CompareTo(right) <= 0;

	/// <inheritdoc/>
	public static CellMap operator ~(in CellMap offsets) => CellMap.Create(~offsets._vector & BitwiseNotConstant);

	/// <inheritdoc/>
	public static CellMap operator +(in CellMap collection, Cell offset)
	{
		var result = collection;
		result += offset;
		return result;
	}

	/// <inheritdoc/>
	public static CellMap operator -(in CellMap collection, Cell offset)
	{
		var result = collection;
		result -= offset;
		return result;
	}

	/// <inheritdoc/>
	public static CellMap operator &(in CellMap left, in CellMap right) => CellMap.Create(left._vector & right._vector);

	/// <inheritdoc/>
	public static CellMap operator |(in CellMap left, in CellMap right) => CellMap.Create(left._vector | right._vector);

	/// <inheritdoc/>
	public static CellMap operator ^(in CellMap left, in CellMap right) => CellMap.Create(left._vector ^ right._vector);

	/// <inheritdoc/>
	public static unsafe ReadOnlySpan<CellMap> operator &(in CellMap map, int subsetSize)
	{
		if (subsetSize == 0 || subsetSize > map.Count)
		{
			return [];
		}

		if (subsetSize == map.Count)
		{
			return (CellMap[])[map];
		}

		var n = map.Count;
		var buffer = stackalloc int[subsetSize];
		if (n <= CellMapBase.MaxLimit && subsetSize <= CellMapBase.MaxLimit)
		{
			// Optimization: Use table to get the total number of result elements.
			var totalIndex = 0;
			var result = new CellMap[Combinatorics.PascalTriangle[n - 1][subsetSize - 1]];
			e(result, subsetSize, n, subsetSize, map.Offsets, (r, c) => r[totalIndex++] = c.AsCellMap());
			return result;
		}
		else
		{
			if (n > CellMapBase.MaxLimit && subsetSize > CellMapBase.MaxLimit)
			{
				throw new NotSupportedException(SR.ExceptionMessage("SubsetsExceeded"));
			}
			var result = new List<CellMap>();
			e(result, subsetSize, n, subsetSize, map.Offsets, static (r, c) => r.AddRef(c.AsCellMap()));
			return result.AsSpan();
		}


		void e<T>(T result, int size, int last, int index, Cell[] offsets, Action<T, ReadOnlySpan<Cell>> addingAction)
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
					var temp = new Cell[size];
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
	public static ReadOnlySpan<CellMap> operator |(in CellMap map, int subsetSize)
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

		var result = new List<CellMap>(desiredSize);
		for (var i = 1; i <= length; i++)
		{
			result.AddRangeRef(map & i);
		}
		return result.AsSpan();
	}

	/// <inheritdoc/>
	public static ReadOnlySpan<CellMap> operator |(in CellMap map, Range subsetSizeRange)
	{
		if (!map)
		{
			return [];
		}

		var (s, e) = subsetSizeRange;
		var result = new List<CellMap>();
		for (var i = s.GetOffset(map.Count); i <= e.GetOffset(map.Count); i++)
		{
			result.AddRangeRef(map & i);
		}
		return result.AsSpan();
	}

	/// <summary>
	/// Expands the current <see cref="CellMap"/> instance, inserting into a <see cref="CandidateMap"/> instance by specified digit.
	/// </summary>
	/// <param name="cells">The cells to be checked.</param>
	/// <param name="digit">The digit to be checked.</param>
	/// <returns>A <see cref="CandidateMap"/> instance.</returns>
	public static CandidateMap operator *(in CellMap cells, Digit digit)
	{
		var result = CandidateMap.Empty;
		foreach (var cell in cells.Offsets)
		{
			result += cell * 9 + digit;
		}
		return result;
	}

	/// <summary>
	/// Reduces the <see cref="CellMap"/> instances, only checks for cells in the specified cells, and merge into a <see cref="Mask"/> value.
	/// </summary>
	/// <param name="cells">The cells to be checked.</param>
	/// <param name="house">The house to be checked.</param>
	/// <returns>A <see cref="Mask"/> instance.</returns>
	public static Mask operator /(in CellMap cells, House house)
	{
		var (result, i) = ((Mask)0, 0);
		foreach (var cell in HousesCells[house])
		{
			if (cells.Contains(cell))
			{
				result |= (Mask)(1 << i);
			}
			i++;
		}
		return result;
	}
}

/// <summary>
/// Represents JSON serialization rules on type <see cref="CellMap"/>.
/// </summary>
file sealed class Converter : JsonConverter<CellMap>
{
	/// <inheritdoc/>
	public override bool HandleNull => false;


	/// <inheritdoc/>
	[SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "<Pending>")]
	public override CellMap Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> new(JsonSerializer.Deserialize<string[]>(ref reader, options)!);

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, CellMap value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		foreach (var element in value.StringChunks)
		{
			writer.WriteStringValue(element);
		}
		writer.WriteEndArray();
	}
}
