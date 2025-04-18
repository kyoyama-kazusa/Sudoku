#undef EMPTY_GRID_STRING_CONSTANT

namespace Sudoku.Concepts;

using GridBase = IGrid<Grid>;

/// <summary>
/// Represents a sudoku grid.
/// </summary>
/// <remarks>
/// <para><include file="../../global-doc-comments.xml" path="/g/large-structure"/></para>
/// </remarks>
[CollectionBuilder(typeof(Grid), nameof(Create))]
[DebuggerStepThrough]
[DebuggerDisplay($$"""{{{nameof(ToString)}}("#")}""")]
[InlineArray(81)]
[JsonConverter(typeof(Converter))]
[TypeImpl(
	TypeImplFlags.Object_Equals | TypeImplFlags.AllEqualityComparisonOperators | TypeImplFlags.Equatable,
	IsLargeStructure = true)]
public partial struct Grid : GridBase, ISubtractionOperators<Grid, Grid, DiffResult?>
{
	/// <inheritdoc cref="GridBase.DefaultMask"/>
	public const Mask DefaultMask = EmptyMask | MaxCandidatesMask;

	/// <inheritdoc cref="GridBase.MaxCandidatesMask"/>
	public const Mask MaxCandidatesMask = (1 << 9) - 1;

	/// <inheritdoc cref="GridBase.EmptyMask"/>
	public const Mask EmptyMask = (Mask)CellState.Empty << 9;

	/// <inheritdoc cref="GridBase.ModifiableMask"/>
	public const Mask ModifiableMask = (Mask)CellState.Modifiable << 9;

	/// <inheritdoc cref="GridBase.GivenMask"/>
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
	/// <inheritdoc cref="GridBase.FirstMaskRef" path="/summary"/>
	/// </summary>
	/// <remarks>
	/// <para><inheritdoc cref="GridBase.FirstMaskRef" path="/remarks/para[1]"/></para>
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
	/// <description>Represents standard sudoku type (flag: <see cref="SudokuType.Standard"/>)</description>
	/// </item>
	/// <item>
	/// <term>0b0010</term>
	/// <description>Represents Sukaku (flag: <see cref="SudokuType.Sukaku"/>)</description>
	/// </item>
	/// </list>
	/// Other values won't be supported for now, even if the flags are defined in type <see cref="SudokuType"/>.
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
	public readonly bool IsUndefined
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => this == Undefined;
	}

	/// <inheritdoc/>
	public readonly bool IsEmpty
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => this == Empty;
	}

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
						foreach (var cell in PeersMap[i])
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
	public readonly SymmetricType Symmetry => GivenCells.Symmetry;

	/// <summary>
	/// Indicates the type of the puzzle.
	/// </summary>
	/// <remarks>
	/// By design, this property can only be either <see cref="SudokuType.Standard"/> or <see cref="SudokuType.Sukaku"/>;
	/// other values like <see cref="SudokuType.JustOneCell"/> won't be created here.
	/// </remarks>
	/// <seealso cref="SudokuType.Standard"/>
	/// <seealso cref="SudokuType.Sukaku"/>
	public readonly SudokuType PuzzleType
		=> GetHeaderBits(0) switch { GridBase.SukakuHeader => SudokuType.Sukaku, _ => SudokuType.Standard };

	/// <inheritdoc/>
	public readonly Cell GivenCellsCount
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => GivenCells.Count;
	}

	/// <inheritdoc/>
	public readonly Cell ModifiableCellsCount
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ModifiableCells.Count;
	}

	/// <inheritdoc/>
	public readonly Cell EmptyCellsCount
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => EmptyCells.Count;
	}

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
					count += BitOperations.PopCount(GetCandidates(i));
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
	public readonly unsafe CellMap GivenCells => GridBase.GetMap(this, &GridPredicates.GivenCells);

	/// <inheritdoc/>
	public readonly unsafe CellMap ModifiableCells => GridBase.GetMap(this, &GridPredicates.ModifiableCells);

	/// <inheritdoc/>
	public readonly unsafe CellMap EmptyCells => GridBase.GetMap(this, &GridPredicates.EmptyCells);

	/// <inheritdoc/>
	public readonly unsafe CellMap BivalueCells => GridBase.GetMap(this, &GridPredicates.BivalueCells);

	/// <inheritdoc/>
	public readonly unsafe ReadOnlySpan<CellMap> CandidatesMap => GridBase.GetMaps(this, &GridPredicates.CandidatesMap);

	/// <inheritdoc/>
	public readonly unsafe ReadOnlySpan<CellMap> DigitsMap => GridBase.GetMaps(this, &GridPredicates.DigitsMap);

	/// <inheritdoc/>
	public readonly unsafe ReadOnlySpan<CellMap> ValuesMap => GridBase.GetMaps(this, &GridPredicates.ValuesMap);

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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			var result = this;
			result.Fix();
			return result;
		}
	}

	/// <inheritdoc/>
	[UnscopedRef]
	readonly ReadOnlySpan<Mask> IInlineArray<Grid, Mask>.Elements => this[..];

	/// <inheritdoc cref="_values"/>
	[UnscopedRef]
	readonly ref readonly Mask GridBase.FirstMaskRef => ref this[0];


	/// <inheritdoc/>
	static int IInlineArray<Grid, Mask>.InlineArrayLength => 81;

	/// <inheritdoc/>
	static string GridBase.EmptyString => EmptyString;

	/// <inheritdoc/>
	static Mask GridBase.DefaultMask => DefaultMask;

	/// <inheritdoc/>
	static Mask GridBase.MaxCandidatesMask => MaxCandidatesMask;

	/// <inheritdoc/>
	static Mask GridBase.EmptyMask => EmptyMask;

	/// <inheritdoc/>
	static Mask GridBase.ModifiableMask => ModifiableMask;

	/// <inheritdoc/>
	static Mask GridBase.GivenMask => GivenMask;

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
	ref Mask IInlineArray<Grid, Mask>.this[Cell cell] => ref this[cell];


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly void Deconstruct(out CellMap givenCells, out CellMap modifiableCells, out CellMap emptyCells)
		=> (givenCells, modifiableCells, emptyCells) = (GivenCells, ModifiableCells, EmptyCells);

	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly void Deconstruct(out CellMap givenCells, out CellMap modifiableCells, out CellMap emptyCells, out CellMap bivalueCells)
		=> ((givenCells, modifiableCells, emptyCells), bivalueCells) = (this, BivalueCells);

	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
	public readonly bool ConflictWith(Cell cell, Digit digit)
	{
		foreach (var tempCell in PeersMap[cell])
		{
			if (GetDigit(tempCell) == digit)
			{
				return true;
			}
		}
		return false;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
	{
		var targetString = ToString(format.IsEmpty ? null : format.ToString(), provider);
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
	public readonly bool GetExistence(Cell cell, Digit digit) => (this[cell] >> digit & 1) != 0;

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool? Exists(Candidate candidate) => Exists(candidate / 9, candidate % 9);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool? Exists(Cell cell, Digit digit) => GetState(cell) == CellState.Empty ? GetExistence(cell, digit) : null;

	/// <inheritdoc cref="object.GetHashCode"/>
	public override readonly int GetHashCode()
		=> this switch { { IsUndefined: true } => 0, { IsEmpty: true } => 1, _ => ToString("#").GetHashCode() };

	/// <inheritdoc cref="IComparable{T}.CompareTo(T)"/>
	/// <exception cref="InvalidOperationException">Throws when the puzzle type is Sukaku.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly int CompareTo(in Grid other)
		=> PuzzleType != SudokuType.Sukaku && other.PuzzleType != SudokuType.Sukaku
			? ToString("#").CompareTo(other.ToString("#"))
			: throw new InvalidOperationException(SR.ExceptionMessage("ComparableGridMustBeStandard"));

	/// <inheritdoc cref="object.ToString"/>
	public override readonly string ToString() => PuzzleType == SudokuType.Sukaku ? ToString("~") : ToString(null, null);

	/// <remarks>
	/// <para>You can use format identifiers to create the format text. All valid format identifiers:
	/// <list type="table">
	/// <listheader>
	/// <term>Format identifier</term>
	/// <description>Meaning</description>
	/// </listheader>
	/// <item>
	/// <term><c>.</c></term>
	/// <description>Dots are placeholders</description>
	/// </item>
	/// <item>
	/// <term><c>0</c></term>
	/// <description>Zero characters are placeholders</description>
	/// </item>
	/// <item>
	/// <term><c>+</c></term>
	/// <description>Output text will include modifiable digits (Susser format only)</description>
	/// </item>
	/// <item>
	/// <term><c>:</c></term>
	/// <description>Output text will include eliminations (for both Susser and multiline formats)</description>
	/// </item>
	/// <item>
	/// <term><c>!</c></term>
	/// <description>Output text will treat modifiable digits as givens</description>
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly string ToString(string? format) => ToString(format, null);

	/// <inheritdoc cref="ToString(string?, IFormatProvider?)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly string ToString(IFormatProvider? formatProvider)
		=> formatProvider switch
		{
			GridFormatInfo<Grid> f => f.FormatCore(this),
			CultureInfo c => (GridFormatInfo<Grid>.GetInstance(c) ?? new SusserGridFormatInfo<Grid>()).FormatCore(this),
			_ => throw new FormatException()
		};

	/// <inheritdoc/>
	/// <remarks>
	/// For more information about <paramref name="format"/>, please see documentation comments of method
	/// <see cref="ToString(string?)"/>.
	/// </remarks>
	/// <seealso cref="ToString(string?)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly string ToString(string? format, IFormatProvider? formatProvider)
		=> (this, formatProvider) switch
		{
			({ IsEmpty: true }, _) => $"<{nameof(Empty)}>",
			({ IsUndefined: true }, _) => $"<{nameof(Undefined)}>",
			(_, GridFormatInfo<Grid> f) => f.FormatCore(this),
			(_, CultureInfo c) => ToString(c),
			(_, not null) when formatProvider.GetFormat(typeof(GridFormatInfo<Grid>)) is GridFormatInfo<Grid> g => g.FormatCore(this),
			_ => GridFormatInfo<Grid>.GetInstance(format)!.FormatCore(this)
		};

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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly Mask[] ToMaskArray() => this[..].ToArray();

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly Mask GetCandidates(Cell cell) => (Mask)(this[cell] & MaxCandidatesMask);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly CellState GetState(Cell cell) => MaskOperations.MaskToCellState(this[cell]);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly Digit GetDigit(Cell cell)
		=> GetState(cell) switch
		{
			CellState.Empty => -1,
			CellState.Modifiable or CellState.Given => BitOperations.TrailingZeroCount(this[cell]),
			_ => throw new InvalidOperationException(SR.ExceptionMessage("GridInvalidCellState"))
		};

	/// <inheritdoc/>
	public void Reset()
	{
		if (PuzzleType != SudokuType.Standard)
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ResetCandidates()
	{
		if (PuzzleType != SudokuType.Standard)
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
		if (PuzzleType != SudokuType.Standard)
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
		if (PuzzleType != SudokuType.Standard)
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Apply(Conclusion conclusion)
	{
		_ = conclusion is var (type, cell, digit);
		switch (type)
		{
			case Assignment:
			{
				SetDigit(cell, digit);
				break;
			}
			case Elimination:
			{
				SetExistence(cell, digit, false);
				break;
			}
		}
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetState(Cell cell, CellState state)
	{
		ref var mask = ref this[cell];
		mask = (Mask)(GetHeaderBits(cell) | (Mask)((int)state << 9) | mask & MaxCandidatesMask);
		OnValueChanged(ref this, cell, -1);
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetCandidates(Cell cell, Mask mask)
		=> SetMask(cell, (Mask)(GetHeaderBits(cell) | (Mask)((int)GetState(cell) << 9) | mask & MaxCandidatesMask));

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ReplaceDigit(Cell cell, Digit digit)
	{
		ArgumentOutOfRangeException.ThrowIfOutOfRange(digit, 0, 9);

		SetDigit(cell, -1);
		SetDigit(cell, digit);
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
	/// Gets the header 4 bits. The value can be <see cref="SudokuType.Sukaku"/> if and only if the puzzle is Sukaku,
	/// and the argument <paramref name="cell"/> is 0.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <returns>The header 4 bits, represented as a <see cref="Mask"/>, left-shifted.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal readonly Mask GetHeaderBits(Cell cell) => (Mask)(this[cell] & ~((1 << GridBase.HeaderShift) - 1));

	/// <summary>
	/// Gets the header 4 bits. The value can be <see cref="SudokuType.Sukaku"/> if and only if the puzzle is Sukaku,
	/// and the argument <paramref name="cell"/> is 0.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <returns>The header 4 bits, represented as a <see cref="Mask"/>.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal readonly Mask GetHeaderBitsUnshifted(Cell cell) => (Mask)(this[cell] >> GridBase.HeaderShift);

	/// <summary>
	/// Appends for Sukaku puzzle header.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void AddSukakuHeader() => this[0] |= GridBase.SukakuHeader;

	/// <summary>
	/// Removes for Sukaku puzzle header.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void RemoveSukakuHeader() => this[0] &= (1 << GridBase.HeaderShift) - 1;

	/// <inheritdoc/>
	[UnscopedRef]
	readonly ReadOnlySpan<Mask> IInlineArray<Grid, Mask>.AsReadOnlySpan() => this;

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
			current = current.SwapDigit(d1, d2);
		}
		return current;
	}

	/// <inheritdoc/>
	readonly IEnumerator<Digit> IEnumerable<Digit>.GetEnumerator() => ToDigitsArray().AsEnumerable().GetEnumerator();

	/// <inheritdoc/>
	readonly IEnumerable<Candidate> IWhereMethod<Grid, Candidate>.Where(Func<Candidate, bool> predicate)
		=> this.Where(predicate).ToArray();

	/// <inheritdoc/>
	readonly IEnumerable<TResult> ISelectMethod<Grid, Candidate>.Select<TResult>(Func<Candidate, TResult> selector)
		=> this.Select(selector).ToArray();

	/// <inheritdoc/>
	Grid IBoardTransformable<Grid>.MirrorLeftRight() => this.MirrorLeftRight();

	/// <inheritdoc/>
	Grid IBoardTransformable<Grid>.MirrorTopBottom() => this.MirrorTopBottom();

	/// <inheritdoc/>
	Grid IBoardTransformable<Grid>.MirrorDiagonal() => this.MirrorDiagonal();

	/// <inheritdoc/>
	Grid IBoardTransformable<Grid>.MirrorAntidiagonal() => this.MirrorAntidiagonal();

	/// <inheritdoc/>
	Grid IBoardTransformable<Grid>.RotateClockwise() => this.RotateClockwise();

	/// <inheritdoc/>
	Grid IBoardTransformable<Grid>.RotateCounterclockwise() => this.RotateCounterclockwise();

	/// <inheritdoc/>
	Grid IElementSwappingTransformable<Grid, Digit>.SwapElement(Digit element1, Digit element2) => this.SwapDigit(element1, element2);

	/// <inheritdoc/>
	[UnscopedRef]
	Span<Mask> IInlineArray<Grid, Mask>.AsSpan() => this;

	/// <inheritdoc/>
	[UnscopedRef]
	ref Mask IInlineArray<Grid, Mask>.GetPinnableReference() => ref this[0];

	/// <summary>
	/// Gets a sudoku grid, removing all value digits not appearing in the specified <paramref name="pattern"/>.
	/// </summary>
	/// <param name="pattern">The pattern.</param>
	/// <returns>The result grid.</returns>
	private readonly Grid Preserve(in CellMap pattern)
	{
		if (PuzzleType != SudokuType.Standard)
		{
			return this;
		}

		var result = this;
		foreach (var cell in ~pattern)
		{
			result.SetDigit(cell, -1);
		}
		return result;
	}


	/// <inheritdoc/>
	public static bool TryParse(string? s, out Grid result)
	{
		try
		{
			result = Parse(s);
			return !result.IsUndefined;
		}
		catch (FormatException)
		{
			result = Undefined;
			return false;
		}
	}

	/// <inheritdoc/>
	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out Grid result)
	{
		try
		{
			if (s is null)
			{
				result = Undefined;
				return false;
			}

			result = Parse(s, provider);
			return !result.IsUndefined;
		}
		catch (FormatException)
		{
			result = Undefined;
			return false;
		}
	}

	/// <inheritdoc cref="TryParse(ReadOnlySpan{char}, IFormatProvider?, out Grid)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryParse(ReadOnlySpan<char> s, out Grid result) => TryParse(s, null, out result);

	/// <inheritdoc/>
	public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Grid result)
	{
		try
		{
			result = Parse(s, provider);
			return !result.IsUndefined;
		}
		catch (FormatException)
		{
			result = Undefined;
			return false;
		}
	}

	/// <summary>
	/// Creates a <see cref="Grid"/> instance using grid values.
	/// </summary>
	/// <param name="gridValues">The array of grid values.</param>
	/// <param name="creatingOption">The grid creating option.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Grid Create(Digit[] gridValues, GridCreatingOption creatingOption = 0) => new(in gridValues[0], creatingOption);

	/// <summary>
	/// Creates a <see cref="Grid"/> instance via the array of cell digits
	/// of type <see cref="ReadOnlySpan{T}"/> of <see cref="Digit"/>.
	/// </summary>
	/// <param name="gridValues">The list of cell digits.</param>
	/// <param name="creatingOption">The grid creating option.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Grid Create(ReadOnlySpan<Digit> gridValues, GridCreatingOption creatingOption = 0)
		=> new(in gridValues[0], creatingOption);

	/// <inheritdoc/>
	public static Grid Parse(string? s)
	{
		if (s is null)
		{
			throw new FormatException();
		}

		var parsers = (GridFormatInfo<Grid>[])[
			new MultipleLineGridFormatInfo(),
			new MultipleLineGridFormatInfo { RemoveGridLines = true },
			new PencilmarkGridFormatInfo(),
			new SusserGridFormatInfo<Grid>(),
			new SusserGridFormatInfo<Grid> { ShortenSusser = true },
			new CsvGridFormatInfo(),
			new OpenSudokuGridFormatInfo(),
			new SukakuGridFormatInfo(),
			new SukakuGridFormatInfo { Multiline = true }
		];

		// The core branches on parsing grids. Here we may leave a bug that we cannot determine if a puzzle is a Sukaku.
		var grid = Undefined;
		switch (s.Length, s.Contains("-+-"), s.Contains('\t'))
		{
			case (729, _, _) when parseAsSukaku(s, out var g): return g;
			case (_, false, true) when parseAsExcel(s, out var g): return g;
			case (_, true, _) when parseMultipleLines(s, out var g): grid = g; break;
			case var _ when parseAll(s, out var g): grid = g; break;
		}
		if (grid.IsUndefined)
		{
			return Undefined;
		}

#if TRANSFORM_LESS_GIVEN_STANDARD_SUDOKU_PUZZLES_TO_SUKAKU
		// Here need an extra check. Sukaku puzzles can be output as a normal pencil-mark grid format.
		// We should check whether the puzzle is a Sukaku in fact or not.
		// This is a bug fix for pencilmark grid parser, which cannot determine whether a puzzle is a Sukaku.
		// I define that a Sukaku must contain 0 given cells, meaning all values should be candidates or modifiable values.
		// If so, we should treat it as a Sukaku instead of a standard sudoku puzzle.
		if (grid.GivenCellsCount < 17)
		{
			reduceGivenCells(ref grid);
			grid.AddSukakuHeader();
		}
#endif

		return grid;


#if TRANSFORM_LESS_GIVEN_STANDARD_SUDOKU_PUZZLES_TO_SUKAKU
		static void reduceGivenCells(ref Grid grid)
		{
			foreach (ref var mask in grid)
			{
				if (MaskOperations.MaskToCellState(mask) != CellState.Empty)
				{
					mask = (Mask)((int)CellState.Empty << 9 | mask & MaxCandidatesMask);
				}
			}
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool parseAsSukaku(string str, out Grid result)
		{
			if (new SukakuGridFormatInfo().ParseCore(str) is { IsUndefined: false } g)
			{
				g.AddSukakuHeader();
				result = g;
				return true;
			}

			result = Undefined;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool parseAsExcel(string str, out Grid result)
		{
			if (new CsvGridFormatInfo().ParseCore(str) is { IsUndefined: false } g)
			{
				result = g;
				return true;
			}

			result = Undefined;
			return false;
		}

		bool parseMultipleLines(string str, out Grid result)
		{
			foreach (var parser in parsers[..3])
			{
				if (parser.ParseCore(str) is { IsUndefined: false } g)
				{
					result = g;
					return true;
				}
			}

			result = Undefined;
			return false;
		}

		bool parseAll(string str, out Grid result)
		{
			foreach (var currentParser in parsers)
			{
				if (currentParser.ParseCore(str) is { IsUndefined: false } g)
				{
					result = g;
					return true;
				}
			}

			result = Undefined;
			return false;
		}
	}

	/// <inheritdoc/>
	public static Grid Parse(string s, IFormatProvider? provider)
		=> provider switch
		{
			GridFormatInfo<Grid> g => g.ParseCore(s),
			CultureInfo { Name: var n } => n switch
			{
				SR.EnglishLanguage => new PencilmarkGridFormatInfo().ParseCore(s),
				SR.ChineseLanguage => new SusserGridFormatInfo<Grid>().ParseCore(s),
				_ => Parse(s)
			},
			_ => Parse(s)
		};

	/// <inheritdoc cref="ISpanParsable{TSelf}.Parse(ReadOnlySpan{char}, IFormatProvider?)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Grid Parse(ReadOnlySpan<char> s) => Parse(s, null);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Grid Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s.ToString(), provider);

	/// <summary>
	/// Creates an <see cref="IEqualityComparer{T}"/> instance from the basic grid checking rule.
	/// </summary>
	/// <param name="comparison">Indicates the comparison rule.</param>
	/// <returns>An <see cref="IEqualityComparer{T}"/> instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IEqualityComparer<Grid> CreateEqualityComparer(BoardComparison comparison)
		=> EqualityComparer<Grid>.Create((a, b) => a.Equals(in b, comparison), obj => obj.GetHashCode(comparison));

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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
				if (uniformValue == 0) { result[..].Clear(); } else { result[..].Fill(uniformValue); }
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
				throw new InvalidOperationException($"The argument '{nameof(values)}' must contain {81} elements.");
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

		foreach (var peerCell in PeersMap[cell])
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
				foreach (var currentCell in PeersMap[cell])
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


	/// <summary>
	/// Analyzes difference between two grids.
	/// If two grids are not same from given cells, the return value will be <see langword="null"/>.
	/// </summary>
	/// <param name="left">The first grid to be checked.</param>
	/// <param name="right">The second grid to be checked.</param>
	/// <returns>The difference between two grids.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DiffResult? operator -(in Grid left, in Grid right)
	{
		Grid.TryAnalyzeDiff(left, right, out var result);
		return result;
	}

	/// <summary>
	/// Analyzes difference between two grids.
	/// If two grids are not same from given cells, a <see cref="GridDiffTooMuchException"/> instance will be thrown.
	/// </summary>
	/// <param name="left">The first grid to be checked.</param>
	/// <param name="right">The second grid to be checked.</param>
	/// <returns>The difference between two grids.</returns>
	/// <exception cref="GridDiffTooMuchException">Throws when two grids are not same from given cells.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DiffResult operator checked -(in Grid left, in Grid right)
		=> left - right ?? throw new GridDiffTooMuchException();

	/// <inheritdoc/>
	static DiffResult? ISubtractionOperators<Grid, Grid, DiffResult?>.operator -(Grid left, Grid right) => left - right;

	/// <inheritdoc/>
	static DiffResult? ISubtractionOperators<Grid, Grid, DiffResult?>.operator checked -(Grid left, Grid right) => checked(left - right);
}
