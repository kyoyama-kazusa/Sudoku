namespace Sudoku.Concepts;

using GridBase = IGrid<MarkerGrid>;
using InlineArrayGridBase = IInlineArrayGrid<MarkerGrid>;

/// <summary>
/// Represents a sudoku grid that will be used for candidate marking.
/// Different with <see cref="Grid"/>, this type will aim to appending candidates,
/// rather than removing candidates at initial state with full candidates.
/// </summary>
/// <remarks>
/// <para>
/// The backing implementation is nearly same as <see cref="Grid"/>, but the only difference is that <see cref="Grid"/> instances
/// will contain all candidates at the initial state. Candidates can be removed by using indexers to update candidates,
/// or <see cref="Grid.SetCandidates(Cell, Mask)"/> to replace candidate masks with new values;
/// however, <see cref="MarkerGrid"/> instances won't contain any candidates at initial state.
/// All candicates should be manually appended into grid.
/// </para>
/// <para>This type uses 162 bytes.</para>
/// </remarks>
/// <seealso cref="Grid"/>
/// <seealso cref="Grid.SetCandidates(Cell, Mask)"/>
[CollectionBuilder(typeof(Grid), nameof(Create))]
[InlineArray(81)]
public struct MarkerGrid : InlineArrayGridBase
{
	/// <summary>
	/// <inheritdoc cref="GridBase.Undefined" path="/summary"/>
	/// </summary>
	/// <remarks>
	/// For <see cref="MarkerGrid"/>, <see cref="Undefined"/> is equivalent to <see cref="Empty"/>
	/// because initial states of such two fields are same.
	/// </remarks>
	public static readonly MarkerGrid Undefined = default;

	/// <summary>
	/// <inheritdoc cref="GridBase.Empty" path="/summary"/>
	/// </summary>
	/// <remarks>
	/// For <see cref="MarkerGrid"/>, <see cref="Undefined"/> is equivalent to <see cref="Empty"/>
	/// because initial states of such two fields are same.
	/// </remarks>
	public static readonly MarkerGrid Empty = default;


	/// <inheritdoc cref="InlineArrayGridBase.FirstMaskRef"/>
	private Mask _values;


	/// <summary>
	/// Creates a <see cref="MarkerGrid"/> instance via the pointer of the first element of the cell digit, and the creating option.
	/// </summary>
	/// <param name="firstElement">The reference of the first element.</param>
	/// <param name="creatingOption">The creating option.</param>
	/// <exception cref="ArgumentNullException">
	/// Throws when the argument <paramref name="firstElement"/> is <see langword="null"/> reference.
	/// </exception>
	private MarkerGrid(ref readonly Digit firstElement, GridCreatingOption creatingOption = GridCreatingOption.None)
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
	public readonly Candidate CandidatesCount
	{
		get
		{
			var result = 0;
			for (var cell = 0; cell < 81; cell++)
			{
				result += PopCount((uint)GetCandidates(cell));
			}
			return result;
		}
	}

	/// <inheritdoc/>
	public readonly MarkerGrid ResetGrid => Preserve(GivenCells);

	/// <inheritdoc/>
	public readonly MarkerGrid UnfixedGrid
	{
		get
		{
			var result = this;
			result.Unfix();
			return result;
		}
	}

	/// <inheritdoc/>
	public readonly MarkerGrid FixedGrid
	{
		get
		{
			var result = this;
			result.Fix();
			return result;
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
	public readonly unsafe CellMap GivenCells => GridBase.GetMap(this, &MarkerGrid.IsGivenCell);

	/// <inheritdoc/>
	public readonly unsafe CellMap ModifiableCells => GridBase.GetMap(this, &MarkerGrid.IsModifiableCell);

	/// <inheritdoc/>
	public readonly unsafe CellMap EmptyCells => GridBase.GetMap(this, &MarkerGrid.IsEmptyCell);

	/// <inheritdoc/>
	public readonly unsafe CellMap BivalueCells => GridBase.GetMap(this, &MarkerGrid.IsBivalueCell);

	/// <inheritdoc/>
	public readonly unsafe ReadOnlySpan<CellMap> CandidatesMap => GridBase.GetMaps(this, &MarkerGrid.ExistsInCandidatesMap);

	/// <inheritdoc/>
	public readonly unsafe ReadOnlySpan<CellMap> DigitsMap => GridBase.GetMaps(this, &MarkerGrid.ExistsInDigitsMap);

	/// <inheritdoc/>
	public readonly unsafe ReadOnlySpan<CellMap> ValuesMap => GridBase.GetMaps(this, &MarkerGrid.ExistsInValuesMap);

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
	/// <remarks>
	/// For <see cref="MarkerGrid"/> instances, this property always returns <see langword="true"/>.
	/// </remarks>
	readonly bool GridBase.IsMissingCandidates => true;

	/// <inheritdoc/>
	/// <remarks>
	/// For <see cref="MarkerGrid"/> instances, this property always returns <see langword="false"/>.
	/// </remarks>
	readonly bool GridBase.IsSolved => false;

	/// <inheritdoc/>
	[UnscopedRef]
	readonly ReadOnlySpan<Mask> InlineArrayGridBase.Elements => this[..];

	/// <inheritdoc/>
	[UnscopedRef]
	readonly ref readonly Mask InlineArrayGridBase.FirstMaskRef => ref this[0];


	/// <inheritdoc/>
	static string GridBase.EmptyString => Grid.EmptyString;

	/// <inheritdoc/>
	static Mask InlineArrayGridBase.DefaultMask => Grid.DefaultMask;

	/// <inheritdoc/>
	static Mask InlineArrayGridBase.MaxCandidatesMask => Grid.MaxCandidatesMask;

	/// <inheritdoc/>
	static Mask InlineArrayGridBase.EmptyMask => Grid.EmptyMask;

	/// <inheritdoc/>
	static Mask InlineArrayGridBase.ModifiableMask => Grid.ModifiableMask;

	/// <inheritdoc/>
	static Mask InlineArrayGridBase.GivenMask => Grid.GivenMask;

	/// <inheritdoc/>
	static ref readonly MarkerGrid GridBase.Undefined => ref Undefined;

	/// <inheritdoc/>
	static ref readonly MarkerGrid GridBase.Empty => ref Empty;


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
			return (Mask)(result & Grid.MaxCandidatesMask);
		}
	}

	/// <inheritdoc/>
	public readonly unsafe Mask this[in CellMap cells, bool withValueCells, MaskAggregator aggregator = MaskAggregator.Or]
	{
		get
		{
			var result = aggregator switch
			{
				MaskAggregator.AndNot or MaskAggregator.And => Grid.MaxCandidatesMask,
				MaskAggregator.Or => (Mask)0,
				_ => throw new ArgumentOutOfRangeException(nameof(aggregator))
			};
			delegate*<ref Mask, in MarkerGrid, Cell, void> mergingFunctionPtr = aggregator switch
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
			return (Mask)(result & Grid.MaxCandidatesMask);


			static void andNot(ref Mask result, in MarkerGrid grid, Cell cell) => result &= (Mask)~grid[cell];

			static void and(ref Mask result, in MarkerGrid grid, Cell cell) => result &= grid[cell];

			static void or(ref Mask result, in MarkerGrid grid, Cell cell) => result |= grid[cell];
		}
	}

	/// <inheritdoc/>
	[UnscopedRef]
	ref Mask InlineArrayGridBase.this[Cell cell] => ref this[cell];


	/// <inheritdoc/>
	public readonly override bool Equals([NotNullWhen(true)] object? obj) => obj is MarkerGrid comparer && Equals(comparer);

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

	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	public readonly bool Equals(in MarkerGrid other) => this[..].SequenceEqual(other[..]);

	/// <inheritdoc/>
	public readonly bool? Exists(Cell cell, Digit digit) => GetState(cell) == CellState.Empty ? GetExistence(cell, digit) : null;

	/// <inheritdoc/>
	public readonly override int GetHashCode() => ToString("#").GetHashCode();

	/// <inheritdoc/>
	public readonly int CompareTo(in MarkerGrid other) => ToString("#").CompareTo(other.ToString("#"));

	/// <inheritdoc cref="object.ToString"/>
	public readonly override string ToString() => ToString(null, null);

	/// <inheritdoc/>
	public readonly string ToString(string? format) => ToString(format, null);

	/// <inheritdoc cref="Grid.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(string? format, IFormatProvider? formatProvider)
	{
		var instance = GridConverterFactory.GetInstance(format) as SusserGridConverter ?? new SusserGridDefaultConverter();
		return instance.TryFormat(in this, formatProvider, out var result)
			? result
			: throw new FormatException();
	}

	/// <inheritdoc/>
	public readonly CellState GetState(Cell cell) => MaskToCellState(this[cell]);

	/// <inheritdoc/>
	public readonly Mask GetCandidates(Cell cell) => (Mask)(this[cell] & Grid.MaxCandidatesMask);

	/// <inheritdoc/>
	public readonly Mask[] ToCandidateMaskArray()
	{
		var result = new Mask[81];
		for (var cell = 0; cell < 81; cell++)
		{
			result[cell] = (Mask)(this[cell] & Grid.MaxCandidatesMask);
		}
		return result;
	}

	/// <summary>
	/// Creates an array of <see cref="Mask"/> values that is a copy for the current inline array data structure.
	/// </summary>
	/// <returns>An array of <see cref="Mask"/> values.</returns>
	public readonly Mask[] ToMaskArray() => this[..].ToArray();

	/// <inheritdoc/>
	public readonly Digit GetDigit(Cell cell)
		=> GetState(cell) switch
		{
			CellState.Empty => -1,
			CellState.Modifiable or CellState.Given => TrailingZeroCount(this[cell]),
			_ => throw new InvalidOperationException(SR.ExceptionMessage("GridInvalidCellState"))
		};

	/// <inheritdoc/>
	public readonly Digit[] ToDigitsArray()
	{
		var result = new Digit[81];
		for (var cell = 0; cell < 81; cell++)
		{
			if (GetState(cell) != CellState.Empty)
			{
				result[cell] = Log2((uint)GetCandidates(cell));
			}
		}
		return result;
	}

	/// <summary>
	/// Preserves the grid, removing cells not specified in <paramref name="template"/>.
	/// </summary>
	/// <param name="template">The cells specified to be preserved.</param>
	public readonly MarkerGrid Preserve(in CellMap template)
	{
		var temp = this;
		foreach (var cell in ~template)
		{
			temp.SetDigit(cell, -1);
		}
		return temp;
	}

	/// <inheritdoc/>
	public void Reset() => this = Preserve(GivenCells);

	/// <inheritdoc/>
	public void Fix()
	{
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
		mask = (Mask)((Mask)((int)state << 9) | (Mask)(mask & Grid.MaxCandidatesMask));
	}

	/// <inheritdoc/>
	public void SetDigit(Cell cell, Digit digit)
	{
		if (digit switch
		{
			-1 when GetState(cell) == CellState.Modifiable => Grid.DefaultMask,
			>= 0 and < 9 => (Mask)(Grid.ModifiableMask | 1 << digit),
			_ => Mask.MinValue
		} is var validMask and not Mask.MinValue)
		{
			this[cell] = validMask;
		}
	}

	/// <inheritdoc/>
	public void SetCandidates(Cell cell, Mask mask)
		=> this[cell] = (Mask)((Mask)((int)GetState(cell) << 9) | (Mask)(mask & Grid.MaxCandidatesMask));

	/// <inheritdoc/>
	public void SetExistence(Cell cell, Digit digit, bool isOn)
	{
		if ((cell, digit) is ( >= 0 and < 81, >= 0 and < 9))
		{
			if (isOn)
			{
				AddCandidates(cell, digit);
			}
			else
			{
				RemoveCandidates(cell, digit);
			}
		}
	}

	/// <summary>
	/// Adds a list of new candidates into the grid.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <param name="digits">The digits.</param>
	public void AddCandidates(Cell cell, params ReadOnlySpan<Digit> digits) => AddCandidates(cell, Mask.Create(digits));

	/// <summary>
	/// Adds a list of new candidates into the grid.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <param name="digitsMask">The mask of digits.</param>
	public void AddCandidates(Cell cell, Mask digitsMask)
	{
		if (GetState(cell) == CellState.Empty)
		{
			this[cell] |= digitsMask;
		}
	}

	/// <summary>
	/// Removes a list of candidates from the grid.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <param name="digits">The digits.</param>
	public void RemoveCandidates(Cell cell, params ReadOnlySpan<Digit> digits)
		=> RemoveCandidates(cell, Mask.Create(digits));

	/// <summary>
	/// Removes a list of candidates from the grid.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <param name="digitsMask">The mask of digits.</param>
	public void RemoveCandidates(Cell cell, Mask digitsMask)
	{
		if (GetState(cell) == CellState.Empty)
		{
			this[cell] &= (Mask)~digitsMask;
		}
	}

	/// <inheritdoc/>
	readonly bool IEquatable<MarkerGrid>.Equals(MarkerGrid other) => Equals(other);

	/// <inheritdoc/>
	readonly MarkerGrid IElementSwappingTransformable<MarkerGrid, Digit>.Shuffle()
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
	readonly IEnumerable<Candidate> IWhereMethod<MarkerGrid, Candidate>.Where(Func<Candidate, bool> predicate)
		=> this.Where(predicate).ToArray();

	/// <inheritdoc/>
	readonly IEnumerable<TResult> ISelectMethod<MarkerGrid, Candidate>.Select<TResult>(Func<Candidate, TResult> selector)
		=> this.Select(selector).ToArray();

	/// <inheritdoc/>
	void GridBase.SetMask(Cell cell, Mask mask) => this[cell] = mask;

	/// <inheritdoc/>
	MarkerGrid ITransformable<MarkerGrid>.RotateClockwise() => this.RotateClockwise();

	/// <inheritdoc/>
	MarkerGrid ITransformable<MarkerGrid>.MirrorLeftRight() => this.MirrorLeftRight();

	/// <inheritdoc/>
	MarkerGrid ITransformable<MarkerGrid>.MirrorTopBottom() => this.MirrorTopBottom();

	/// <inheritdoc/>
	MarkerGrid ITransformable<MarkerGrid>.MirrorDiagonal() => this.MirrorDiagonal();

	/// <inheritdoc/>
	MarkerGrid ITransformable<MarkerGrid>.MirrorAntidiagonal() => this.MirrorAntidiagonal();

	/// <inheritdoc/>
	MarkerGrid IElementSwappingTransformable<MarkerGrid, Digit>.SwapElement(Digit element1, Digit element2)
		=> this.SwapDigit(element1, element2);


	/// <inheritdoc/>
	public static bool TryParse([NotNullWhen(true)] string? s, out MarkerGrid result) => TryParse(s.AsSpan(), out result);

	/// <inheritdoc/>
	public static bool TryParse(ReadOnlySpan<char> s, out MarkerGrid result)
		=> new SusserGridDefaultConverter().TryParse(s, null, out result);

	/// <inheritdoc/>
	public static MarkerGrid Create(params ReadOnlySpan<Mask> values)
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

	/// <summary>
	/// Creates a <see cref="MarkerGrid"/> instance using grid values.
	/// </summary>
	/// <param name="gridValues">The array of grid values.</param>
	/// <param name="creatingOption">The grid creating option.</param>
	public static MarkerGrid Create(Digit[] gridValues, GridCreatingOption creatingOption = 0)
		=> new(in gridValues[0], creatingOption);

	/// <summary>
	/// Creates a <see cref="MarkerGrid"/> instance via the array of cell digits
	/// of type <see cref="ReadOnlySpan{T}"/> of <see cref="Digit"/>.
	/// </summary>
	/// <param name="gridValues">The list of cell digits.</param>
	/// <param name="creatingOption">The grid creating option.</param>
	public static MarkerGrid Create(ReadOnlySpan<Digit> gridValues, GridCreatingOption creatingOption = 0)
		=> new(in gridValues[0], creatingOption);

	/// <inheritdoc/>
	public static MarkerGrid Parse(string? s) => Parse(s.AsSpan());

	/// <inheritdoc/>
	public static MarkerGrid Parse(ReadOnlySpan<char> s)
		=> new SusserGridDefaultConverter().TryParse<MarkerGrid>(s, null, out var result) ? result : throw new FormatException();


	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)"/>
	public static bool operator >(in MarkerGrid left, in MarkerGrid right) => left.CompareTo(right) > 0;

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)"/>
	public static bool operator <(in MarkerGrid left, in MarkerGrid right) => left.CompareTo(right) < 0;

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)"/>
	public static bool operator >=(in MarkerGrid left, in MarkerGrid right) => left.CompareTo(right) >= 0;

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)"/>
	public static bool operator <=(in MarkerGrid left, in MarkerGrid right) => left.CompareTo(right) <= 0;

	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)"/>
	public static bool operator ==(in MarkerGrid left, in MarkerGrid right) => left.Equals(right);

	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)"/>
	public static bool operator !=(in MarkerGrid left, in MarkerGrid right) => !(left == right);

	/// <inheritdoc/>
	static bool IEqualityOperators<MarkerGrid, MarkerGrid, bool>.operator ==(MarkerGrid left, MarkerGrid right) => left == right;

	/// <inheritdoc/>
	static bool IEqualityOperators<MarkerGrid, MarkerGrid, bool>.operator !=(MarkerGrid left, MarkerGrid right) => left != right;

	/// <inheritdoc/>
	static bool IComparisonOperators<MarkerGrid, MarkerGrid, bool>.operator >(MarkerGrid left, MarkerGrid right) => left > right;

	/// <inheritdoc/>
	static bool IComparisonOperators<MarkerGrid, MarkerGrid, bool>.operator <(MarkerGrid left, MarkerGrid right) => left < right;

	/// <inheritdoc/>
	static bool IComparisonOperators<MarkerGrid, MarkerGrid, bool>.operator >=(MarkerGrid left, MarkerGrid right) => left >= right;

	/// <inheritdoc/>
	static bool IComparisonOperators<MarkerGrid, MarkerGrid, bool>.operator <=(MarkerGrid left, MarkerGrid right) => left <= right;


	/// <summary>
	/// Casts the current instance into a <see cref="Grid"/>, only with given and modifiable cells reserved.
	/// </summary>
	/// <param name="grid">The grid to be cast from.</param>
	public static explicit operator Grid(in MarkerGrid grid) => Grid.Create(grid.ToDigitsArray());

	/// <summary>
	/// Casts the current instance into a <see cref="MarkerGrid"/>, only with given and modifiable cells reserved.
	/// </summary>
	/// <param name="grid">The grid to be cast from.</param>
	public static explicit operator MarkerGrid(in Grid grid) => Create(grid.ToDigitsArray());
}
