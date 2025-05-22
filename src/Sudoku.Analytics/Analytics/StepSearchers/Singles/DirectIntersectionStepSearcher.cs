namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with a <b>Direct Intersection</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>
/// Direct Pointing family:
/// <list type="bullet">
/// <item>Pointing Full House</item>
/// <item>Pointing Crosshatching in Block</item>
/// <item>Pointing Crosshatching in Row</item>
/// <item>Pointing Crosshatching in Column</item>
/// <item>Pointing Naked Single</item>
/// </list>
/// </item>
/// <item>
/// Direct Claiming family:
/// <list type="bullet">
/// <item>Claiming Full House</item>
/// <item>Claiming Crosshatching in Block</item>
/// <item>Claiming Crosshatching in Row</item>
/// <item>Claiming Crosshatching in Column</item>
/// <item>Claiming Naked Single</item>
/// </list>
/// </item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_DirectIntersectionStepSearcher",
	Technique.ComplexFullHouse, Technique.ComplexCrosshatchingBlock, Technique.ComplexCrosshatchingRow,
	Technique.ComplexCrosshatchingColumn, Technique.ComplexNakedSingle,
	IsCachingSafe = true,
	IsAvailabilityReadOnly = true,
	IsOrderingFixed = true,
	RuntimeFlags = StepSearcherRuntimeFlags.DirectTechniquesOnly | StepSearcherRuntimeFlags.SkipVerification)]
public sealed partial class DirectIntersectionStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates whether the step searcher allows for searching for direct pointing.
	/// </summary>
	[SettingItemName(SettingItemNames.AllowDirectPointing)]
	public bool AllowDirectPointing { get; set; }

	/// <summary>
	/// Indicates whether the step searcher allows for searching for direct claiming.
	/// </summary>
	[SettingItemName(SettingItemNames.AllowDirectClaiming)]
	public bool AllowDirectClaiming { get; set; }


	/// <inheritdoc/>
	/// <remarks>
	/// <include file="../../global-doc-comments.xml" path="/g/developer-notes" />
	/// Please check documentation comments for <see cref="LockedCandidatesStepSearcher.Collect(ref StepAnalysisContext)"/>
	/// to learn more information about this technique.
	/// </remarks>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		ref readonly var grid = ref context.Grid;
		var emptyCells = grid.EmptyCells;
		var candidatesMap = grid.CandidatesMap;
		foreach (var ((bs, cs), (a, b, c, _)) in Miniline.Map)
		{
			if (!LockedCandidates.IsLockedCandidates(grid, a, b, c, emptyCells, out var m))
			{
				continue;
			}

			foreach (var digit in m)
			{
				ref readonly var map = ref candidatesMap[digit];

				// Check whether the digit contains any eliminations.
				var (housesMask, elimMap) = a & map ? ((Mask)(cs << 8 | bs), a & map) : ((Mask)(bs << 8 | cs), b & map);
				if (!elimMap)
				{
					continue;
				}

				var (realBaseSet, realCoverSet, intersection) = (housesMask >> 8 & 127, housesMask & 127, c & map);
				if (intersection.Count < 2)
				{
					continue;
				}

				if (!AllowDirectPointing && realBaseSet < 9 || !AllowDirectClaiming && realBaseSet >= 9)
				{
					continue;
				}

				// Different with normal locked candidates searcher, this searcher is used as direct views.
				// We should check any possible assignments after such eliminations applied -
				// such assignments are the real conclusions of this technique.
				if (CheckFullHouse(
					ref context, grid, realBaseSet, realCoverSet, intersection, emptyCells, elimMap, digit) is { } fullHouse)
				{
					return fullHouse;
				}
				if (CheckHiddenSingle(
					ref context, grid, realBaseSet, realCoverSet, intersection, elimMap, candidatesMap, emptyCells, digit) is { } hiddenSingle)
				{
					return hiddenSingle;
				}
				if (CheckNakedSingle(
					ref context, grid, realBaseSet, realCoverSet, intersection, elimMap, emptyCells, digit) is { } nakedSingle)
				{
					return nakedSingle;
				}
			}
		}

		return null;
	}

	/// <summary>
	/// Check full house.
	/// </summary>
	/// <param name="context">The context.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="baseSet">The base set.</param>
	/// <param name="coverSet">The cover set.</param>
	/// <param name="intersection">The intersection cells.</param>
	/// <param name="emptyCells">Indicates the empty cells of grid.</param>
	/// <param name="elimMap">The eliminated cells of digit <paramref name="digit"/>.</param>
	/// <param name="digit">The digit that can be eliminated in locked candidates.</param>
	/// <returns>The result step found.</returns>
	private DirectIntersectionStep? CheckFullHouse(
		ref StepAnalysisContext context,
		in Grid grid,
		House baseSet,
		House coverSet,
		in CellMap intersection,
		in CellMap emptyCells,
		in CellMap elimMap,
		Digit digit
	)
	{
		foreach (var house in elimMap.Houses)
		{
			var emptyCellsInHouse = HousesMap[house] & emptyCells;
			if (emptyCellsInHouse.Count != 2)
			{
				continue;
			}

			var lastDigitMask = (Mask)(grid[emptyCellsInHouse] & ~(1 << digit));
			if (!BitOperations.IsPow2(lastDigitMask))
			{
				continue;
			}

			var lastDigit = BitOperations.Log2(lastDigitMask);
			var lastCell = (emptyCellsInHouse & elimMap)[0];
			var step = new DirectIntersectionStep(
				new SingletonArray<Conclusion>(new(Assignment, lastCell, lastDigit)),
				[
					[
						.. from cell in intersection select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + digit),
						.. Excluder.GetLockedCandidatesExcluders(grid, digit, baseSet, intersection),
						new CandidateViewNode(ColorIdentifier.Elimination, lastCell * 9 + digit),
						new HouseViewNode(ColorIdentifier.Normal, baseSet),
						new HouseViewNode(ColorIdentifier.Auxiliary1, coverSet),
						new HouseViewNode(ColorIdentifier.Auxiliary3, house)
					]
				],
				context.Options,
				lastCell,
				lastDigit,
				HousesMap[baseSet] & HousesMap[coverSet] & emptyCells,
				baseSet,
				lastCell.AsCellMap(),
				digit,
				house switch
				{
					< 9 => SingleSubtype.FullHouseBlock,
					< 18 => SingleSubtype.FullHouseRow,
					_ => SingleSubtype.FullHouseColumn
				},
				Technique.FullHouse,
				baseSet < 9
			);
			if (context.OnlyFindOne)
			{
				return step;
			}

			context.Accumulator.Add(step);
		}

		return null;
	}

	/// <summary>
	/// Check hidden single.
	/// </summary>
	/// <param name="context">The context.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="baseSet">The base set.</param>
	/// <param name="coverSet">The cover set.</param>
	/// <param name="intersection">The intersection cells.</param>
	/// <param name="elimMap">The eliminated cells of digit <paramref name="digit"/>.</param>
	/// <param name="candidatesMap">Indicates the candidates map.</param>
	/// <param name="emptyCells">Indicates the empty cells.</param>
	/// <param name="digit">The digit that can be eliminated in locked candidates.</param>
	/// <returns>The result step found.</returns>
	private DirectIntersectionStep? CheckHiddenSingle(
		ref StepAnalysisContext context,
		in Grid grid,
		House baseSet,
		House coverSet,
		in CellMap intersection,
		in CellMap elimMap,
		ReadOnlySpan<CellMap> candidatesMap,
		in CellMap emptyCells,
		Digit digit
	)
	{
		foreach (var house in elimMap.Houses)
		{
			var emptyCellsInHouse = HousesMap[house] & candidatesMap[digit] & ~elimMap;
			if (emptyCellsInHouse is not [var lastCell])
			{
				continue;
			}

			var step = new DirectIntersectionStep(
				new SingletonArray<Conclusion>(new(Assignment, lastCell, digit)),
				[
					[
						.. Excluder.GetHiddenSingleExcluders(grid, digit, house, lastCell, out var chosenCells, out _),
						.. from cell in intersection select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + digit),
						.. Excluder.GetLockedCandidatesExcluders(grid, digit, baseSet, intersection),
						..
						from cell in HousesMap[house] & elimMap
						select new CandidateViewNode(ColorIdentifier.Elimination, cell * 9 + digit),
						new DiamondViewNode(ColorIdentifier.Auxiliary3, lastCell),
						new CandidateViewNode(ColorIdentifier.Elimination, lastCell * 9 + digit),
						new HouseViewNode(ColorIdentifier.Normal, baseSet),
						new HouseViewNode(ColorIdentifier.Auxiliary1, coverSet),
						new HouseViewNode(ColorIdentifier.Auxiliary3, house)
					]
				],
				context.Options,
				lastCell,
				digit,
				HousesMap[baseSet] & HousesMap[coverSet] & emptyCells,
				baseSet,
				(HousesMap[house] & candidatesMap[digit]) - lastCell,
				digit,
				TechniqueNaming.Single.GetHiddenSingleSubtype(grid, lastCell, house, chosenCells),
				house switch
				{
					< 9 => Technique.CrosshatchingBlock,
					< 18 => Technique.CrosshatchingRow,
					_ => Technique.CrosshatchingColumn
				},
				baseSet < 9
			);
			if (context.OnlyFindOne)
			{
				return step;
			}

			context.Accumulator.Add(step);
		}

		return null;
	}

	/// <summary>
	/// Check naked single.
	/// </summary>
	/// <param name="context">The context.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="baseSet">The base set.</param>
	/// <param name="coverSet">The cover set.</param>
	/// <param name="intersection">The intersection cells.</param>
	/// <param name="elimMap">The eliminated cells of digit <paramref name="digit"/>.</param>
	/// <param name="digit">The digit that can be eliminated in locked candidates.</param>
	/// <param name="emptyCells">Indicates the empty cells.</param>
	/// <returns>The result step found.</returns>
	private DirectIntersectionStep? CheckNakedSingle(
		ref StepAnalysisContext context,
		in Grid grid,
		House baseSet,
		House coverSet,
		in CellMap intersection,
		in CellMap elimMap,
		in CellMap emptyCells,
		Digit digit
	)
	{
		foreach (var lastCell in elimMap)
		{
			if (grid.GetCandidates(lastCell) is var digitsMask && BitOperations.PopCount(digitsMask) != 2)
			{
				continue;
			}

			var lastDigit = BitOperations.TrailingZeroCount((Mask)(digitsMask & ~(1 << digit)));
			var step = new DirectIntersectionStep(
				new SingletonArray<Conclusion>(new(Assignment, lastCell, lastDigit)),
				[
					[
						.. from cell in intersection select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + digit),
						.. Excluder.GetNakedSingleExcluders(grid, lastCell, lastDigit, out _),
						.. Excluder.GetLockedCandidatesExcluders(grid, digit, baseSet, intersection),
						new DiamondViewNode(ColorIdentifier.Auxiliary3, lastCell),
						new CandidateViewNode(ColorIdentifier.Elimination, lastCell * 9 + digit),
						new HouseViewNode(ColorIdentifier.Normal, baseSet),
						new HouseViewNode(ColorIdentifier.Auxiliary1, coverSet)
					]
				],
				context.Options,
				lastCell,
				lastDigit,
				HousesMap[baseSet] & HousesMap[coverSet] & emptyCells,
				baseSet,
				lastCell.AsCellMap(),
				digit,
				TechniqueNaming.Single.GetNakedSingleSubtype(grid, lastCell),
				Technique.NakedSingle,
				baseSet < 9
			);
			if (context.OnlyFindOne)
			{
				return step;
			}

			context.Accumulator.Add(step);
		}

		return null;
	}
}
