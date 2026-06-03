#undef EMPTY_GRID_STRING_CONSTANT

namespace Sudoku.Concepts;

using GridBase = IGrid<Grid>;
using InlineArrayGridBase = IInlineArrayGrid<Grid>;

/// <summary>
/// Represents a sudoku grid.
/// </summary>
/// <remarks>
/// This type uses 162 bytes.
/// </remarks>
[CollectionBuilder(typeof(Grid), nameof(Create))]
[DebuggerDisplay($$"""{{{nameof(ToString)}}("#")}""")]
[InlineArray(81)]
[JsonConverter(typeof(Converter))]
public partial struct Grid : InlineArrayGridBase
{
	/// <inheritdoc cref="InlineArrayGridBase.DefaultMask"/>
	public const Mask DefaultMask = EmptyMask | MaxCandidatesMask;

	/// <inheritdoc cref="InlineArrayGridBase.MaxCandidatesMask"/>
	public const Mask MaxCandidatesMask = (1 << 9) - 1;

	/// <inheritdoc cref="InlineArrayGridBase.EmptyMask"/>
	public const Mask EmptyMask = (Mask)CellState.Empty << 9;

	/// <inheritdoc cref="InlineArrayGridBase.ModifiableMask"/>
	public const Mask ModifiableMask = (Mask)CellState.Modifiable << 9;

	/// <inheritdoc cref="InlineArrayGridBase.GivenMask"/>
	public const Mask GivenMask = (Mask)CellState.Given << 9;

#if EMPTY_GRID_STRING_CONSTANT
	/// <inheritdoc cref="GridBase.EmptyString"/>
	public const string EmptyString = "000000000000000000000000000000000000000000000000000000000000000000000000000000000";
#endif


#if !EMPTY_GRID_STRING_CONSTANT
	/// <inheritdoc cref="GridBase.EmptyString"/>
	public static readonly string EmptyString = new('0', 81);
#endif

	/// <inheritdoc cref="GridBase.Empty"/>
	public static readonly Grid Empty = [DefaultMask];

	/// <inheritdoc cref="GridBase.Undefined"/>
	public static readonly Grid Undefined;


	/// <summary>
	/// <inheritdoc cref="InlineArrayGridBase.FirstMaskRef" path="/summary"/>
	/// </summary>
	/// <remarks>
	/// <para><inheritdoc cref="InlineArrayGridBase.FirstMaskRef" path="/remarks/para[1]"/></para>
	/// <para>
	/// Part (3) is for the reserved bits. Such bits won't be used except for the array element at index 0 -
	/// The first element in the array will use (3) to represent the sudoku grid type. There are only two kinds of grid type value:
	/// <list type="table">
	/// <listheader>
	/// <term>Value</term>
	/// <description>Description</description>
	/// </listheader>
	/// <item>
	/// <term>0b0000</term>
	/// <description>Represents standard sudoku type</description>
	/// </item>
	/// <item>
	/// <term>0b0010</term>
	/// <description>Represents Sukaku</description>
	/// </item>
	/// </list>
	/// </para>
	/// </remarks>
	private Mask _values;


	/// <summary>
	/// Creates a <see cref="Grid"/> instance via the pointer of the first element of the cell digit, and the creating option.
	/// </summary>
	/// <param name="firstElement">The reference of the first element.</param>
	/// <param name="creatingOption">The creating option.</param>
	/// <exception cref="ArgumentNullException">
	/// Throws when the argument <paramref name="firstElement"/> is <see langword="null"/> reference.
	/// </exception>
	private Grid(ref readonly Digit firstElement, GridCreatingOption creatingOption = GridCreatingOption.None)
	{
		// Firstly we should initialize the inner values.
		this = Empty;

		// Then traverse the array (span, pointer or etc.), to get refresh the values.
		var minusOneEnabled = creatingOption == GridCreatingOption.MinusOne;
		for (var i = 0; i < 81; i++)
		{
			var value = Unsafe.Add(ref Unsafe.AsRef(in firstElement), i);
			if ((minusOneEnabled ? value - 1 : value) is var realValue and not -1)
			{
				// Calls the indexer to trigger the event (Clear the candidates in peer cells).
				SetDigit(i, realValue);

				// Set the state to 'CellState.Given'.
				SetState(i, CellState.Given);
			}
		}
	}


	/// <inheritdoc/>
	public readonly bool IsUndefined => this == Undefined;

	/// <inheritdoc/>
	public readonly bool IsEmpty => this == Empty;

	/// <inheritdoc/>
	public readonly bool IsSolved
	{
		get
		{
			for (var i = 0; i < 81; i++)
			{
				if (GetState(i) == CellState.Empty)
				{
					return false;
				}
			}

			for (var i = 0; i < 81; i++)
			{
				switch (GetState(i))
				{
					case CellState.Given or CellState.Modifiable:
					{
						var curDigit = GetDigit(i);
						foreach (var cell in Peer.PeersMap[i])
						{
							if (curDigit == GetDigit(cell))
							{
								return false;
							}
						}
						break;
					}
					case CellState.Empty:
					{
						continue;
					}
					default:
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	/// <inheritdoc/>
	public readonly bool IsMissingCandidates => ResetGrid == ResetCandidatesGrid.ResetGrid && this != ResetCandidatesGrid;

	/// <summary>
	/// Indicates whether the grid is Sukaku (a sudoku variant that is a full-pencilmarked grid, without any candidates).
	/// </summary>
	public readonly bool IsSukaku => GetHeaderBits(0) == GridBase.SukakuHeader;

	/// <summary>
	/// Indicates whether the grid is standard sudoku.
	/// </summary>
	public readonly bool IsStandard => GetHeaderBits(0) != GridBase.SukakuHeader;

	/// <summary>
	/// Indicates whether the grid is at invalid state that a certain cell has no candidates.
	/// </summary>
	public readonly bool HasCellHavingNoCandidates
	{
		get
		{
			for (var cell = 0; cell < 81; cell++)
			{
				if (GetCandidates(cell) == 0)
				{
					return true;
				}
			}
			return false;
		}
	}

	/// <inheritdoc/>
	public readonly Cell GivenCellsCount => GivenCells.Count;

	/// <inheritdoc/>
	public readonly Cell ModifiableCellsCount => ModifiableCells.Count;

	/// <inheritdoc/>
	public readonly Cell EmptyCellsCount => EmptyCells.Count;

	/// <inheritdoc/>
	public readonly Candidate CandidatesCount
	{
		get
		{
			var count = 0;
			for (var i = 0; i < 81; i++)
			{
				if (GetState(i) == CellState.Empty)
				{
					count += PopCount((uint)GetCandidates(i));
				}
			}
			return count;
		}
	}

	/// <inheritdoc/>
	public readonly HouseMask EmptyHouses
	{
		get
		{
			var result = 0;
			for (var (house, valueCells) = (0, ~EmptyCells); house < 27; house++)
			{
				if (valueCells / house == 0)
				{
					result |= 1 << house;
				}
			}
			return result;
		}
	}

	/// <inheritdoc/>
	public readonly HouseMask CompletedHouses
	{
		get
		{
			var emptyCells = EmptyCells;
			var result = 0;
			for (var house = 0; house < 27; house++)
			{
				if (!(HousesMap[house] & emptyCells))
				{
					result |= 1 << house;
				}
			}
			return result;
		}
	}

	/// <inheritdoc/>
	public readonly unsafe CellMap GivenCells => GridBase.GetMap(this, &Grid.IsGivenCell);

	/// <inheritdoc/>
	public readonly unsafe CellMap ModifiableCells => GridBase.GetMap(this, &Grid.IsModifiableCell);

	/// <inheritdoc/>
	public readonly unsafe CellMap EmptyCells => GridBase.GetMap(this, &Grid.IsEmptyCell);

	/// <inheritdoc/>
	public readonly unsafe CellMap BivalueCells => GridBase.GetMap(this, &Grid.IsBivalueCell);

	/// <inheritdoc/>
	public readonly unsafe ReadOnlySpan<CellMap> CandidatesMap => GridBase.GetMaps(this, &Grid.ExistsInCandidatesMap);

	/// <inheritdoc/>
	public readonly unsafe ReadOnlySpan<CellMap> DigitsMap => GridBase.GetMaps(this, &Grid.ExistsInDigitsMap);

	/// <inheritdoc/>
	public readonly unsafe ReadOnlySpan<CellMap> ValuesMap => GridBase.GetMaps(this, &Grid.ExistsInValuesMap);

	/// <inheritdoc/>
	public readonly ReadOnlySpan<Candidate> Candidates
	{
		get
		{
			var candidates = new Candidate[CandidatesCount];
			for (var (cell, i) = (0, 0); cell < 81; cell++)
			{
				if (GetState(cell) == CellState.Empty)
				{
					foreach (var digit in GetCandidates(cell))
					{
						candidates[i++] = cell * 9 + digit;
					}
				}
			}
			return candidates;
		}
	}

	/// <inheritdoc/>
	public readonly ReadOnlySpan<Conjugate> ConjugatePairs
	{
		get
		{
			var conjugatePairs = new List<Conjugate>();
			var candidatesMap = CandidatesMap;
			for (var digit = 0; digit < 9; digit++)
			{
				ref readonly var cellsMap = ref candidatesMap[digit];
				foreach (var houseMap in HousesMap)
				{
					if ((houseMap & cellsMap) is { Count: 2 } temp)
					{
						conjugatePairs.Add(new(temp, digit));
					}
				}
			}
			return conjugatePairs.AsSpan();
		}
	}

	/// <inheritdoc/>
	public readonly Grid ResetGrid => Preserve(GivenCells);

	/// <summary>
	/// Gets the grid where all empty cells are filled with all possible candidates.
	/// </summary>
	public readonly Grid ResetCandidatesGrid
	{
		get
		{
			var result = this;
			result.ResetCandidates();
			return result;
		}
	}

	/// <inheritdoc/>
	public readonly Grid UnfixedGrid
	{
		get
		{
			var result = this;
			result.Unfix();
			return result;
		}
	}

	/// <inheritdoc/>
	public readonly Grid FixedGrid
	{
		get
		{
			var result = this;
			result.Fix();
			return result;
		}
	}

	/// <inheritdoc/>
	[UnscopedRef]
	readonly ReadOnlySpan<Mask> InlineArrayGridBase.Elements => this[..];

	/// <inheritdoc cref="_values"/>
	[UnscopedRef]
	readonly ref readonly Mask InlineArrayGridBase.FirstMaskRef => ref this[0];


	/// <inheritdoc/>
	static string GridBase.EmptyString => EmptyString;

	/// <inheritdoc/>
	static Mask InlineArrayGridBase.DefaultMask => DefaultMask;

	/// <inheritdoc/>
	static Mask InlineArrayGridBase.MaxCandidatesMask => MaxCandidatesMask;

	/// <inheritdoc/>
	static Mask InlineArrayGridBase.EmptyMask => EmptyMask;

	/// <inheritdoc/>
	static Mask InlineArrayGridBase.ModifiableMask => ModifiableMask;

	/// <inheritdoc/>
	static Mask InlineArrayGridBase.GivenMask => GivenMask;

	/// <inheritdoc/>
	static ref readonly Grid GridBase.Empty => ref Empty;

	/// <inheritdoc/>
	static ref readonly Grid GridBase.Undefined => ref Undefined;


	/// <inheritdoc/>
	public readonly Mask this[in CellMap cells]
	{
		get
		{
			var result = (Mask)0;
			foreach (var cell in cells)
			{
				result |= this[cell];
			}
			return (Mask)(result & MaxCandidatesMask);
		}
	}

	/// <inheritdoc/>
	public readonly unsafe Mask this[in CellMap cells, bool withValueCells, MaskAggregator aggregator = MaskAggregator.Or]
	{
		get
		{
			var result = aggregator switch
			{
				MaskAggregator.AndNot or MaskAggregator.And => MaxCandidatesMask,
				MaskAggregator.Or => (Mask)0,
				_ => throw new ArgumentOutOfRangeException(nameof(aggregator))
			};
			delegate*<ref Mask, in Grid, Cell, void> mergingFunctionPtr = aggregator switch
			{
				MaskAggregator.AndNot => &andNot,
				MaskAggregator.And => &and,
				MaskAggregator.Or => &or
			};
			foreach (var cell in cells)
			{
				if (withValueCells || GetState(cell) == CellState.Empty)
				{
					mergingFunctionPtr(ref result, this, cell);
				}
			}
			return (Mask)(result & MaxCandidatesMask);


			static void andNot(ref Mask result, in Grid grid, Cell cell) => result &= (Mask)~grid[cell];

			static void and(ref Mask result, in Grid grid, Cell cell) => result &= grid[cell];

			static void or(ref Mask result, in Grid grid, Cell cell) => result |= grid[cell];
		}
	}

	/// <inheritdoc/>
	[UnscopedRef]
	ref Mask InlineArrayGridBase.this[Cell cell] => ref this[cell];


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruct-method']/target[@name='method']"/>
	public readonly void Deconstruct(out CellMap givenCells, out CellMap modifiableCells, out CellMap emptyCells)
		=> (givenCells, modifiableCells, emptyCells) = (GivenCells, ModifiableCells, EmptyCells);

	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruct-method']/target[@name='method']"/>
	public readonly void Deconstruct(out CellMap givenCells, out CellMap modifiableCells, out CellMap emptyCells, out CellMap bivalueCells)
		=> ((givenCells, modifiableCells, emptyCells), bivalueCells) = (this, BivalueCells);

	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruct-method']/target[@name='method']"/>
	public readonly void Deconstruct(
		out CellMap emptyCells,
		out CellMap bivalueCells,
		out ReadOnlySpan<CellMap> candidatesMap,
		out ReadOnlySpan<CellMap> digitsMap,
		out ReadOnlySpan<CellMap> valuesMap
	)
	{
		(emptyCells, bivalueCells) = (EmptyCells, BivalueCells);
		candidatesMap = CandidatesMap;
		digitsMap = DigitsMap;
		valuesMap = ValuesMap;
	}

	/// <inheritdoc/>
	public readonly override bool Equals([NotNullWhen(true)] object? obj) => obj is Grid comparer && Equals(comparer);

	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	public readonly unsafe bool Equals(in Grid other)
	{
		if (Avx2.IsSupported || Sse2.IsSupported)
		{
			var i = 0;
			fixed (short* pLeft = this[..], pRight = other[..])
			{
				if (Avx2.IsSupported)
				{
					var step = Vector256<short>.Count; // 16
					for (; i <= 81 - step; i += step)
					{
						var v1 = Avx.LoadVector256(pLeft + i);
						var v2 = Avx.LoadVector256(pRight + i);
						if (Avx2.MoveMask(Avx2.CompareEqual(v1, v2).AsByte()) != -1)
						{
							return false;
						}
					}
				}
				else if (Sse2.IsSupported)
				{
					var step = Vector128<short>.Count; // 8
					for (; i <= 81 - step; i += step)
					{
						var v1 = Sse2.LoadVector128(pLeft + i);
						var v2 = Sse2.LoadVector128(pRight + i);
						if (Sse2.MoveMask(Sse2.CompareEqual(v1, v2).AsByte()) != 0xFFFF)
						{
							return false;
						}
					}
				}
				for (; i < 81; i++)
				{
					if (this[i] != other[i])
					{
						return false;
					}
				}
			}
			return true;
		}

		// Fallback.
		return this[..].SequenceEqual(other[..]);
	}

	/// <summary>
	/// Compares two <see cref="Grid"/> instances and returns a <see cref="bool"/> value
	/// indicating whether they are same under the specified comparison rule using <paramref name="comparer"/>.
	/// </summary>
	/// <param name="other">The other object to be compared.</param>
	/// <param name="comparer">The comparer instance.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public readonly bool Equals(in Grid other, IEqualityComparer<Grid>? comparer)
		=> (comparer ?? new GridEqualityComparer()).Equals(this, other);

	/// <inheritdoc/>
	public readonly bool ConflictWith(Cell cell, Digit digit)
	{
		foreach (var tempCell in Peer.PeersMap[cell])
		{
			if (GetDigit(tempCell) == digit)
			{
				return true;
			}
		}
		return false;
	}

	/// <inheritdoc/>
	public readonly bool GetExistence(Cell cell, Digit digit) => (this[cell] >> digit & 1) != 0;

	/// <inheritdoc/>
	public readonly bool? Exists(Candidate candidate) => Exists(candidate / 9, candidate % 9);

	/// <inheritdoc/>
	public readonly bool? Exists(Cell cell, Digit digit) => GetState(cell) == CellState.Empty ? GetExistence(cell, digit) : null;

	/// <inheritdoc cref="object.GetHashCode"/>
	public readonly override int GetHashCode()
		=> this switch { { IsUndefined: true } => 0, { IsEmpty: true } => 1, _ => ToString("#").GetHashCode() };

	/// <summary>
	/// Serves as hash code function; with a comparer instance to calculate hash code.
	/// </summary>
	/// <param name="comparer">The comparer instance.</param>
	/// <returns>A hash code.</returns>
	public readonly int GetHashCode(IEqualityComparer<Grid>? comparer) => (comparer ?? new GridEqualityComparer()).GetHashCode(this);

	/// <inheritdoc cref="IComparable{T}.CompareTo(T)"/>
	/// <exception cref="InvalidOperationException">Throws when the puzzle type is Sukaku.</exception>
	public readonly int CompareTo(in Grid other)
		=> !IsSukaku && !other.IsSukaku
			? ToString("#").CompareTo(other.ToString("#"))
			: throw new InvalidOperationException(SR.ExceptionMessage("ComparableGridMustBeStandard"));

	/// <inheritdoc cref="object.ToString"/>
	public readonly override string ToString() => IsSukaku ? ToString("~") : ToString(GridConverterFactory.InvariantCultureInstance);

	/// <remarks>
	/// <para>You can use format identifiers to create the format text. All valid format identifiers:
	/// <list type="table">
	/// <listheader>
	/// <term>Format identifier</term>
	/// <description>Meaning</description>
	/// </listheader>
	/// <item>
	/// <term><c>.</c></term>
	/// <description>Placeholders; empty cells will be replaced with <c>'.'</c></description>
	/// </item>
	/// <item>
	/// <term><c>0</c></term>
	/// <description>Placeholders; empty cells will be replaced with <c>'0'</c></description>
	/// </item>
	/// <item>
	/// <term><c>+</c></term>
	/// <description>Includes modifiable digits (Susser format only)</description>
	/// </item>
	/// <item>
	/// <term><c>:</c></term>
	/// <description>Includes eliminations (for both Susser and multiline formats)</description>
	/// </item>
	/// <item>
	/// <term><c>!</c></term>
	/// <description>Treats modifiable digits as givens</description>
	/// </item>
	/// <item>
	/// <term><c>^</c></term>
	/// <description>Negates the eliminations in Susser format (if available)</description>
	/// </item>
	/// <item>
	/// <term><c>*</c></term>
	/// <description>
	/// The shorten format (Susser format only);
	/// for multiline format, it means border line characters will use subtle characters instead
	/// </description>
	/// </item>
	/// <item>
	/// <term><c>@</c></term>
	/// <description>Multiline format</description>
	/// </item>
	/// <item>
	/// <term><c>~</c></term>
	/// <description>Sukaku format</description>
	/// </item>
	/// <item>
	/// <term><c>#</c></term>
	/// <description>Equivalent to combination <c>".+:"</c></description>
	/// </item>
	/// </list>
	/// </para>
	/// <para>
	/// Such symbols can be combinated with other symbols in order to make output text better-looking.
	/// </para>
	/// <para>
	/// Examples:
	/// <list type="table">
	/// <item>
	/// <term><c>"0+"</c></term>
	/// <description>Susser format (single-line format), with zeros being placeholders, including modifiable digits</description>
	/// </item>
	/// <item>
	/// <term><c>".!"</c></term>
	/// <description>
	/// Susser format (single-line format), with treating modifiable digits as givens, and using dots as placeholders
	/// </description>
	/// </item>
	/// <item>
	/// <term><c>"@:"</c></term>
	/// <description>Multiline format, with candidates displaying</description>
	/// </item>
	/// </list>
	/// </para>
	/// </remarks>
	/// <inheritdoc cref="ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(string? format) => ToString(format, null);

	/// <summary>
	/// Retrieves <see cref="IGridConverter"/> instance via the specified culture, and format the current instance.
	/// </summary>
	/// <param name="culture">The culture.</param>
	/// <returns>The string.</returns>
	public readonly string ToString(CultureInfo culture)
	{
		var instance = GridConverterFactory.GetInstance(culture);
		return instance.TryFormat(in this, culture, out var result) ? result : throw new FormatException();
	}

	/// <inheritdoc cref="ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(IFormatProvider? formatProvider)
		=> GridConverterFactory.TryFormat(in this, formatProvider, out var result) ? result : throw new FormatException();

	/// <inheritdoc cref="ToString(IGridConverter, IFormatProvider?)"/>
	public readonly string ToString(IGridConverter converter)
		=> converter.TryFormat(in this, null, out var result) ? result : throw new FormatException();

	/// <summary>
	/// Performs formatting operation via the specified grid converter and format provider.
	/// </summary>
	/// <param name="converter">The converter.</param>
	/// <param name="formatProvider">The format provider.</param>
	/// <returns>The string representation.</returns>
	/// <exception cref="FormatException">Throws when the current grid is malformed.</exception>
	public readonly string ToString(IGridConverter converter, IFormatProvider? formatProvider)
		=> converter.TryFormat(in this, formatProvider, out var result) ? result : throw new FormatException();

	/// <summary>
	/// Performs formatting operation via the specified grid converter and format provider.
	/// </summary>
	/// <param name="format">
	/// The format. For more information about <paramref name="format"/>, please see documentation comments of method
	/// <see cref="ToString(string?)"/>.
	/// </param>
	/// <param name="formatProvider">The format provider.</param>
	/// <returns>The string representation.</returns>
	/// <seealso cref="ToString(string?)"/>
	public readonly string ToString(string? format, IFormatProvider? formatProvider)
	{
		var converter = GridConverterFactory.GetInstance(format) ?? GridConverterFactory.InvariantCultureInstance;
		return converter.TryFormat(in this, formatProvider, out var result) ? result : throw new FormatException();
	}

	/// <inheritdoc/>
	public readonly Digit[] ToDigitsArray()
	{
		var result = new Digit[81];
		for (var i = 0; i < 81; i++)
		{
			// -1..8 -> 0..9
			result[i] = GetDigit(i) + 1;
		}
		return result;
	}

	/// <inheritdoc/>
	public readonly Mask[] ToCandidateMaskArray()
	{
		var result = new Mask[81];
		for (var cell = 0; cell < 81; cell++)
		{
			result[cell] = (Mask)(this[cell] & MaxCandidatesMask);
		}
		return result;
	}

	/// <summary>
	/// Creates an array of <see cref="Mask"/> values that is a copy for the current inline array data structure.
	/// </summary>
	/// <returns>An array of <see cref="Mask"/> values.</returns>
	public readonly Mask[] ToMaskArray() => this[..].ToArray();

	/// <inheritdoc/>
	public readonly Mask GetCandidates(Cell cell) => (Mask)(this[cell] & MaxCandidatesMask);

	/// <inheritdoc/>
	public readonly CellState GetState(Cell cell) => MaskToCellState(this[cell]);

	/// <inheritdoc/>
	public readonly Digit GetDigit(Cell cell)
		=> GetState(cell) switch
		{
			CellState.Empty => -1,
			CellState.Modifiable or CellState.Given => TrailingZeroCount(this[cell]),
			_ => throw new InvalidOperationException(SR.ExceptionMessage("GridInvalidCellState"))
		};

	/// <summary>
	/// Filters the candidates that satisfies the specified condition.
	/// </summary>
	/// <param name="predicate">The condition to filter candidates.</param>
	/// <returns>All candidates satisfied the specified condition.</returns>
	public readonly ReadOnlySpan<Candidate> Where(Func<Candidate, bool> predicate)
	{
		var (result, i) = (new Candidate[CandidatesCount], 0);
		foreach (var candidate in Candidates)
		{
			if (predicate(candidate))
			{
				result[i++] = candidate;
			}
		}
		return result.AsReadOnlySpan()[..i];
	}

	/// <summary>
	/// Projects each element of a sequence into a new form.
	/// </summary>
	/// <typeparam name="TResult">The type of the value returned by <paramref name="selector"/>.</typeparam>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <returns>
	/// An array of <typeparamref name="TResult"/> elements converted.
	/// </returns>
	public readonly ReadOnlySpan<TResult> Select<TResult>(Func<Candidate, TResult> selector)
	{
		var (result, i) = (new TResult[CandidatesCount], 0);
		foreach (var candidate in Candidates)
		{
			result[i++] = selector(candidate);
		}
		return result.AsReadOnlySpan()[..i];
	}

	/// <inheritdoc/>
	public void Reset()
	{
		if (!IsStandard)
		{
			// Don't handle if the puzzle type is not a valid standard sudoku puzzle.
			return;
		}

		for (var i = 0; i < 81; i++)
		{
			if (GetState(i) == CellState.Modifiable)
			{
				SetDigit(i, -1); // Reset the cell, and then re-compute all candidates.
			}
		}
	}

	/// <summary>
	/// Reset the sudoku grid, but only making candidates to be reset to the initial state related to the current grid
	/// from given and modifiable values.
	/// </summary>
	public void ResetCandidates()
	{
		if (!IsStandard)
		{
			// Don't handle if the puzzle type is not a valid standard sudoku puzzle.
			return;
		}

		if (ToString("#") is var p && p.IndexOf(':') is var colonTokenPos and not -1)
		{
			this = Parse(p[..colonTokenPos]);
		}
	}

	/// <inheritdoc/>
	public void Fix()
	{
		if (!IsStandard)
		{
			// Don't handle if the puzzle type is not a valid standard sudoku puzzle.
			return;
		}

		for (var i = 0; i < 81; i++)
		{
			if (GetState(i) == CellState.Modifiable)
			{
				SetState(i, CellState.Given);
			}
		}
	}

	/// <inheritdoc/>
	public void Unfix()
	{
		if (!IsStandard)
		{
			// Don't handle if the puzzle type is not a valid standard sudoku puzzle.
			return;
		}

		for (var i = 0; i < 81; i++)
		{
			if (GetState(i) == CellState.Given)
			{
				SetState(i, CellState.Modifiable);
			}
		}
	}

	/// <inheritdoc/>
	public void Apply(Conclusion conclusion)
	{
		var (type, cell, digit) = conclusion;
		if (type == Assignment)
		{
			SetDigit(cell, digit);
		}
		else if (type == Elimination)
		{
			SetExistence(cell, digit, false);
		}
	}

	/// <inheritdoc/>
	public void SetState(Cell cell, CellState state)
	{
		ref var mask = ref this[cell];
		mask = (Mask)(GetHeaderBits(cell) | (Mask)((int)state << 9) | mask & MaxCandidatesMask);
		OnValueChanged(ref this, cell, -1);
	}

	/// <inheritdoc/>
	public void SetCandidates(Cell cell, Mask mask)
		=> SetMask(cell, (Mask)(GetHeaderBits(cell) | (Mask)((int)GetState(cell) << 9) | mask & MaxCandidatesMask));

	/// <inheritdoc/>
	public void SetMask(Cell cell, Mask mask)
	{
		ref var newMask = ref this[cell];
		newMask = mask;
		OnValueChanged(ref this, cell, -1);
	}

	/// <summary>
	/// Replace the specified cell with the specified digit.
	/// </summary>
	/// <param name="cell">The cell to be set.</param>
	/// <param name="digit">The digit to be set.</param>
	/// <exception cref="ArgumentOutOfRangeException">Throws when the argument <paramref name="digit"/> is invalid (e.g. -1).</exception>
	public void ReplaceDigit(Cell cell, Digit digit)
	{
		ArgumentOutOfRangeException.Assert(digit is >= 0 and < 9);

		SetDigit(cell, -1);
		SetDigit(cell, digit);
	}

	/// <inheritdoc/>
	public void SetDigit(Cell cell, Digit digit)
	{
		switch (digit)
		{
			case -1 when GetState(cell) == CellState.Modifiable:
			{
				// If 'value' is -1, we should reset the grid.
				// Note that reset candidates may not trigger the event.
				this[cell] = (Mask)(GetHeaderBits(cell) | DefaultMask);

				OnRefreshingCandidates(ref this);
				break;
			}
			case >= 0 and < 9:
			{
				ref var result = ref this[cell];

				// Set cell state to 'CellState.Modifiable'.
				result = (Mask)(GetHeaderBits(cell) | ModifiableMask | 1 << digit);

				// To trigger the event, which is used for eliminate all same candidates in peer cells.
				OnValueChanged(ref this, cell, digit);
				break;
			}
		}
	}

	/// <inheritdoc/>
	public void SetExistence(Cell cell, Digit digit, bool isOn)
	{
		if (cell is >= 0 and < 81 && digit is >= 0 and < 9)
		{
			if (isOn)
			{
				this[cell] |= (Mask)(1 << digit);
			}
			else
			{
				this[cell] &= (Mask)~(1 << digit);
			}

			// To trigger the event.
			OnValueChanged(ref this, cell, -1);
		}
	}

	/// <summary>
	/// Preserves the grid, removing cells not specified in <paramref name="template"/>.
	/// </summary>
	/// <param name="template">The cells specified to be preserved.</param>
	public readonly Grid Preserve(in CellMap template)
	{
		var temp = this;
		if (!IsStandard)
		{
			goto Return;
		}

		foreach (var cell in ~template)
		{
			temp.SetDigit(cell, -1);
		}

	Return:
		return temp;
	}

	/// <summary>
	/// Gets the header 4 bits. The value can be 2 if and only if the puzzle is Sukaku,
	/// with argument <paramref name="cell"/> equal to 0.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <returns>The header 4 bits, represented as a <see cref="Mask"/>, left-shifted.</returns>
	/// <remarks>
	/// For more information about this type, please visit <c>remarks</c> part in <see cref="GetHeaderBitsUnshifted(Cell)"/>.
	/// </remarks>
	/// <seealso cref="GetHeaderBitsUnshifted(Cell)"/>
	internal readonly Mask GetHeaderBits(Cell cell) => (Mask)(this[cell] & ~((1 << GridBase.HeaderShift) - 1));

	/// <summary>
	/// Gets the header 4 bits of the specified cell.
	/// </summary>
	/// <param name="cell">
	/// The cell. Generally the value should be 0 because today only higher bits at cell <c>r1c1</c> is in use.
	/// </param>
	/// <returns>The header, in 4 bits, but using original mask type <see cref="Mask"/> to represent that.</returns>
	/// <remarks>
	/// <include file="../../global-doc-comments.xml" path="/g/developer-notes"/>
	/// <para>
	/// Due to design complexity of this type, I hard-coded the value 2 into the first cell of the inline array,
	/// the backing field of this type, with higher 4 bits (12..16) equal to <c>0b0010</c>.
	/// If the value is <c>0b0010</c> at cell 0 in higher bits, we can know this grid is a Sukaku.
	/// </para>
	/// <para>
	/// I don't like this design, but it is Sukaku-unaware if we loss such data.
	/// We cannot directly check whether a grid has no given cells or not to know the grid is a standard sudoku puzzle or not;
	/// it effects string-parsing modules, causing wrong returns.
	/// </para>
	/// <para>
	/// In the early of this repository built,
	/// I tried compilation symbol <c>TRANSFORM_LESS_GIVEN_STANDARD_SUDOKU_PUZZLES_TO_SUKAKU</c>
	/// to enable (include) or disable (exclude) some code snippets, but it didn't work in some edge cases.
	/// Although such code are disabled by default, I still want to adjust such procedure to make such API graceful.
	/// Please visit raw code in <see cref="Parse(string?)"/> if you are full of curiosity.
	/// </para>
	/// </remarks>
	/// <seealso cref="Parse(string?)"/>
	internal readonly Mask GetHeaderBitsUnshifted(Cell cell) => (Mask)(this[cell] >> GridBase.HeaderShift);

	/// <summary>
	/// Appends for Sukaku puzzle header.
	/// </summary>
	internal void AddSukakuHeader() => this[0] |= GridBase.SukakuHeader;

	/// <summary>
	/// Removes for Sukaku puzzle header.
	/// </summary>
	internal void RemoveSukakuHeader() => this[0] &= (1 << GridBase.HeaderShift) - 1;

	/// <inheritdoc/>
	readonly bool IEquatable<Grid>.Equals(Grid other) => Equals(other);

	/// <inheritdoc/>
	readonly Grid IElementSwappingTransformable<Grid, Digit>.Shuffle()
	{
		var rng = Random.Shared;
		var current = this;
		for (var d1 = 0; d1 < 9; d1++)
		{
			Digit d2;
			do
			{
				d2 = rng.NextDigit();
			} while (d1 == d2);
			current.SwapDigit(d1, d2);
		}
		return current;
	}

	/// <inheritdoc/>
	readonly IEnumerator<Digit> IEnumerable<Digit>.GetEnumerator() => ToDigitsArray().AsEnumerable().GetEnumerator();

	/// <inheritdoc/>
	readonly IEnumerable<Candidate> IWhereMethod<Grid, Candidate>.Where(Func<Candidate, bool> predicate)
		=> Where(predicate).ToArray();

	/// <inheritdoc/>
	readonly IEnumerable<TResult> ISelectMethod<Grid, Candidate>.Select<TResult>(Func<Candidate, TResult> selector)
		=> Select(selector).ToArray();

	/// <inheritdoc/>
	Grid ITransformable<Grid>.MirrorLeftRight() => this.MirrorLeftRight();

	/// <inheritdoc/>
	Grid ITransformable<Grid>.MirrorTopBottom() => this.MirrorTopBottom();

	/// <inheritdoc/>
	Grid ITransformable<Grid>.MirrorDiagonal() => this.MirrorDiagonal();

	/// <inheritdoc/>
	Grid ITransformable<Grid>.MirrorAntidiagonal() => this.MirrorAntidiagonal();

	/// <inheritdoc/>
	Grid ITransformable<Grid>.RotateClockwise() => this.RotateClockwise();

	/// <inheritdoc/>
	Grid ITransformable<Grid>.RotateCounterclockwise() => this.RotateCounterclockwise();

	/// <inheritdoc/>
	Grid IElementSwappingTransformable<Grid, Digit>.SwapElement(Digit element1, Digit element2) => this.SwapDigit(element1, element2);


	/// <inheritdoc/>
	public static bool TryParse(string? s, out Grid result) => TryParse(s.AsSpan(), default(IFormatProvider), out result);

	/// <inheritdoc cref="TryParse(ReadOnlySpan{char}, CultureInfo, out Grid)"/>
	public static bool TryParse(string? s, CultureInfo culture, out Grid result) => TryParse(s.AsSpan(), culture, out result);

	/// <inheritdoc cref="TryParse(ReadOnlySpan{char}, IGridConverter, IFormatProvider?, out Grid)"/>
	public static bool TryParse(string? s, IGridConverter converter, out Grid result)
		=> TryParse(s.AsSpan(), converter, null, out result);

	/// <inheritdoc cref="TryParse(ReadOnlySpan{char}, IGridConverter, IFormatProvider?, out Grid)"/>
	public static bool TryParse(string? s, IGridConverter converter, IFormatProvider? formatProvider, out Grid result)
		=> TryParse(s.AsSpan(), converter, formatProvider, out result);

	/// <inheritdoc cref="TryParse(ReadOnlySpan{char}, IGridConverter, IFormatProvider?, out Grid)"/>
	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out Grid result)
		=> TryParse(s.AsSpan(), provider, out result);

	/// <inheritdoc cref="TryParse(ReadOnlySpan{char}, IFormatProvider?, out Grid)"/>
	public static bool TryParse(ReadOnlySpan<char> s, out Grid result) => GridConverterFactory.TryParse(s, null, out result);

	/// <summary>
	/// Retrieves a grid converter instance of type <see cref="IGridConverter"/> via the specified culture,
	/// and try to convert the specified string into grid instance.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="culture">The culture.</param>
	/// <param name="result">The result parsed.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool TryParse(ReadOnlySpan<char> s, CultureInfo culture, out Grid result)
		=> culture switch
		{
			{ IsEnglish: true } => new PencilmarkGridConverter().TryParse(s, culture, out result),
			{ IsChinese: true } => new SusserGridDefaultConverter().TryParse(s, culture, out result),
			_ => GridConverterFactory.TryParse(s, culture, out result)
		};

	/// <inheritdoc cref="TryParse(string?, IGridConverter, IFormatProvider?, out Grid)"/>
	public static bool TryParse(ReadOnlySpan<char> s, IGridConverter converter, out Grid result)
		=> converter.TryParse(s, null, out result);

	/// <summary>
	/// Try to parse the current string into <see cref="Grid"/> instance;
	/// specify a converter that controls the parsing rule, with format provider effecting that rule
	/// (like culture, numeric literal handling, etc.).
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="converter">The converter.</param>
	/// <param name="formatProvider">The format provider.</param>
	/// <param name="result">The result.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public static bool TryParse(ReadOnlySpan<char> s, IGridConverter converter, IFormatProvider? formatProvider, out Grid result)
		=> converter.TryParse(s, formatProvider, out result);

	/// <inheritdoc cref="TryParse(ReadOnlySpan{char}, IGridConverter, IFormatProvider?, out Grid)"/>
	public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Grid result)
		=> GridConverterFactory.TryParse(s, provider, out result);

	/// <summary>
	/// Creates a <see cref="Grid"/> instance using grid values.
	/// </summary>
	/// <param name="gridValues">The array of grid values.</param>
	/// <param name="creatingOption">The grid creating option.</param>
	public static Grid Create(Digit[] gridValues, GridCreatingOption creatingOption = 0) => new(in gridValues[0], creatingOption);

	/// <summary>
	/// Creates a <see cref="Grid"/> instance via the array of cell digits
	/// of type <see cref="ReadOnlySpan{T}"/> of <see cref="Digit"/>.
	/// </summary>
	/// <param name="gridValues">The list of cell digits.</param>
	/// <param name="creatingOption">The grid creating option.</param>
	public static Grid Create(ReadOnlySpan<Digit> gridValues, GridCreatingOption creatingOption = 0)
		=> new(in gridValues[0], creatingOption);

	/// <inheritdoc/>
	public static Grid Parse(string? s) => Parse(s.AsSpan());

	/// <inheritdoc cref="Parse(ReadOnlySpan{char}, CultureInfo?)"/>
	public static Grid Parse(string? s, CultureInfo? culture) => Parse(s.AsSpan(), culture);

	/// <inheritdoc/>
	public static Grid Parse(string? s, IFormatProvider? provider) => Parse(s.AsSpan(), provider);

	/// <inheritdoc cref="Parse(string?, IGridConverter, IFormatProvider?)"/>
	public static Grid Parse(string? s, IGridConverter converter)
		=> converter.TryParse(s, null, out var result) ? result : throw new FormatException();

	/// <inheritdoc cref="Parse(ReadOnlySpan{char}, IGridConverter, IFormatProvider?)"/>
	public static Grid Parse(string? s, IGridConverter converter, IFormatProvider? provider)
		=> converter.TryParse(s, provider, out var result) ? result : throw new FormatException();

	/// <inheritdoc cref="Parse(ReadOnlySpan{char}, IGridConverter, IFormatProvider?)"/>
	public static Grid Parse(ReadOnlySpan<char> s)
		=> GridConverterFactory.TryParse(s, null, out var result) ? result : throw new FormatException();

	/// <summary>
	/// Retrieves the target <see cref="IGridConverter"/> instance via the specified culture,
	/// and then parse the string into target <see cref="Grid"/> result.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="culture">The culture.</param>
	/// <returns>The target grid parsed.</returns>
	/// <exception cref="FormatException">Throws when the text is malformed.</exception>
	public static Grid Parse(ReadOnlySpan<char> s, CultureInfo? culture)
		=> GridConverterFactory.GetInstance(culture).TryParse(s, culture, out var result) ? result : throw new FormatException();

	/// <inheritdoc/>
	public static Grid Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
		=> GridConverterFactory.TryParse(s, provider, out var result) ? result : throw new FormatException();

	/// <inheritdoc cref="Parse(ReadOnlySpan{char}, IGridConverter, IFormatProvider?)"/>
	public static Grid Parse(ReadOnlySpan<char> s, IGridConverter converter)
		=> converter.TryParse(s, null, out var result) ? result : throw new FormatException();

	/// <summary>
	/// Performs parsing operation via the specified converter and format provider.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="converter">The converter.</param>
	/// <param name="provider">The format provider.</param>
	/// <returns>The grid.</returns>
	/// <exception cref="FormatException">Throws when the string is malformed.</exception>
	public static Grid Parse(ReadOnlySpan<char> s, IGridConverter converter, IFormatProvider? provider)
		=> converter.TryParse(s, provider, out var result) ? result : throw new FormatException();

	/// <inheritdoc/>
	static void GridBase.OnValueChanged(ref Grid @this, Cell cell, Digit setValue) => OnValueChanged(ref @this, cell, setValue);

	/// <inheritdoc/>
	static void GridBase.OnRefreshingCandidates(ref Grid @this) => OnRefreshingCandidates(ref @this);

	/// <inheritdoc/>
	static Grid GridBase.Create(ReadOnlySpan<Mask> values) => Create(values);

	/// <summary>
	/// Returns a <see cref="Grid"/> instance via the raw mask values.
	/// </summary>
	/// <param name="values">
	/// <para>The raw mask values.</para>
	/// <para>
	/// This value can contain 1 or 81 elements.
	/// If the array contain 1 element, all elements in the target sudoku grid will be initialized by it, the uniform value;
	/// if the array contain 81 elements, elements will be initialized by the array one by one using the array elements respectively.
	/// </para>
	/// </param>
	/// <returns>A <see cref="Grid"/> result.</returns>
	/// <remarks><b><i>
	/// This creation ignores header bits. Please don't use this method in the puzzle creation.
	/// </i></b></remarks>
	private static Grid Create(ReadOnlySpan<Mask> values)
	{
		switch (values)
		{
			case []:
			{
				return Undefined;
			}
			case [var uniformValue]:
			{
				var result = Undefined;
				if (uniformValue == 0)
				{
					result[..].Clear();
				}
				else
				{
					result[..].Fill(uniformValue);
				}
				return result;
			}
			case { Length: 81 }:
			{
				var result = Undefined;
				values[..].CopyTo(result[..]);
				return result;
			}
			default:
			{
				throw new InvalidOperationException($"The argument '{nameof(values)}' must contain 81 elements.");
			}
		}
	}

	/// <inheritdoc cref="GridBase.OnValueChanged(ref Grid, Cell, Digit)"/>
	private static void OnValueChanged(ref Grid @this, Cell cell, Digit setValue)
	{
		if (setValue == -1)
		{
			// This method will do nothing if 'setValue' is -1.
			return;
		}

		foreach (var peerCell in Peer.PeersMap[cell])
		{
			if (@this.GetState(peerCell) == CellState.Empty)
			{
				@this[peerCell] &= (Mask)~(1 << setValue);
			}
		}
	}

	/// <inheritdoc cref="GridBase.OnRefreshingCandidates(ref Grid)"/>
	private static void OnRefreshingCandidates(ref Grid @this)
	{
		for (var cell = 0; cell < 81; cell++)
		{
			if (@this.GetState(cell) == CellState.Empty)
			{
				// Remove all appeared digits.
				var mask = MaxCandidatesMask;
				foreach (var currentCell in Peer.PeersMap[cell])
				{
					if (@this.GetDigit(currentCell) is var digit and not -1)
					{
						mask &= (Mask)~(1 << digit);
					}
				}
				@this[cell] = (Mask)((Mask)(@this.GetHeaderBits(cell) | EmptyMask) | mask);
			}
		}
	}


	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)"/>
	public static bool operator ==(in Grid left, in Grid right) => left.Equals(right);

	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)"/>
	public static bool operator !=(in Grid left, in Grid right) => !(left == right);

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)"/>
	public static bool operator >(in Grid left, in Grid right) => left.CompareTo(right) > 0;

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)"/>
	public static bool operator <(in Grid left, in Grid right) => left.CompareTo(right) < 0;

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)"/>
	public static bool operator >=(in Grid left, in Grid right) => left.CompareTo(right) >= 0;

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)"/>
	public static bool operator <=(in Grid left, in Grid right) => left.CompareTo(right) <= 0;


	/// <inheritdoc/>
	static bool IEqualityOperators<Grid, Grid, bool>.operator ==(Grid left, Grid right) => left == right;

	/// <inheritdoc/>
	static bool IEqualityOperators<Grid, Grid, bool>.operator !=(Grid left, Grid right) => left != right;

	/// <inheritdoc/>
	static bool IComparisonOperators<Grid, Grid, bool>.operator >(Grid left, Grid right) => left > right;

	/// <inheritdoc/>
	static bool IComparisonOperators<Grid, Grid, bool>.operator <(Grid left, Grid right) => left < right;

	/// <inheritdoc/>
	static bool IComparisonOperators<Grid, Grid, bool>.operator >=(Grid left, Grid right) => left >= right;

	/// <inheritdoc/>
	static bool IComparisonOperators<Grid, Grid, bool>.operator <=(Grid left, Grid right) => left <= right;
}

/// <summary>
/// Represents JSON serialization rules on type <see cref="Grid"/>.
/// </summary>
file sealed class Converter : JsonConverter<Grid>
{
	/// <inheritdoc/>
	public override bool HandleNull => true;


	/// <inheritdoc/>
	public override Grid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> reader.GetString() is { } s ? Grid.Parse(s) : Grid.Undefined;

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, Grid value, JsonSerializerOptions options)
		=> writer.WriteStringValue(value.ToString("#"));
}
