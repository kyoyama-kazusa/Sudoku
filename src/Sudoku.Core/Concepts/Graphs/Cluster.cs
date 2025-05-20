namespace Sudoku.Concepts.Graphs;

using ConflictedInfo = ((Cell Left, Cell Right), CellMap InfluencedRange);

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
[TypeImpl(
	TypeImplFlags.AllObjectMethods | TypeImplFlags.EqualityOperators | TypeImplFlags.Equatable,
	ToStringBehavior = ToStringBehavior.RecordLike)]
public readonly ref partial struct Cluster(in Grid grid, Digit digit, scoped in CellMap map) : IEquatable<Cluster>
{
	/// <summary>
	/// Indicates the grid used.
	/// </summary>
	[HashCodeMember]
	[StringMember]
	private readonly ref readonly Grid _grid = ref grid;

	/// <summary>
	/// Indicates the backing cells map used.
	/// </summary>
	[HashCodeMember]
	[StringMember]
	[EquatableMember]
	[SuppressMessage("Style", "IDE0032:Use auto property", Justification = "<Pending>")]
	private readonly CellMap _map = map;


	/// <summary>
	/// Indicates the digit used.
	/// </summary>
	[HashCodeMember]
	[StringMember]
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
							goto NextComponent;
						}
					}
				}

			NextComponent:
				;
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
				var (conflictedCells, conflictedPair) = (CellMap.Empty, new HashSet<ConflictedInfo>());
				foreach (var cell1 in parity1)
				{
					foreach (var cell2 in parity2)
					{
						var intersection = (cell1.AsCellMap() + cell2).PeerIntersection;
						var currentConflictCells = intersection & candsMap;
						if (!!currentConflictCells
							&& !conflictedPair.Any(p => (p.InfluencedRange & currentConflictCells) == currentConflictCells))
						{
							conflictedPair.Add(((cell1, cell2), currentConflictCells));
							conflictedCells |= currentConflictCells;
						}
					}
				}
				result |= conflictedCells;
			}
			return result;
		}
	}


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
}
