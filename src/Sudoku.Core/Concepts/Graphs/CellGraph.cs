namespace Sudoku.Concepts.Graphs;

/// <summary>
/// <para>Represents a list of cells that form a undirected graph.</para>
/// <para>
/// Please visit <see href="https://en.wikipedia.org/wiki/Component_(graph_theory)">this link</see>
/// to learn more information about concept Component.
/// </para>
/// </summary>
/// <seealso href="https://en.wikipedia.org/wiki/Component_(graph_theory)">Wikipedia - Component (Graph Theory)</seealso>
[CollectionBuilder(typeof(CellGraph), nameof(Create))]
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.Object_GetHashCode | TypeImplFlags.Equatable | TypeImplFlags.EqualityOperators)]
public readonly partial struct CellGraph : IEquatable<CellGraph>, IFormattable, IReadOnlyCollection<Cell>
{
	/// <summary>
	/// Indicates the default empty graph without any cells.
	/// </summary>
	public static readonly CellGraph Empty = new();


	/// <summary>
	/// Indicates the cells used.
	/// </summary>
	[EquatableMember]
	[HashCodeMember]
	private readonly CellMap _cells;

	/// <summary>
	/// Indicates invalid cells. Such cells may not be covered from the grid.
	/// <see cref="GetComponentOf(Cell, out ReadOnlySpan{CellGraphDepth})"/> will ignore them.
	/// </summary>
	/// <seealso cref="GetComponentOf(Cell, out ReadOnlySpan{CellGraphDepth})"/>
	private readonly CellMap _invalidCells;

	/// <summary>
	/// Indicates the directly-connected cells.
	/// </summary>
	private readonly Dictionary<Cell, CellMap> _directlyConnectedCellsDictionary = [];


	/// <summary>
	/// Initializes a <see cref="CellGraph"/> instance with the specified cells.
	/// </summary>
	/// <param name="cells">The cells.</param>
	public CellGraph(in CellMap cells) : this(cells, CellMap.Empty)
	{
	}

	/// <summary>
	/// Initializes a <see cref="CellGraph"/> instance with the specified cells,
	/// and some invalid cells that will be used by conflict checking.
	/// </summary>
	/// <param name="cells">Indicates the cells used.</param>
	/// <param name="invalidCells">
	/// <para>Indicates invalid cells. Such cells will interrupt the conjugate pair connection from a cell to a cell.</para>
	/// <para>
	/// In general, this argument is an empty collection of <see cref="CellMap"/> (i.e. a <see cref="CellMap.Empty"/> or <c>[]</c>),
	/// but sometimes it can be useful for calculation of conflict on conjugate pair parity checks.
	/// </para>
	/// </param>
	public CellGraph(in CellMap cells, in CellMap invalidCells)
	{
		_cells = cells;
		_invalidCells = invalidCells;
		_directlyConnectedCellsDictionary = new(cells.Count);
		foreach (var cell in cells)
		{
			_directlyConnectedCellsDictionary.Add(cell, cells & PeersMap[cell]);
		}
	}


	/// <summary>
	/// Indicates whether the graph is connected, i.e. all nodes in the graph is a part of the whole graph without being separated.
	/// </summary>
	public bool IsConnected => Components is [var onlyComponent] && _cells == onlyComponent._cells;

	/// <summary>
	/// Indicates whether the current graph is an empty graph, i.e. there's no cells inside the current graph.
	/// </summary>
	public bool IsEmpty => _cells.Count == 0;

	/// <summary>
	/// Indicates whether the graph is bipartite graph.
	/// </summary>
	/// <remarks>
	/// <para>
	/// In general, a bipartite graph is a graph that can color with only 2 colors in a cyclic path,
	/// through alternating coloring rule. If the graph is of an odd length, it may not be a bipartite graph
	/// because we can find at least one pair of adjacent cells in a same color.
	/// </para>
	/// <para>
	/// For more information about "Bipartite Graph", please visit
	/// <see href="https://en.wikipedia.org/wiki/Bipartite_graph">this link</see>.
	/// </para>
	/// </remarks>
	public bool IsBipartite
	{
		get
		{
			// A bipartite graph in this pattern must include at least one hamiltonian cycle.
			if (GetHamiltonianCycles() is not [var loop, ..])
			{
				return false;
			}

			// Verify 2-coloring.
			var coloringDictionary = new Dictionary<bool, CellMap>();
			var isFirst = false;
			foreach (var cell in loop)
			{
				if (!coloringDictionary.TryAdd(isFirst, cell.AsCellMap()))
				{
					coloringDictionary.GetValueRef(isFirst).Add(cell);
				}
				isFirst = !isFirst;
			}

			// Determine whether at least one cell contains same color with its peers.
			foreach (var @switch in (false, true))
			{
				foreach (var cell in coloringDictionary[@switch])
				{
					foreach (var peer in PeersMap[cell] & _cells)
					{
						if (coloringDictionary[@switch].Contains(peer))
						{
							return false;
						}
					}
				}
			}
			return true;
		}
	}

	/// <summary>
	/// Indicates the vertices count, i.e. the number of cells in the graph.
	/// </summary>
	public int VerticesCount => _cells.Count;

	/// <summary>
	/// Indicates the internal map.
	/// </summary>
	[UnscopedRef]
	public ref readonly CellMap Map => ref _cells;

	/// <summary>
	/// Indicates all possible connected components of the graph.
	/// </summary>
	public ReadOnlySpan<CellGraph> Components
	{
		get
		{
			var lastCells = _cells;
			var result = new List<CellGraph>();
			while (lastCells)
			{
				foreach (var cell in lastCells.Offsets)
				{
					var queue = new Queue<Cell>();
					queue.Enqueue(cell);

					var currentGraph = cell.AsCellMap();
					while (queue.Count != 0)
					{
						var currentCell = queue.Dequeue();
						var connectedCells = _directlyConnectedCellsDictionary[currentCell];
						foreach (var peerCell in connectedCells)
						{
							if (!currentGraph.Contains(peerCell))
							{
								queue.Enqueue(peerCell);
							}
						}
						currentGraph |= connectedCells;
					}

					lastCells &= ~currentGraph;
					result.Add(new(currentGraph, _invalidCells));
					break;
				}
			}
			return result.AsSpan();
		}
	}

	/// <inheritdoc/>
	int IReadOnlyCollection<Cell>.Count => VerticesCount;


	/// <summary>
	/// Try to get all cells whose degree is the specified value.
	/// </summary>
	/// <param name="degree">The degree. The value must be between 0 and 3.</param>
	/// <returns>All cells whose degree is equal to the specified value.</returns>
	public CellGraph this[[ConstantExpected(Min = 0, Max = 3)] int degree]
	{
		get
		{
			var result = CellMap.Empty;
			foreach (var cell in _cells)
			{
				if (GetDegreeOf(cell) == degree)
				{
					result.Add(cell);
				}
			}
			return new(result, _invalidCells);
		}
	}


	/// <summary>
	/// Try to get the degree of the specified cell.
	/// </summary>
	/// <param name="cell">The desired cell.</param>
	/// <returns>An <see cref="int"/> indicating that. If the cell isn't in the current graph, -1 will be returned.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetDegreeOf(Cell cell) => _cells.Contains(cell) ? _directlyConnectedCellsDictionary[cell].Count : -1;

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public string ToString(IFormatProvider? formatProvider)
	{
		var converter = CoordinateConverter.GetInstance(formatProvider);
		var sb = new StringBuilder();
		foreach (var kvp in _directlyConnectedCellsDictionary)
		{
			sb.AppendLine($"{converter.CellConverter(kvp.Key.AsCellMap())}: {converter.CellConverter(kvp.ValueRef)}");
		}
		return sb.ToString();
	}

	/// <summary>
	/// Try to find a path that connects with all cells of the graph, with each edge connecting with two cells in a same house.
	/// </summary>
	/// <returns>
	/// All found paths, with different connecting rules.
	/// For example, sequences <c>a -&gt; b -&gt; c -&gt; d -&gt; a</c> and <c>c -&gt; b -&gt; a -&gt; d -&gt; c</c> are same
	/// because they have a same connecting rules (using same houses to connect adjacent cells).
	/// </returns>
	public ReadOnlySpan<HamiltonianCycle> GetHamiltonianCycles()
	{
		if (_cells)
		{
			var paths = new HashSet<HamiltonianCycle>(
				EqualityComparer<HamiltonianCycle>.Create(
					static (left, right) => left.Equals(right, HamiltonianCycleComparison.IgnoreDirection),
					static obj => obj.GetHashCode(HamiltonianCycleComparison.IgnoreDirection)
				)
			);
			dfs(_cells[1..], _cells[0], [_cells[0]], paths);
			return paths.ToArray();
		}
		return [];


		static void dfs(CellMap lastCells, Cell current, List<Cell> path, HashSet<HamiltonianCycle> paths)
		{
			if (!lastCells)
			{
				paths.Add(new([.. path]));
				return;
			}

			foreach (var next in lastCells & PeersMap[current])
			{
				path.Add(next);
				dfs(lastCells - next, next, path, paths);
				path.Remove(next);
			}
		}
	}

	/// <summary>
	/// Try to get connected cells for the specified cell. 
	/// </summary>
	/// <param name="cell">Indicates the cell to be checked.</param>
	/// <returns>All cells that connect the current cell.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public CellMap GetConnectedCells(Cell cell) => _directlyConnectedCellsDictionary[cell];

	/// <summary>
	/// Creates an enumerator type that can iterate on each cell in the collection.
	/// </summary>
	/// <returns>An enumerator instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Enumerator GetEnumerator() => new(_cells.ToArrayUnsafe());

	/// <summary>
	/// Try to get a <see cref="CellGraph"/> that contains the specified cell.
	/// </summary>
	/// <param name="cell">The desired cell.</param>
	/// <param name="depths">A list of <see cref="CellGraphDepth"/> values.</param>
	/// <returns>A <see cref="CellGraph"/> instance.</returns>
	public CellGraph GetComponentOf(Cell cell, out ReadOnlySpan<CellGraphDepth> depths)
	{
		var startDepthInstance = new CellGraphDepth(0, cell);
		var queue = new Queue<CellGraphDepth>();
		var depthValues = new List<CellGraphDepth>();
		queue.Enqueue(startDepthInstance);
		depthValues.Add(startDepthInstance);

		var currentGraph = cell.AsCellMap();
		while (queue.Count != 0)
		{
			var (currentDepth, currentCell) = queue.Dequeue();
			var connectedCells = _directlyConnectedCellsDictionary[currentCell];
			var coveredCells = CellMap.Empty;
			foreach (var peerCell in connectedCells)
			{
				if (currentGraph.Contains(peerCell) || _invalidCells.Contains(peerCell))
				{
					continue;
				}

				var matchHouse = (currentCell.AsCellMap() + peerCell).FirstSharedHouse;
				if (HousesMap[matchHouse] & _invalidCells)
				{
					continue;
				}

				var depth = new CellGraphDepth(currentDepth + 1, peerCell);
				queue.Enqueue(depth);
				depthValues.Add(depth);
				coveredCells.Add(peerCell);
			}
			currentGraph |= coveredCells;
		}

		depths = depthValues.AsSpan();
		return new(currentGraph, _invalidCells);
	}

	/// <inheritdoc/>
	public bool Contains(Cell item) => _cells.Contains(item);

	/// <inheritdoc/>
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(formatProvider);

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => _cells.ToArrayUnsafe().GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<Cell> IEnumerable<Cell>.GetEnumerator() => _cells.ToArrayUnsafe().AsEnumerable().GetEnumerator();


	/// <summary>
	/// Creates an <see cref="CellGraph"/> instance via the specified cells.
	/// </summary>
	/// <param name="cells">The cells.</param>
	/// <returns>An <see cref="CellGraph"/> instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CellGraph Create(in CellMap cells) => new(cells, CellMap.Empty);

	/// <inheritdoc cref="Create(in CellMap)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CellGraph Create(scoped ReadOnlySpan<Cell> cells) => Create(cells.AsCellMap());

	/// <summary>
	/// Initializes an <see cref="CellGraph"/> instance via a list of cells, checking conjugate pairs.
	/// </summary>
	/// <param name="grid">The grid to be used.</param>
	/// <param name="digit">The digit to be used.</param>
	/// <param name="cells">The cells to be used.</param>
	/// <returns>An <see cref="CellGraph"/> instance.</returns>
	public static CellGraph CreateFromConjugatePair(in Grid grid, Digit digit, in CellMap cells)
	{
		var globalCells = grid.CandidatesMap[digit];
		var invalidCells = CellMap.Empty;
		foreach (var house in globalCells.Houses)
		{
			var tempCells = globalCells & HousesMap[house];
			if (tempCells != (cells & HousesMap[house]))
			{
				invalidCells |= tempCells & ~(cells & HousesMap[house]);
			}
		}
		return new(cells, invalidCells);
	}
}
