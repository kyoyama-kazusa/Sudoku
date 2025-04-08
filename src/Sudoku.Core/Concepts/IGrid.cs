namespace Sudoku.Concepts;

/// <summary>
/// Represents a type that supports all basic functions that operates with a sudoku puzzle.
/// </summary>
/// <typeparam name="TSelf"><include file="../../global-doc-comments.xml" path="/g/self-type-constraint"/></typeparam>
public interface IGrid<TSelf> :
	IBoard,
	IBoardTransformable<TSelf>,
	IComparable<TSelf>,
	IComparisonOperators<TSelf, TSelf, bool>,
	IElementSwappingTransformable<TSelf, Digit>,
	IEnumerable<Digit>,
	IEquatable<TSelf>,
	IEqualityOperators<TSelf, TSelf, bool>,
	IFormattable,
	IInlineArray<TSelf, Mask>,
	IMinMaxValue<TSelf>,
	IParsable<TSelf>,
	IReadOnlyCollection<Digit>,
	ISelectMethod<TSelf, Candidate>,
	ISpanFormattable,
	ISpanParsable<TSelf>,
	IToArrayMethod<TSelf, Digit>,
	IWhereMethod<TSelf, Candidate>
	where TSelf : unmanaged, IGrid<TSelf>
{
	/// <summary>
	/// Indicates the shifting bits count for header bits.
	/// </summary>
	protected internal const int HeaderShift = 9 + 3;

	/// <summary>
	/// Indicates ths header bits describing the sudoku type is a Sukaku.
	/// </summary>
	protected internal const Mask SukakuHeader = (int)SudokuType.Sukaku << HeaderShift;


	/// <summary>
	/// Determines whether the current grid contains any missing candidates.
	/// </summary>
	public abstract bool IsMissingCandidates { get; }

	/// <summary>
	/// Indicates whether the grid is <see cref="Empty"/>, which means the grid holds totally same value with <see cref="Empty"/>.
	/// </summary>
	/// <seealso cref="Empty"/>
	public virtual bool IsEmpty => (TSelf)this == TSelf.Empty;

	/// <summary>
	/// Indicates whether the grid is <see cref="Undefined"/>, which means the grid holds totally same value with <see cref="Undefined"/>.
	/// </summary>
	/// <seealso cref="Undefined"/>
	public virtual bool IsUndefined => (TSelf)this == TSelf.Undefined;

	/// <summary>
	/// Indicates the grid has already solved. If the value is <see langword="true"/>, the grid is solved;
	/// otherwise, <see langword="false"/>.
	/// </summary>
	public abstract bool IsSolved { get; }

	/// <summary>
	/// Try to get the symmetry of the puzzle.
	/// </summary>
	public virtual SymmetricType Symmetry => GivenCells.Symmetry;

	/// <summary>
	/// Indicates the total number of given cells.
	/// </summary>
	public virtual Cell GivenCellsCount => GivenCells.Count;

	/// <summary>
	/// Indicates the total number of modifiable cells.
	/// </summary>
	public virtual Cell ModifiableCellsCount => ModifiableCells.Count;

	/// <summary>
	/// Indicates the total number of empty cells.
	/// </summary>
	public virtual Cell EmptyCellsCount => EmptyCells.Count;

	/// <summary>
	/// Gets a cell list that only contains the given cells.
	/// </summary>
	public abstract CellMap GivenCells { get; }

	/// <summary>
	/// Gets a cell list that only contains the modifiable cells.
	/// </summary>
	public abstract CellMap ModifiableCells { get; }

	/// <summary>
	/// Indicates a cell list whose corresponding position in this grid is empty.
	/// </summary>
	public abstract CellMap EmptyCells { get; }

	/// <summary>
	/// Indicates a cell list whose corresponding position in this grid contain two candidates.
	/// </summary>
	public abstract CellMap BivalueCells { get; }

	/// <summary>
	/// Indicates the number of total candidates.
	/// </summary>
	public abstract Candidate CandidatesCount { get; }

	/// <summary>
	/// Indicates the map of possible positions of the existence of the candidate value for each digit.
	/// The return value will be an array of 9 elements, which stands for the statuses of 9 digits.
	/// </summary>
	public abstract ReadOnlySpan<CellMap> CandidatesMap { get; }

	/// <summary>
	/// <para>
	/// Indicates the map of possible positions of the existence of each digit. The return value will
	/// be an array of 9 elements, which stands for the statuses of 9 digits.
	/// </para>
	/// <para>
	/// Different with <see cref="CandidatesMap"/>, this property contains all givens, modifiables and
	/// empty cells only if it contains the digit in the mask.
	/// </para>
	/// </summary>
	/// <seealso cref="CandidatesMap"/>
	public abstract ReadOnlySpan<CellMap> DigitsMap { get; }

	/// <summary>
	/// <para>
	/// Indicates the map of possible positions of the existence of that value of each digit.
	/// The return value will be an array of 9 elements, which stands for the statuses of 9 digits.
	/// </para>
	/// <para>
	/// Different with <see cref="CandidatesMap"/>, the value only contains the given or modifiable
	/// cells whose mask contain the set bit of that digit.
	/// </para>
	/// </summary>
	/// <seealso cref="CandidatesMap"/>
	public abstract ReadOnlySpan<CellMap> ValuesMap { get; }

	/// <summary>
	/// Indicates all possible candidates in the current grid.
	/// </summary>
	public abstract ReadOnlySpan<Candidate> Candidates { get; }

	/// <summary>
	/// Indicates all possible conjugate pairs appeared in this grid.
	/// </summary>
	public abstract ReadOnlySpan<Conjugate> ConjugatePairs { get; }

	/// <summary>
	/// <para>Indicates which houses are empty houses.</para>
	/// <para>An <b>Empty House</b> is a house holding 9 empty cells, i.e. all cells in this house are empty.</para>
	/// <para>
	/// The property returns a <see cref="HouseMask"/> value as a mask that contains all possible house indices.
	/// For example, if the row 5, column 5 and block 5 (1-9) are null houses, the property will return
	/// the result <see cref="HouseMask"/> value, <c>000010000_000010000_000010000</c> as binary.
	/// </para>
	/// </summary>
	public abstract HouseMask EmptyHouses { get; }

	/// <summary>
	/// <para>Indicates which houses are completed, regardless of ways of filling.</para>
	/// <para><inheritdoc cref="EmptyHouses" path="//summary/para[3]"/></para>
	/// </summary>
	public abstract HouseMask CompletedHouses { get; }

	/// <summary>
	/// Gets the grid where all modifiable cells are empty cells (i.e. the initial one).
	/// </summary>
	public abstract TSelf ResetGrid { get; }

	/// <summary>
	/// Indicates the unfixed grid for the current grid, meaning all given digits will be replaced with modifiable ones.
	/// </summary>
	public abstract TSelf UnfixedGrid { get; }

	/// <summary>
	/// Indicates the fixed grid for the current grid, meaning all modifiable digits will be replaced with given ones.
	/// </summary>
	public abstract TSelf FixedGrid { get; }

	/// <summary>
	/// Indicates the inner array that stores the masks of the sudoku grid, which stores the in-time sudoku grid inner information.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The field uses the mask table of length 81 to indicate the state and all possible candidates
	/// holding for each cell. Each mask uses a <see cref="Mask"/> value, but only uses 11 of 16 bits.
	/// <code>
	/// | 15  14  13  12  11  10  9   8   7   6   5   4   3   2   1   0 |
	/// |---------------|-----------|-----------------------------------|
	/// |   |   |   |   | 0 | 0 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 |
	/// '---------------|-----------|-----------------------------------'
	///  \_____________/ \_________/ \_________________________________/
	///        (3)           (2)                     (1)
	/// </code>
	/// Here the 9 bits in (1) indicate whether each digit is possible candidate in the current cell for each bit respectively,
	/// and the higher 3 bits in (2) indicate the cell state. The possible cell state are:
	/// <list type="table">
	/// <listheader>
	/// <term>State name</term>
	/// <description>Description</description>
	/// </listheader>
	/// <item>
	/// <term>Empty cell (flag: <see cref="CellState.Empty"/>)</term>
	/// <description>The cell is currently empty, and wait for being filled.</description>
	/// </item>
	/// <item>
	/// <term>Modifiable cell (flag: <see cref="CellState.Modifiable"/>)</term>
	/// <description>The cell is filled by a digit, but the digit isn't the given by the initial grid.</description>
	/// </item>
	/// <item>
	/// <term>Given cell (flag: <see cref="CellState.Given"/>)</term>
	/// <description>The cell is filled by a digit, which is given by the initial grid and can't be modified.</description>
	/// </item>
	/// </list>
	/// </para>
	/// <para>Part (3) is for reserved cases. Such bits may not be used.</para>
	/// </remarks>
	/// <seealso cref="CellState"/>
	/// <seealso cref="SudokuType"/>
	[UnscopedRef]
	protected abstract ref readonly Mask FirstMaskRef { get; }

	/// <inheritdoc/>
	int IBoard.Rows => 9;

	/// <inheritdoc/>
	int IBoard.Columns => 9;


	/// <summary>
	/// Represents a string value that describes a <typeparamref name="TSelf"/> instance can be parsed into <see cref="Empty"/>.
	/// </summary>
	/// <seealso cref="Empty"/>
	public static abstract string EmptyString { get; }

	/// <summary>
	/// Indicates the default mask of a cell (an empty cell, with all 9 candidates left).
	/// </summary>
	public static virtual Mask DefaultMask => (Mask)(TSelf.EmptyMask | TSelf.MaxCandidatesMask);

	/// <summary>
	/// Indicates the empty mask, modifiable mask and given mask.
	/// </summary>
	public static abstract Mask EmptyMask { get; }

	/// <summary>
	/// Indicates the modifiable mask.
	/// </summary>
	public static abstract Mask ModifiableMask { get; }

	/// <summary>
	/// Indicates the given mask.
	/// </summary>
	public static abstract Mask GivenMask { get; }

	/// <summary>
	/// Indicates the maximum candidate mask that used.
	/// </summary>
	public static abstract Mask MaxCandidatesMask { get; }

	/// <summary>
	/// The empty grid that is valid during implementation or running the program
	/// (all values are <see cref="DefaultMask"/>, i.e. empty cells).
	/// </summary>
	/// <remarks>
	/// This field is initialized by the static constructor of this structure.
	/// </remarks>
	/// <seealso cref="DefaultMask"/>
	public static abstract ref readonly TSelf Empty { get; }

	/// <summary>
	/// Indicates the default grid that all values are initialized 0.
	/// This value is equivalent to <see langword="default"/>(<typeparamref name="TSelf"/>).
	/// </summary>
	/// <remarks>
	/// This value can be used for non-candidate-based sudoku operations, e.g. a sudoku grid canvas.
	/// </remarks>
	public static abstract ref readonly TSelf Undefined { get; }

	/// <summary>
	/// Indicates the minimum possible grid value that the current type can reach.
	/// </summary>
	/// <remarks>
	/// This value is found out via backtracking algorithm.
	/// </remarks>
	static TSelf IMinMaxValue<TSelf>.MinValue => TSelf.Parse("123456789456789123789123456214365897365897214897214365531642978642978531978531642");

	/// <summary>
	/// Indicates the maximum possible grid value that the current type can reach.
	/// </summary>
	/// <remarks>
	/// This value is found out via backtracking algorithm.
	/// </remarks>
	static TSelf IMinMaxValue<TSelf>.MaxValue => TSelf.Parse("987654321654321987321987654896745213745213896213896745579468132468132579132579468");


	/// <summary>
	/// Creates a mask of type <see cref="Mask"/> that represents the usages of digits 1 to 9,
	/// ranged in a specified list of cells in the current sudoku grid.
	/// </summary>
	/// <param name="cells">A list of desired cells.</param>
	/// <returns>A mask of type <see cref="Mask"/> that represents the usages of digits 1 to 9.</returns>
	public abstract Mask this[in CellMap cells] { get; }

	/// <summary>
	/// <inheritdoc cref="this[in CellMap]" path="/summary"/>
	/// </summary>
	/// <param name="cells"><inheritdoc cref="this[in CellMap]" path="/param[@name='cells']"/></param>
	/// <param name="withValueCells">
	/// Indicates whether the value cells (given or modifiable ones) will be included to be checked.
	/// If <see langword="true"/>, all value cells (no matter what kind of cell) will be summed up.
	/// </param>
	/// <param name="aggregator">
	/// Indicates the aggregator method.
	/// <list type="table">
	/// <listheader>
	/// <term>Value</term>
	/// <description>Meaning</description>
	/// </listheader>
	/// <item>
	/// <term><see cref="MaskAggregator.And"/></term>
	/// <description>Use <b>bitwise and</b> operator to merge masks</description>
	/// </item>
	/// <item>
	/// <term><see cref="MaskAggregator.Or"/></term>
	/// <description>Use <b>bitwise or</b> operator to merge masks</description>
	/// </item>
	/// <item>
	/// <term><see cref="MaskAggregator.AndNot"/></term>
	/// <description>Use <b>bitwise nand</b> operator to merge masks</description>
	/// </item>
	/// </list>
	/// By default, the value is <see cref="MaskAggregator.Or"/>.
	/// You can reference <see cref="MaskAggregator"/> constants to set values.
	/// </param>
	/// <returns><inheritdoc cref="this[in CellMap]" path="/returns"/></returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when <paramref name="aggregator"/> is not defined.</exception>
	public abstract Mask this[in CellMap cells, bool withValueCells, MaskAggregator aggregator = MaskAggregator.Or] { get; }


	/// <summary>
	/// Reset the sudoku grid, making all modifiable values to empty ones.
	/// </summary>
	public abstract void Reset();

	/// <summary>
	/// Fix the current grid, making all modifiable values will be changed to given ones.
	/// </summary>
	public abstract void Fix();

	/// <summary>
	/// Unfix the current grid, making all given values will be changed to modifiable ones.
	/// </summary>
	public abstract void Unfix();

	/// <summary>
	/// Try to apply the specified conclusion.
	/// </summary>
	/// <param name="conclusion">The conclusion to be applied.</param>
	public abstract void Apply(Conclusion conclusion);

	/// <summary>
	/// Set the specified cell to the specified state.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <param name="state">The state.</param>
	public abstract void SetState(Cell cell, CellState state);

	/// <summary>
	/// Set the specified cell with specified candidates.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <param name="mask">The mask that holds a list of desired digits.</param>
	public abstract void SetCandidates(Cell cell, Mask mask);

	/// <summary>
	/// Set the specified cell to the specified mask.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <param name="mask">The mask to set.</param>
	public abstract void SetMask(Cell cell, Mask mask);

	/// <summary>
	/// Set the specified digit into the specified cell.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <param name="digit">
	/// <para>
	/// The value you want to set. The value should be between 0 and 8.
	/// If assigning -1, the grid will execute an implicit behavior that candidates in <b>all</b> empty cells will be re-computed.
	/// </para>
	/// <para>
	/// The values set into the grid will be regarded as the modifiable values.
	/// If the cell contains a digit, it will be covered when it is a modifiable value.
	/// If the cell is a given cell, the setter will do nothing.
	/// </para>
	/// </param>
	public abstract void SetDigit(Cell cell, Digit digit);

	/// <summary>
	/// Sets the target candidate state.
	/// </summary>
	/// <param name="cell">The cell offset between 0 and 80.</param>
	/// <param name="digit">The digit between 0 and 8.</param>
	/// <param name="isOn">
	/// The case you want to set. <see langword="false"/> means that this candidate
	/// doesn't exist in this current sudoku grid; otherwise, <see langword="true"/>.
	/// </param>
	public abstract void SetExistence(Cell cell, Digit digit, bool isOn);

	/// <summary>
	/// Sets a candidate existence case with a <see cref="bool"/> value.
	/// </summary>
	/// <returns>A <see cref="bool"/> value indicating that.</returns>
	/// <inheritdoc cref="SetExistence(Cell, Digit, bool)"/>
	public abstract bool GetExistence(Cell cell, Digit digit);

	/// <inheritdoc cref="object.Equals(object?)"/>
	public abstract bool Equals([NotNullWhen(true)] object? other);

	/// <summary>
	/// Determines whether the current instance has same mask values with the other object.
	/// </summary>
	/// <param name="other">The other instance.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public abstract bool Equals(in TSelf other);

	/// <summary>
	/// Determine whether the digit in the target cell is conflict with a certain cell in the peers of the current cell,
	/// if the digit is filled into the cell.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <param name="digit">The digit.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public abstract bool ConflictWith(Cell cell, Digit digit);

	/// <inheritdoc cref="Exists(Cell, Digit)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual bool? Exists(Candidate candidate) => Exists(candidate / 9, candidate % 9);

	/// <summary>
	/// Indicates whether the current grid contains the digit in the specified cell.
	/// </summary>
	/// <param name="cell">The cell to be checked.</param>
	/// <param name="digit">The digit to be checked.</param>
	/// <returns>
	/// The method will return a <see cref="bool"/>? value
	/// (containing three possible cases: <see langword="true"/>, <see langword="false"/> and <see langword="null"/>).
	/// All values corresponding to the cases are below:
	/// <list type="table">
	/// <listheader>
	/// <term>Value</term>
	/// <description>Case description on this value</description>
	/// </listheader>
	/// <item>
	/// <term><see langword="true"/></term>
	/// <description>
	/// The cell is an empty cell <b>and</b> contains the specified digit.
	/// </description>
	/// </item>
	/// <item>
	/// <term><see langword="false"/></term>
	/// <description>
	/// The cell is an empty cell <b>but doesn't</b> contain the specified digit.
	/// </description>
	/// </item>
	/// <item>
	/// <term><see langword="null"/></term>
	/// <description>The cell is <b>not</b> an empty cell.</description>
	/// </item>
	/// </list>
	/// </returns>
	/// <remarks>
	/// <para>
	/// Note that the method will return a <see cref="bool"/>?, so you should use the code
	/// '<c>grid.Exists(cell, digit) is true</c>' or '<c>grid.Exists(cell, digit) == true</c>'
	/// to decide whether a condition is true.
	/// </para>
	/// <para>
	/// In addition, because the type is <see cref="bool"/>? rather than <see cref="bool"/>,
	/// the result case will be more precisely than the indexer <see cref="GetExistence(Cell, Digit)"/>,
	/// which is the main difference between this method and that indexer.
	/// </para>
	/// </remarks>
	/// <seealso cref="GetExistence(Cell, Digit)"/>
	public abstract bool? Exists(Cell cell, Digit digit);

	/// <inheritdoc cref="object.GetHashCode"/>
	public abstract int GetHashCode();

	/// <inheritdoc cref="IComparable{T}.CompareTo(T)"/>
	public abstract int CompareTo(in TSelf other);

	/// <inheritdoc cref="object.ToString"/>
	public abstract string ToString();

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public virtual string ToString(IFormatProvider? formatProvider) => ToString(null, formatProvider);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public virtual string ToString(string? format) => ToString(format, null);

	/// <summary>
	/// Get the cell state at the specified cell.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <returns>The cell state.</returns>
	public abstract CellState GetState(Cell cell);

	/// <summary>
	/// Get the candidate mask part of the specified cell.
	/// </summary>
	/// <param name="cell">The cell offset you want to get.</param>
	/// <returns>
	/// <para>
	/// The candidate mask. The return value is a 9-bit <see cref="Mask"/> value, where each bit will be:
	/// <list type="table">
	/// <item>
	/// <term><c>0</c></term>
	/// <description>The cell <b>doesn't contain</b> the possibility of the digit.</description>
	/// </item>
	/// <item>
	/// <term><c>1</c></term>
	/// <description>The cell <b>contains</b> the possibility of the digit.</description>
	/// </item>
	/// </list>
	/// </para>
	/// <para>
	/// For example, if the result mask is 266 (i.e. <c>0b<b>1</b>00_00<b>1</b>_0<b>1</b>0</c> in binary),
	/// the value will indicate the cell contains the digit 2, 4 and 9.
	/// </para>
	/// </returns>
	public abstract Mask GetCandidates(Cell cell);

	/// <summary>
	/// Try to get the digit filled in the specified cell.
	/// </summary>
	/// <param name="cell">The cell used.</param>
	/// <returns>The digit that the current cell filled. If the cell is empty, return -1.</returns>
	/// <exception cref="InvalidOperationException">Throws when the specified cell keeps a wrong cell state value.</exception>
	public abstract Digit GetDigit(Cell cell);

	/// <summary>
	/// Serializes this instance to an array, where all digit value will be stored.
	/// </summary>
	/// <returns>
	/// This array. All elements are the raw masks
	/// that are between 0 and <see cref="MaxCandidatesMask"/> (i.e. 511).
	/// </returns>
	/// <seealso cref="MaxCandidatesMask"/>
	public abstract Mask[] ToCandidateMaskArray();

	/// <summary>
	/// Try to create a new array of <see cref="Digit"/> instances indicating filling digits inside cells.
	/// </summary>
	/// <returns>An array of <see cref="Digit"/> instances.</returns>
	/// <seealso cref="Digit"/>
	public abstract Digit[] ToDigitsArray();

	/// <inheritdoc/>
	bool IEquatable<TSelf>.Equals(TSelf other) => Equals(other);

	/// <inheritdoc/>
	int IComparable<TSelf>.CompareTo(TSelf other) => CompareTo(other);

	/// <inheritdoc/>
	int IReadOnlyCollection<Digit>.Count => 81;

	/// <inheritdoc/>
	Digit[] IToArrayMethod<TSelf, Digit>.ToArray() => ToDigitsArray();

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


	/// <inheritdoc cref="IParsable{TSelf}.TryParse(string?, IFormatProvider?, out TSelf)"/>
	public static new virtual bool TryParse(string? s, IFormatProvider? formatProvider, out TSelf result)
	{
		try
		{
			result = TSelf.Parse(s, formatProvider);
			return true;
		}
		catch (FormatException)
		{
			result = TSelf.Undefined;
			return false;
		}
	}

	/// <inheritdoc cref="ISpanParsable{TSelf}.TryParse(ReadOnlySpan{char}, IFormatProvider?, out TSelf)"/>
	public static new virtual bool TryParse(ReadOnlySpan<char> s, IFormatProvider? formatProvider, out TSelf result)
	{
		try
		{
			result = TSelf.Parse(s, formatProvider);
			return true;
		}
		catch (FormatException)
		{
			result = TSelf.Undefined;
			return false;
		}
	}

	/// <inheritdoc cref="IParsable{TSelf}.TryParse(string?, IFormatProvider?, out TSelf)"/>
	public static virtual bool TryParse(string? s, out TSelf result) => TSelf.TryParse(s, null, out result);

	/// <inheritdoc cref="ISpanParsable{TSelf}.TryParse(ReadOnlySpan{char}, IFormatProvider?, out TSelf)"/>
	public static virtual bool TryParse(ReadOnlySpan<char> s, out TSelf result) => TSelf.TryParse(s, null, out result);

	/// <summary>
	/// Creates a <typeparamref name="TSelf"/> instance via the specified list of <see cref="Mask"/> values.
	/// </summary>
	/// <param name="values">The values to be created.</param>
	/// <returns>A <typeparamref name="TSelf"/> instance created.</returns>
	public static abstract TSelf Create(ReadOnlySpan<Mask> values);

	/// <inheritdoc cref="IParsable{TSelf}.Parse(string?, IFormatProvider?)"/>
	public static virtual TSelf Parse(string? s) => TSelf.Parse(s, null);

	/// <inheritdoc cref="ISpanParsable{TSelf}.Parse(ReadOnlySpan{char}, IFormatProvider?)"/>
	public static virtual TSelf Parse(ReadOnlySpan<char> s) => TSelf.Parse(s, null);

	/// <summary>
	/// Event handler on value changed.
	/// </summary>
	/// <param name="this">The grid itself.</param>
	/// <param name="cell">Indicates the cell changed.</param>
	/// <param name="setValue">
	/// Indicates the set value. If to clear the cell, the value will be -1.
	/// In fact, if the value is -1, this method will do nothing.
	/// </param>
	protected static virtual void OnValueChanged(ref TSelf @this, Cell cell, Digit setValue)
	{
	}

	/// <summary>
	/// Event handler on refreshing candidates.
	/// </summary>
	/// <param name="this">The grid itself.</param>
	protected static virtual void OnRefreshingCandidates(ref TSelf @this)
	{
	}

	/// <summary>
	/// Called by properties <see cref="EmptyCells"/> and <see cref="BivalueCells"/>.
	/// </summary>
	/// <param name="this">The current instance.</param>
	/// <param name="predicate">The predicate.</param>
	/// <returns>The map.</returns>
	/// <seealso cref="EmptyCells"/>
	/// <seealso cref="BivalueCells"/>
	protected static unsafe CellMap GetMap(in TSelf @this, delegate*<in TSelf, Cell, bool> predicate)
	{
		var result = CellMap.Empty;
		for (var cell = 0; cell < 81; cell++)
		{
			if (predicate(@this, cell))
			{
				result.Add(cell);
			}
		}
		return result;
	}

	/// <summary>
	/// Called by properties <see cref="CandidatesMap"/>, <see cref="DigitsMap"/> and <see cref="ValuesMap"/>.
	/// </summary>
	/// <param name="this">The current instance.</param>
	/// <param name="predicate">The predicate.</param>
	/// <returns>The map indexed by each digit.</returns>
	/// <seealso cref="CandidatesMap"/>
	/// <seealso cref="DigitsMap"/>
	/// <seealso cref="ValuesMap"/>
	protected static unsafe CellMap[] GetMaps(in TSelf @this, delegate*<in TSelf, Cell, Digit, bool> predicate)
	{
		var result = new CellMap[9];
		for (var digit = 0; digit < 9; digit++)
		{
			ref var map = ref result[digit];
			for (var cell = 0; cell < 81; cell++)
			{
				if (predicate(@this, cell, digit))
				{
					map.Add(cell);
				}
			}
		}
		return result;
	}


	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)"/>
	public static abstract bool operator ==(in TSelf left, in TSelf right);

	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)"/>
	public static abstract bool operator !=(in TSelf left, in TSelf right);

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)"/>
	public static abstract bool operator >(in TSelf left, in TSelf right);

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)"/>
	public static abstract bool operator >=(in TSelf left, in TSelf right);

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)"/>
	public static abstract bool operator <(in TSelf left, in TSelf right);

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)"/>
	public static abstract bool operator <=(in TSelf left, in TSelf right);

	/// <inheritdoc/>
	static bool IEqualityOperators<TSelf, TSelf, bool>.operator ==(TSelf left, TSelf right) => left == right;

	/// <inheritdoc/>
	static bool IEqualityOperators<TSelf, TSelf, bool>.operator !=(TSelf left, TSelf right) => left != right;

	/// <inheritdoc/>
	static bool IComparisonOperators<TSelf, TSelf, bool>.operator >(TSelf left, TSelf right) => left > right;

	/// <inheritdoc/>
	static bool IComparisonOperators<TSelf, TSelf, bool>.operator <(TSelf left, TSelf right) => left < right;

	/// <inheritdoc/>
	static bool IComparisonOperators<TSelf, TSelf, bool>.operator >=(TSelf left, TSelf right) => left >= right;

	/// <inheritdoc/>
	static bool IComparisonOperators<TSelf, TSelf, bool>.operator <=(TSelf left, TSelf right) => left <= right;
}
