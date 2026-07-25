namespace Sudoku.Concepts.Graphs;

/// <summary>
/// <para>Represents a cluster. A cluster is a group of candidates which are all connected with strong links.</para>
/// <para>
/// This data structure will simplify the definition, only reserving for single-digit strong links (i.e. conjugate pairs),
/// in order to make the implementation behave well and easily on representing data.
/// </para>
/// <para>
/// Please visit <see href="http://sudopedia.enjoysudoku.com/Cluster.html">this link</see> to learn more information.
/// </para>
/// </summary>
/// <param name="grid"><inheritdoc cref="_grid" path="/summary"/></param>
/// <param name="digit"><inheritdoc cref="Digit" path="/summary"/></param>
/// <param name="map"><inheritdoc cref="Map" path="/summary"/></param>
/// <seealso href="http://sudopedia.enjoysudoku.com/Cluster.html">Sudopedia Mirror - Cluster</seealso>
public readonly ref struct Cluster(in Grid grid, Digit digit, scoped in CellMap map) : IEquatable<Cluster>
{
	/// <summary>
	/// Indicates the grid used.
	/// </summary>
	private readonly ref readonly Grid _grid = ref grid;

	/// <summary>
	/// Indicates the backing cells map used.
	/// </summary>
	private readonly CellMap _map = map;


	/// <summary>
	/// Indicates the digit used.
	/// </summary>
	public Digit Digit { get; } = digit;

	/// <summary>
	/// Indicates the internal map.
	/// </summary>
	[UnscopedRef]
	public ref readonly CellMap Map => ref _map;

	/// <summary>
	/// Represents a list of cells that will form wrap contradictions in the cluster.
	/// </summary>
	public CellMap WrapContradictions
	{
		get
		{
			var result = CellMap.Empty;
			var graph = CellGraph.CreateFromConjugatePair(_grid, Digit, Map);
			foreach (ref readonly var component in graph.Components)
			{
				var parities = Parity.Create(component);
				if (parities.Length == 0)
				{
					continue;
				}

				ref readonly var firstParityPair = ref parities[0];
				var parity1 = firstParityPair.On.Cells;
				var parity2 = firstParityPair.Off.Cells;
			loop_i:
				for (var i = 0; i < 2; i++)
				{
					// Check whether there're two or more cells lying in a same house.
					ref readonly var parity = ref i == 0 ? ref parity1 : ref parity2;
					foreach (var house in parity.Houses)
					{
						if ((HousesMap[house] & parity).Count >= 2)
						{
							// All this parity is incorrect, and the other one is correct.
							result |= parity;
							break loop_i;
						}
					}
				}
			}
			return result;
		}
	}

	/// <summary>
	/// Represents a list of cells that will form trap contradictions in the cluster.
	/// </summary>
	public CellMap TrapContradictions
	{
		get
		{
			var candsMap = _grid.CandidatesMap[Digit];
			var result = CellMap.Empty;
			var graph = CellGraph.CreateFromConjugatePair(_grid, Digit, Map);
			foreach (ref readonly var component in graph.Components)
			{
				var parities = Parity.Create(component);
				if (parities.Length == 0)
				{
					continue;
				}

				ref readonly var firstParityPair = ref parities[0];
				var parity1 = firstParityPair.On.Cells;
				var parity2 = firstParityPair.Off.Cells;

				// Now we should iterate two collections to get contradiction.
				var conflictCells = CellMap.Empty;
				var conflictPair = new HashSet<((Cell Left, Cell Right), CellMap InfluencedRange)>();
				foreach (var cell1 in parity1)
				{
					foreach (var cell2 in parity2)
					{
						var intersection = (cell1.AsCellMap() + cell2).PeerIntersection;
						var currentConflictCells = intersection & candsMap;
						if (!!currentConflictCells
							&& !conflictPair.Any(p => (p.InfluencedRange & currentConflictCells) == currentConflictCells))
						{
							conflictPair.AddRef(((cell1, cell2), currentConflictCells));
							conflictCells |= currentConflictCells;
						}
					}
				}
				result |= conflictCells;
			}
			return result;
		}
	}


	/// <inheritdoc cref="ReadOnlySpan{T}.Equals"/>
	public override bool Equals(object? obj) => false;

	/// <inheritdoc/>
	public bool Equals(Cluster other) => _map == other._map;

	/// <inheritdoc/>
	public override int GetHashCode() => _map.GetHashCode();

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString()
		=> $$"""{{nameof(Cluster)}} { Grid = {{_grid}}, Map = {{_map}}, {{nameof(Digit)}} = {{Digit + 1}} }""";


	/// <summary>
	/// Creates a <see cref="Cluster"/> instance via the specified grid.
	/// </summary>
	/// <param name="grid">The grid to be used.</param>
	/// <param name="digit">Indicates the digits used.</param>
	/// <returns>A <see cref="Cluster"/> instance.</returns>
	public static Cluster Create(in Grid grid, Digit digit)
	{
		var result = CellMap.Empty;
		foreach (var cp in grid.ConjugatePairs)
		{
			if (cp.Digit == digit)
			{
				result |= cp.Map;
			}
		}
		return new(grid, digit, result);
	}


	/// <inheritdoc/>
	public static bool operator ==(Cluster left, Cluster right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(Cluster left, Cluster right) => !(left == right);
}
