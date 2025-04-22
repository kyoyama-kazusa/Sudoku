namespace Sudoku.Concepts;

using CellMapBase = ICellMapOrCandidateMap<CellMap, Cell>;

/// <summary>
/// Encapsulates a binary series of cell state table.
/// </summary>
/// <remarks>
/// <include file="../../global-doc-comments.xml" path="/g/large-structure"/>
/// </remarks>
[JsonConverter(typeof(Converter))]
[StructLayout(LayoutKind.Auto)]
[CollectionBuilder(typeof(CellMap), nameof(Create))]
[DebuggerStepThrough]
[TypeImpl(
	TypeImplFlags.AllObjectMethods | TypeImplFlags.AllEqualityComparisonOperators | TypeImplFlags.TrueAndFalseOperators
		| TypeImplFlags.LogicalNotOperator | TypeImplFlags.Equatable,
	IsLargeStructure = true)]
public partial struct CellMap : CellMapBase
{
	/// <inheritdoc cref="CellMapBase.Shifting"/>
	private const int Shifting = 41;


	/// <inheritdoc cref="CellMapBase.Empty"/>
	public static readonly CellMap Empty = [];

	/// <inheritdoc cref="CellMapBase.Full"/>
	public static readonly CellMap Full = ~default(CellMap);

	/// <summary>
	/// Indicates the constant that will be used on bitwise not operation.
	/// </summary>
	private static readonly Vector128<ulong> BitwiseNotConstant = CV(0xFF_FFFF_FFFFUL, 0x1FF_FFFF_FFFFUL);

	/// <summary>
	/// Indicates the <see cref="Vector128{T}"/> instances to be used for checking shared houses.
	/// </summary>
	/// <seealso cref="Vector128{T}"/>
	private static readonly ReadOnlyMemory<Vector128<ulong>> SharedHouseConstants = (Vector128<ulong>[])[
		CV(~0b_000000000_000000000_000000000_000000000_0000UL, ~0b_00000_000000000_000000111_000000111_000000111UL),
		CV(~0b_000000000_000000000_000000000_000000000_0000UL, ~0b_00000_000000000_000111000_000111000_000111000UL),
		CV(~0b_000000000_000000000_000000000_000000000_0000UL, ~0b_00000_000000000_111000000_111000000_111000000UL),
		CV(~0b_000000000_000000000_000000000_000000111_0000UL, ~0b_00111_000000111_000000000_000000000_000000000UL),
		CV(~0b_000000000_000000000_000000000_000111000_0001UL, ~0b_11000_000111000_000000000_000000000_000000000UL),
		CV(~0b_000000000_000000000_000000000_111000000_1110UL, ~0b_00000_111000000_000000000_000000000_000000000UL),
		CV(~0b_000000111_000000111_000000111_000000000_0000UL, ~0b_00000_000000000_000000000_000000000_000000000UL),
		CV(~0b_000111000_000111000_000111000_000000000_0000UL, ~0b_00000_000000000_000000000_000000000_000000000UL),
		CV(~0b_111000000_111000000_111000000_000000000_0000UL, ~0b_00000_000000000_000000000_000000000_000000000UL),
		CV(~0b_000000000_000000000_000000000_000000000_0000UL, ~0b_00000_000000000_000000000_000000000_111111111UL),
		CV(~0b_000000000_000000000_000000000_000000000_0000UL, ~0b_00000_000000000_000000000_111111111_000000000UL),
		CV(~0b_000000000_000000000_000000000_000000000_0000UL, ~0b_00000_000000000_111111111_000000000_000000000UL),
		CV(~0b_000000000_000000000_000000000_000000000_0000UL, ~0b_00000_111111111_000000000_000000000_000000000UL),
		CV(~0b_000000000_000000000_000000000_000000000_1111UL, ~0b_11111_000000000_000000000_000000000_000000000UL),
		CV(~0b_000000000_000000000_000000000_111111111_0000UL, ~0b_00000_000000000_000000000_000000000_000000000UL),
		CV(~0b_000000000_000000000_111111111_000000000_0000UL, ~0b_00000_000000000_000000000_000000000_000000000UL),
		CV(~0b_000000000_111111111_000000000_000000000_0000UL, ~0b_00000_000000000_000000000_000000000_000000000UL),
		CV(~0b_111111111_000000000_000000000_000000000_0000UL, ~0b_00000_000000000_000000000_000000000_000000000UL),
		CV(~0b_000000001_000000001_000000001_000000001_0000UL, ~0b_00001_000000001_000000001_000000001_000000001UL),
		CV(~0b_000000010_000000010_000000010_000000010_0000UL, ~0b_00010_000000010_000000010_000000010_000000010UL),
		CV(~0b_000000100_000000100_000000100_000000100_0000UL, ~0b_00100_000000100_000000100_000000100_000000100UL),
		CV(~0b_000001000_000001000_000001000_000001000_0000UL, ~0b_01000_000001000_000001000_000001000_000001000UL),
		CV(~0b_000010000_000010000_000010000_000010000_0000UL, ~0b_10000_000010000_000010000_000010000_000010000UL),
		CV(~0b_000100000_000100000_000100000_000100000_0001UL, ~0b_00000_000100000_000100000_000100000_000100000UL),
		CV(~0b_001000000_001000000_001000000_001000000_0010UL, ~0b_00000_001000000_001000000_001000000_001000000UL),
		CV(~0b_010000000_010000000_010000000_010000000_0100UL, ~0b_00000_010000000_010000000_010000000_010000000UL),
		CV(~0b_100000000_100000000_100000000_100000000_1000UL, ~0b_00000_100000000_100000000_100000000_100000000UL)
	];

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
	[HashCodeMember]
	[EquatableMember]
	private Vector128<ulong> _vector;


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
	public readonly bool IsInIntersection
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => Count == 1 || Count <= 3 && BitOperations.PopCount(SharedHouses) == 2;
	}

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
				case 2: { return FirstSharedHouse != 32; }
				default:
				{
					foreach (ref readonly var pair in this & 2)
					{
						if (pair.FirstSharedHouse != 32)
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
	public readonly int Count => BitOperations.PopCount(_vector[1]) + BitOperations.PopCount(_vector[0]);

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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
	public readonly House SharedBlock
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => BitOperations.TrailingZeroCount(SharedHouses & Grid.MaxCandidatesMask);
	}

	/// <summary>
	/// Indicates the shared line, i.e. a line of 9 cells that contain all possible cells stored in the current collection.
	/// </summary>
	/// <remarks><b>
	/// Please note that the result value may be invalid if no shared houses can be found.
	/// In such case, the return value will be 32 (instead of -1, intuitive value).
	/// </b></remarks>
	public readonly House SharedLine
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => BitOperations.TrailingZeroCount(SharedHouses & ~Grid.MaxCandidatesMask);
	}

	/// <summary>
	/// Indicates the first shared house returned.
	/// </summary>
	/// <remarks>
	/// A shared house is a house of 9 cells that contain all possible cells stored in the current collection.
	/// For example, cells <c>r1c25</c> is lying in row 1, the return value will be 9 (index of row 1).
	/// However, <b>if the collection has no cells, the return value will be 32</b> (not -1).
	/// </remarks>
	public readonly House FirstSharedHouse
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => BitOperations.TrailingZeroCount(SharedHouses);
	}

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
				if ((_vector & tempSpan[i]) == Vector128<ulong>.Zero)
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
	public readonly HouseMask Houses
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => (HouseMask)BlockMask | RowMask << 9 | ColumnMask << 18;
	}

	/// <summary>
	/// Try to get the symmetric type of the pattern.
	/// </summary>
	public readonly SymmetricType Symmetry
	{
		get
		{
			foreach (var symmetry in SymmetricType.Values[1..].EnumerateReversely())
			{
				var isThisSymmetry = true;
				foreach (var cell in this)
				{
					var symmetricCells = symmetry.GetCells(cell);
					if ((this & symmetricCells) != symmetricCells)
					{
						isThisSymmetry = false;
						break;
					}
				}
				if (!isThisSymmetry)
				{
					continue;
				}
				return symmetry;
			}
			return SymmetricType.None;
		}
	}

	/// <summary>
	/// Gets the expanded peers of the current map.
	/// </summary>
	/// <remarks>
	/// An <b>Expanded Peers</b> is a list of cells that contains all peer cells of each cell
	/// appeared in the current collection. For example, if a collection contains cells <c>r1c123</c>,
	/// this collection will be the result of the expression <c>PeersMap[r1c1] | PeersMap[r1c2] | PeersMap[r1c3]</c>,
	/// where the member <c>PeersMap</c> corresponds to the array <see cref="PeersMap"/>.
	/// </remarks>
	/// <seealso cref="PeersMap"/>
	public readonly CellMap ExpandedPeers
	{
		get
		{
			var result = Empty;
			foreach (var cell in Offsets)
			{
				result |= PeersMap[cell];
			}
			return result;
		}
	}

	/// <inheritdoc/>
	public readonly CellMap PeerIntersection
	{
		get
		{
			var (lowerBits, higherBits, i) = (0UL, 0UL, 0);
			foreach (var offset in Offsets)
			{
				var (low, high) = (0UL, 0UL);
				foreach (var peer in PeersMap[offset])
				{
					(peer / Shifting == 0 ? ref low : ref high) |= 1UL << peer % Shifting;
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

			var vector = CV(higherBits, lowerBits);
			return CreateByVector(vector);
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
			for (
				var value = _vector[0];
				value != 0;
				arr[pos++] = BitOperations.TrailingZeroCount(value), value &= value - 1
			) ;
			for (
				var value = _vector[1];
				value != 0;
				arr[pos++] = Shifting + BitOperations.TrailingZeroCount(value), value &= value - 1
			) ;
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
				return BitOperations.TrailingZeroCount(Bmi2.X64.ParallelBitDeposit(1UL << index, low)) switch
				{
					var l and not 64 => l,
					_ => BitOperations.TrailingZeroCount(Bmi2.X64.ParallelBitDeposit(1UL << index - BitOperations.PopCount(low), high)) switch
					{
						var h and not 64 => h + Shifting,
						_ => -1
					}
				};
			}

			return BitOperations.PopCount(low) is var popCountLow && popCountLow == index
				? 63 - BitOperations.LeadingZeroCount(low)
				: popCountLow > index
					? low.SetAt(index)
					: high.SetAt(index - popCountLow) is var z and not 64 ? z + Shifting : -1;
		}
	}


	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly void CopyTo(ref Cell sequence, Cell length)
		=> Offsets.AsReadOnlySpan().TryCopyTo(@ref.AsSpan(ref sequence, length));

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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool Contains(Cell item) => (_vector[item < Shifting ? 0 : 1] >> item % Shifting & 1) != 0;

	/// <inheritdoc cref="ISpanFormattable.TryFormat(Span{char}, out int, ReadOnlySpan{char}, IFormatProvider?)"/>
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

	/// <inheritdoc cref="IUtf8SpanFormattable.TryFormat(Span{byte}, out int, ReadOnlySpan{char}, IFormatProvider?)"/>
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
	public readonly int CompareTo(in CellMap other)
	{
		return Count > other.Count ? 1 : Count < other.Count ? -1 : -Math.Sign($"{b(this)}".CompareTo($"{b(other)}"));


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static string b(in CellMap f) => f.ToString(new BitmapCellMapFormatInfo());
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

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly string ToString(IFormatProvider? formatProvider)
		=> formatProvider switch
		{
			CellMapFormatInfo i => i.FormatCore(this),
			_ => CoordinateConverter.GetInstance(formatProvider).CellConverter(this)
		};

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly Cell[] ToArray() => Offsets[..];

	/// <inheritdoc cref="ToArray"/>
	/// <remarks><b><i>
	/// Although the return value is an array, you cannot modify them to keep data-safety (read-only array).
	/// Returning other types of such value will raise extra conversion, which is slower.
	/// </i></b></remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly Cell[] ToArrayUnsafe() => Offsets;

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly AnonymousSpanEnumerator<Cell> GetEnumerator() => new(Offsets);

	/// <inheritdoc/>
	public readonly CellMap Slice(int start, int count)
	{
		var (result, offsets) = (Empty, Offsets);
		for (int i = start, end = start + count; i < end; i++)
		{
			result.Add(offsets[i]);
		}
		return result;
	}

	/// <summary>
	/// Add a new <see cref="Cell"/> into the collection.
	/// </summary>
	/// <param name="item">The offset to be added.</param>
	/// <returns>A <see cref="bool"/> value indicating whether the collection has already contained the offset.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Add(Cell item)
	{
		if (Contains(item))
		{
			return false;
		}

		_vector = _vector.WithElement(item / Shifting, _vector[item / Shifting] | 1UL << item % Shifting);
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Remove(Cell item)
	{
		if (!Contains(item))
		{
			return false;
		}

		_vector = _vector.WithElement(item / Shifting, _vector[item / Shifting] & ~(1UL << item % Shifting));
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Toggle(Cell offset) => _ = Contains(offset) ? Remove(offset) : Add(offset);

	/// <summary>
	/// Remove all elements stored in the current collection, and set the property <see cref="Count"/> to zero.
	/// </summary>
	/// <seealso cref="Count"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Clear() => this = Empty;

	/// <inheritdoc/>
	readonly bool IAnyAllMethod<CellMap, Cell>.Any() => Count != 0;

	/// <inheritdoc/>
	readonly bool IAnyAllMethod<CellMap, Cell>.Any(Func<Cell, bool> predicate) => this.Any(predicate);

	/// <inheritdoc/>
	readonly bool IAnyAllMethod<CellMap, Cell>.All(Func<Cell, bool> predicate) => this.All(predicate);

	/// <inheritdoc/>
	readonly string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(formatProvider);

	/// <inheritdoc/>
	readonly Cell IFirstLastMethod<CellMap, Cell>.First() => this[0];

	/// <inheritdoc/>
	readonly Cell IFirstLastMethod<CellMap, Cell>.First(Func<Cell, bool> predicate) => this.First(predicate);

	/// <inheritdoc/>
	readonly IEnumerable<Cell> IWhereMethod<CellMap, Cell>.Where(Func<Cell, bool> predicate) => this.Where(predicate);

	/// <inheritdoc/>
	readonly IEnumerable<IGrouping<TKey, Cell>> IGroupByMethod<CellMap, Cell>.GroupBy<TKey>(Func<Cell, TKey> keySelector)
		=> this.GroupBy(keySelector).ToArray().Select(static element => (IGrouping<TKey, Cell>)element);

	/// <inheritdoc/>
	readonly IEnumerable<TResult> ISelectMethod<CellMap, Cell>.Select<TResult>(Func<Cell, TResult> selector) => this.Select(selector).ToArray();


	/// <inheritdoc cref="IParsable{TSelf}.TryParse(string, IFormatProvider?, out TSelf)"/>
	public static bool TryParse(string str, out CellMap result)
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
	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out CellMap result)
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

	/// <inheritdoc cref="TryParse(ReadOnlySpan{char}, IFormatProvider?, out CellMap)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryParse(ReadOnlySpan<char> s, out CellMap result) => TryParse(s, null, out result);

	/// <inheritdoc cref="ISpanParsable{TSelf}.TryParse(ReadOnlySpan{char}, IFormatProvider?, out TSelf)"/>
	public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out CellMap result)
	{
		try
		{
			result = Parse(s, provider);
			return true;
		}
		catch (FormatException)
		{
			result = Empty;
			return false;
		}
	}

	/// <summary>
	/// Creates a <see cref="CellMap"/> instance via the specified cells.
	/// </summary>
	/// <param name="cells">The cells.</param>
	/// <returns>A <see cref="CellMap"/> instance.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static CellMap Create(ReadOnlySpan<Cell> cells)
	{
		if (cells.IsEmpty)
		{
			return default;
		}

		var result = default(CellMap);
		foreach (var cell in cells)
		{
			result.Add(cell);
		}
		return result;
	}

	/// <summary>
	/// Initializes an instance with two binary values.
	/// </summary>
	/// <param name="high">Higher 40 bits.</param>
	/// <param name="low">Lower 41 bits.</param>
	/// <returns>The result instance created.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CellMap CreateByBits(ulong high, ulong low)
	{
		CellMap result;
		result._vector = CV(high, low);
		return result;
	}

	/// <summary>
	/// Initializes an instance with three binary values.
	/// </summary>
	/// <param name="high">Higher 27 bits.</param>
	/// <param name="mid">Medium 27 bits.</param>
	/// <param name="low">Lower 27 bits.</param>
	/// <returns>The result instance created.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CellMap CreateByBits(int high, int mid, int low)
		=> CreateByBits(
			((ulong)high & 0x7FFFFFFUL) << 13 | (ulong)mid >> 14 & 0x1FFFUL,
			((ulong)mid & 0x3FFFL) << 27 | (ulong)low & 0x7FFFFFFUL
		);

	/// <summary>
	/// Initializes an instance with a <see cref="Vector128{T}"/> of <see cref="long"/>.
	/// </summary>
	/// <param name="vector">Two bits, represented as high 41 and low 40 bits.</param>
	/// <returns>A <see cref="CellMap"/> instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CellMap CreateByVector(Vector128<ulong> vector)
	{
		CellMap result;
		result._vector = vector;
		return result;
	}

	/// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
	public static CellMap Parse(string str)
	{
		foreach (var parser in
			from element in CoordinateType.Values
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CellMap Parse(string s, IFormatProvider? provider)
		=> provider switch
		{
			CellMapFormatInfo i => i.ParseCore(s),
			_ => CoordinateParser.GetInstance(provider).CellParser(s)
		};

	/// <inheritdoc cref="Parse(ReadOnlySpan{char}, IFormatProvider?)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CellMap Parse(ReadOnlySpan<char> s) => Parse(s, null);

	/// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CellMap Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s.ToString(), provider);

	/// <summary>
	/// Creates a <see cref="Vector128{T}"/> of <see cref="ulong"/> instance.
	/// </summary>
	/// <param name="e1">The higher 64 bits.</param>
	/// <param name="e0">The lower 64 bits.</param>
	/// <returns>A <see cref="Vector128{T}"/> of <see cref="ulong"/> instance.</returns>
	/// <remarks><b>
	/// This method will only be used in constant creation, just for readability on binary integers' positions.
	/// </b></remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[DebuggerStepThrough]
	[EditorBrowsable(EditorBrowsableState.Never)]
	private static Vector128<ulong> CV(ulong e1, ulong e0) => Vector128.Create(e0, e1);


	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CellMap operator ~(in CellMap offsets) => CreateByVector(~offsets._vector & BitwiseNotConstant);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CellMap operator +(in CellMap collection, Cell offset)
	{
		var result = collection;
		result.Add(offset);
		return result;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CellMap operator -(in CellMap collection, Cell offset)
	{
		var result = collection;
		result.Remove(offset);
		return result;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CellMap operator %(in CellMap @base, in CellMap template) => (@base & template).PeerIntersection & template;

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CellMap operator &(in CellMap left, in CellMap right) => CreateByVector(left._vector & right._vector);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CellMap operator |(in CellMap left, in CellMap right) => CreateByVector(left._vector | right._vector);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CellMap operator ^(in CellMap left, in CellMap right) => CreateByVector(left._vector ^ right._vector);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
			result.Add(cell * 9 + digit);
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
