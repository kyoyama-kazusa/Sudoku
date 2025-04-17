namespace Sudoku.Analytics.Construction.Patterns;

/// <summary>
/// Defines a data pattern that describes an ALS.
/// </summary>
/// <param name="digitsMask">Indicates the mask of digits used.</param>
/// <param name="cells">Indicates the cells used.</param>
/// <param name="possibleEliminationMap">Gets the possible cells that can store eliminations for the ALS.</param>
/// <param name="eliminationMap">
/// The cells that can be eliminated, grouped by digit. The former 9 elements of the array is the cells
/// that can be eliminated for the corresponding digit, and the last element is the merged cells.
/// </param>
/// <remarks>
/// An <b>Almost Locked Set</b> is a sudoku concept, which describes a case that
/// <c>n</c> cells contains <c>(n + 1)</c> kinds of different digits.
/// The special case is a bi-value cell.
/// </remarks>
[TypeImpl(TypeImplFlags.Object_GetHashCode | TypeImplFlags.Object_ToString | TypeImplFlags.ComparisonOperators)]
public sealed partial class AlmostLockedSetPattern(
	[Property, HashCodeMember] Mask digitsMask,
	[Property, HashCodeMember] in CellMap cells,
	[Property] in CellMap possibleEliminationMap,
	[Property] CellMap[] eliminationMap
) :
	Pattern,
	IComparable<AlmostLockedSetPattern>,
	IComparisonOperators<AlmostLockedSetPattern, AlmostLockedSetPattern, bool>,
	IFormattable,
	IParsable<AlmostLockedSetPattern>
{
	/// <summary>
	/// Indicates an array of the total number of the strong relations in an ALS of the different size.
	/// The field is only unused in the property <see cref="StrongLinks"/>.
	/// </summary>
	/// <seealso cref="StrongLinks"/>
	private static readonly int[] StrongRelationsCount = [0, 1, 3, 6, 10, 15, 21, 28, 36, 45];


	/// <summary>
	/// Indicates whether the ALS only uses a bi-value cell.
	/// </summary>
	public bool IsBivalueCell
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => Cells.Count == 1;
	}

	/// <inheritdoc/>
	public override bool IsChainingCompatible => true;

	/// <inheritdoc/>
	public override PatternType Type => PatternType.AlmostLockedSet;

	/// <summary>
	/// Indicates the house used.
	/// </summary>
	public House House
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => Cells.FirstSharedHouse;
	}

	/// <summary>
	/// Indicates all strong links in this ALS.
	/// The result will be represented as a <see cref="Mask"/> of 9 bits indicating which bits used.
	/// </summary>
	public ReadOnlySpan<Mask> StrongLinks
	{
		get
		{
			var digits = DigitsMask.AllSets;
			var result = new Mask[StrongRelationsCount[digits.Length - 1]];
			for (var (i, x, l) = (0, 0, digits.Length); i < l - 1; i++)
			{
				for (var j = i + 1; j < l; j++)
				{
					result[x++] = (Mask)(1 << digits[i] | 1 << digits[j]);
				}
			}
			return result;
		}
	}


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out Mask digitsMask, out CellMap cells) => (digitsMask, cells) = (DigitsMask, Cells);

	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out Mask digitsMask, out CellMap cells, out CellMap possibleEliminationMap)
		=> ((digitsMask, cells), possibleEliminationMap) = (this, PossibleEliminationMap);

	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out Mask digitsMask, out CellMap cells, out CellMap possibleEliminationMap, out CellMap[] eliminationMap)
		=> ((digitsMask, cells, possibleEliminationMap), eliminationMap) = (this, EliminationMap);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Pattern? other)
		=> other is AlmostLockedSetPattern comparer && DigitsMask == comparer.DigitsMask && Cells == comparer.Cells;

	/// <inheritdoc/>
	/// <exception cref="ArgumentNullException">Throws when the argument <paramref name="other"/> is <see langword="null"/>.</exception>
	public int CompareTo(AlmostLockedSetPattern? other)
		=> other is null
			? throw new ArgumentNullException(nameof(other))
			: Cells.Count.CompareTo(other.Cells.Count) is var p and not 0 ? p : Cells.CompareTo(other.Cells);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public string ToString(IFormatProvider? formatProvider)
	{
		var converter = CoordinateConverter.GetInstance(formatProvider);
		var digitsStr = converter.DigitConverter(DigitsMask);
		var houseStr = converter.HouseConverter(1 << House);
		var cellsStr = converter.CellConverter(Cells);
		return IsBivalueCell
			? $"{digitsStr}/{cellsStr}"
			: $"{digitsStr}/{cellsStr} {SR.Get("KeywordIn", converter.CurrentCulture ?? CultureInfo.CurrentUICulture)} {houseStr}";
	}

	/// <inheritdoc/>
	public override AlmostLockedSetPattern Clone() => new(DigitsMask, Cells, PossibleEliminationMap, EliminationMap);

	/// <inheritdoc/>
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(formatProvider);


	/// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
	public static bool TryParse(string s, [NotNullWhen(true)] out AlmostLockedSetPattern? result) => TryParse(s, null, out result);

	/// <inheritdoc/>
	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [NotNullWhen(true)] out AlmostLockedSetPattern? result)
	{
		try
		{
			if (s is null)
			{
				throw new FormatException();
			}

			result = Parse(s, provider);
			return true;
		}
		catch (FormatException)
		{
			result = null;
			return false;
		}
	}

	/// <summary>
	/// Collects all possible <see cref="AlmostLockedSetPattern"/>s in the specified grid.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <returns>All possible found <see cref="AlmostLockedSetPattern"/> instances.</returns>
	[Cached]
	public static ReadOnlySpan<AlmostLockedSetPattern> Collect(in Grid grid)
	{
		// INTERCEPTOR_VARIABLE_DECLARATION_BEGIN
		_ = grid is { EmptyCells: var __EmptyCells, BivalueCells: var __BivalueCells, CandidatesMap: var __CandidatesMap };
		// INTERCEPTOR_VARIABLE_DECLARATION_END

		// Get all bi-value-cell ALSes.
		var result = new List<AlmostLockedSetPattern>();
		foreach (var cell in __BivalueCells)
		{
			var eliminationMap = new CellMap[9];
			foreach (var digit in grid.GetCandidates(cell))
			{
				eliminationMap[digit] = PeersMap[cell] & __CandidatesMap[digit];
			}

			result.Add(new(grid.GetCandidates(cell), in cell.AsCellMap(), PeersMap[cell] & __EmptyCells, eliminationMap));
		}

		// Get all non-bi-value-cell ALSes.
		for (var house = 0; house < 27; house++)
		{
			if ((HousesMap[house] & __EmptyCells) is not { Count: >= 3 } tempMap)
			{
				continue;
			}

			for (var size = 2; size <= tempMap.Count - 1; size++)
			{
				foreach (ref readonly var map in tempMap & size)
				{
					var blockMask = map.BlockMask;
					if (Mask.IsPow2(blockMask) && house >= 9)
					{
						// All ALS cells lying on a box-row or a box-column
						// will be processed as a block ALS.
						continue;
					}

					// Get all candidates in these cells.
					var digitsMask = grid[map];
					if (Mask.PopCount(digitsMask) - 1 != size)
					{
						continue;
					}

					var eliminationMap = new CellMap[9];
					foreach (var digit in digitsMask)
					{
						eliminationMap[digit] = map % __CandidatesMap[digit];
					}

					var coveredLine = map.SharedLine;
					result.Add(
						new(
							digitsMask,
							map,
							house < 9 && coveredLine is >= 9 and not 32
								? (HousesMap[house] | HousesMap[coveredLine]) & __EmptyCells & ~map
								: tempMap & ~map,
							eliminationMap
						)
					);
				}
			}
		}
		return result.AsSpan();
	}

	/// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
	public static AlmostLockedSetPattern Parse(string s) => Parse(s, null);

	/// <inheritdoc/>
	public static AlmostLockedSetPattern Parse(string s, IFormatProvider? provider)
	{
		const StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
		var parser = CoordinateParser.GetInstance(provider);
		return s.Split('/', options) is [var digitsStr, var cellsStrAndHouseStr]
			? cellsStrAndHouseStr.Split(' ', options) is [var cellsStr, _, _]
				? new(parser.DigitParser(digitsStr), parser.CellParser(cellsStr), [], [])
				: throw new FormatException(SR.ExceptionMessage("AlsMissingCellsInTargetHouse"))
			: throw new FormatException(SR.ExceptionMessage("AlsMissingSlash"));
	}


	/// <inheritdoc/>
	static bool IEqualityOperators<AlmostLockedSetPattern, AlmostLockedSetPattern, bool>.operator ==(AlmostLockedSetPattern? left, AlmostLockedSetPattern? right)
		=> left == right;

	/// <inheritdoc/>
	static bool IEqualityOperators<AlmostLockedSetPattern, AlmostLockedSetPattern, bool>.operator !=(AlmostLockedSetPattern? left, AlmostLockedSetPattern? right)
		=> left != right;
}
