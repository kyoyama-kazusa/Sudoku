namespace Sudoku.Concepts.Graphs;

/// <summary>
/// Represents the concept "<see href="https://en.wikipedia.org/wiki/Hamiltonian_path">Hamiltonian Path</see>"
/// that will be applied to a <see cref="CellGraph"/> instance.
/// </summary>
/// <param name="cells"><inheritdoc cref="_cells" path="/summary"/></param>
/// <seealso cref="CellGraph"/>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.EqualityOperators)]
public readonly partial struct HamiltonianCycle(Cell[] cells) :
	IEnumerable<Cell>,
	IEquatable<HamiltonianCycle>,
	IEqualityOperators<HamiltonianCycle, HamiltonianCycle, bool>,
	IReadOnlyList<Cell>
{
	/// <summary>
	/// Indicates the cells.
	/// </summary>
	private readonly int[] _cells = cells;


	/// <summary>
	/// Indicates the number of cells used.
	/// </summary>
	public int Length => _cells.Length;

	/// <summary>
	/// Indicates the sequence of cells in order.
	/// </summary>
	public ReadOnlySpan<Cell> Cells => _cells;

	/// <inheritdoc/>
	int IReadOnlyCollection<Cell>.Count => Length;


	/// <summary>
	/// Returns the cell at the specified index.
	/// </summary>
	/// <param name="index">The desired index.</param>
	/// <returns>The cell at the specified index.</returns>
	public Cell this[int index] => _cells[index];


	/// <inheritdoc/>
	public bool Equals(HamiltonianCycle other) => Equals(other, HamiltonianCycleComparison.Default);

	/// <summary>
	/// Determine whether two <see cref="HamiltonianCycle"/> instances are considered as equal under the specified comparison rule.
	/// </summary>
	/// <param name="other">Indicates the other object to be compared.</param>
	/// <param name="comparison">The comparison rule to be used.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the argument <paramref name="comparison"/> is not defined.
	/// </exception>
	public bool Equals(HamiltonianCycle other, HamiltonianCycleComparison comparison)
	{
		if (!Enum.IsDefined(comparison))
		{
			throw new ArgumentOutOfRangeException(nameof(comparison));
		}
		if (_cells.Length != other.Length)
		{
			return false;
		}
		if (_cells.Length == 0)
		{
			return true;
		}

		// Find for the same start.
		var start = _cells[0];
		var otherStartIndex = Array.IndexOf(other._cells, start);
		if (otherStartIndex == -1)
		{
			return false;
		}

		// Check for the next position; if the collection only contains one element, it cannot be a loop.
		// Just throw IndexOutOfRangeException, ignoring such invalid case.
		var nextCell = _cells[1];
		var previous = other._cells[(otherStartIndex >= 1 ? otherStartIndex : otherStartIndex + _cells.Length) - 1];
		var next = other._cells[(otherStartIndex + 1) % _cells.Length];
		if (nextCell != previous && nextCell != next)
		{
			return false;
		}

		for (
			var (i, j) = (otherStartIndex, 0);
			j < _cells.Length;
			i = nextCell == previous ? (i >= 1 ? i : i + _cells.Length) - 1 : (i + 1) % _cells.Length, j++
		)
		{
			if (_cells[j] != other._cells[i])
			{
				return false;
			}
		}
		return true;
	}

	/// <inheritdoc/>
	public override int GetHashCode() => GetHashCode(HamiltonianCycleComparison.Default);

	/// <summary>
	/// Returns the hash code of the current instance, using the specified comparison rule.
	/// </summary>
	/// <param name="comparison">The comparison rule.</param>
	/// <returns>An <see cref="int"/> value indicating the result.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the argument <paramref name="comparison"/> is not defined.
	/// </exception>
	public int GetHashCode(HamiltonianCycleComparison comparison)
	{
		var hashCode = new HashCode();
		var indexedCell = _cells.Index();
		var min = indexedCell.MinBy(static pair => pair.Value);
		switch (comparison)
		{
			case HamiltonianCycleComparison.IgnoreDirection:
			{
				var previous = (min.Index >= 1 ? min.Index : min.Index + _cells.Length) - 1;
				var next = (min.Index + 1) % _cells.Length;
				for (
					var (i, j) = (min.Index, 0);
					j < _cells.Length;
					i = _cells[previous] <= _cells[next] ? (i >= 1 ? i : i + _cells.Length) - 1 : (i + 1) % _cells.Length, j++
				)
				{
					hashCode.Add(_cells[i]);
				}
				return hashCode.ToHashCode();
			}
			case HamiltonianCycleComparison.Default:
			{
				for (var (i, j) = (min.Index, 0); j < _cells.Length; i = (i + 1) % _cells.Length, j++)
				{
					hashCode.Add(_cells[i]);
				}
				return hashCode.ToHashCode();
			}
			default:
			{
				throw new ArgumentOutOfRangeException(nameof(comparison));
			}
		}
	}

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString() => ToString(null);

	/// <summary>
	/// Returns a string that represents the current object, using the specified coordinate converter object to format cells.
	/// </summary>
	/// <param name="converter">
	/// Indicates the converter. If <see langword="null"/> is assigned, RxCy notation will be adopted.
	/// </param>
	/// <returns>A <see cref="string"/> that represents the current object.</returns>
	public string ToString(CoordinateConverter? converter)
	{
		converter ??= new RxCyConverter();

		const string separator = " -> ";
		var sb = new StringBuilder();
		foreach (var cell in _cells)
		{
			sb.Append(converter.CellConverter(cell.AsCellMap()));
			sb.Append(separator);
		}
		return sb.RemoveFrom(^separator.Length).ToString();
	}

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public AnonymousSpanEnumerator<Cell> GetEnumerator() => new(_cells);

	/// <summary>
	/// Enumerate all adjacent cell pairs, including the last and the first cell.
	/// </summary>
	/// <returns>An enumerator object that can iterate on each adjacent cell pairs, including the last and the first cell.</returns>
	public AnonymousSpanEnumerator<(Cell First, Cell Second)> EnumerateAdjacentCells()
	{
		var span = new (Cell, Cell)[Length];
		for (var i = 0; i < Length - 1; i++)
		{
			span[i] = (_cells[i], _cells[i + 1]);
		}
		span[^1] = (_cells[^1], _cells[0]);
		return new(span);
	}

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => _cells.GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<Cell> IEnumerable<Cell>.GetEnumerator() => _cells.AsEnumerable().GetEnumerator();
}
