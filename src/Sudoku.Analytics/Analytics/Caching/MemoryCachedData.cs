namespace Sudoku.Analytics.Caching;

/// <summary>
/// Represents cached fields used by solving and analyzing for a sudoku puzzle, reducing repeated and redundant calculations.
/// </summary>
internal static class MemoryCachedData
{
	/// <summary>
	/// The backing field storing on strong links.
	/// </summary>
	public static readonly LinkDictionary StrongLinkDictionary = [];

	/// <summary>
	/// The backing field storing on weak links.
	/// </summary>
	public static readonly LinkDictionary WeakLinkDictionary = [];


	/// <summary>
	/// Backing field of <see cref="EmptyCells"/>.
	/// </summary>
	private static CellMap _cachedEmptyCells;

	/// <summary>
	/// Backing field of <see cref="BivalueCells"/>.
	/// </summary>
	private static CellMap _cachedBivalueCells;

	/// <summary>
	/// Backing field of <see cref="CandidatesMap"/>.
	/// </summary>
	private static CellMap[] _cachedCandidatesMap = null!;

	/// <summary>
	/// Backing field of <see cref="DigitsMap"/>.
	/// </summary>
	private static CellMap[] _cachedDigitsMap = null!;

	/// <summary>
	/// Backing field of <see cref="ValuesMap"/>.
	/// </summary>
	private static CellMap[] _cachedValuesMap = null!;

	/// <summary>
	/// Backing field of <see cref="Solution"/>.
	/// </summary>
	private static Grid _cachedSolution;


	/// <summary>
	/// The backing field storing on strong links, grouped by link type.
	/// </summary>
	public static LinkType StrongLinkTypesCollected { get; private set; }

	/// <summary>
	/// The backing field storing on weak links, grouped by link type.
	/// </summary>
	public static LinkType WeakLinkTypesCollected { get; private set; }

	/// <summary>
	/// Indicates the number of candidates appeared in the puzzle.
	/// </summary>
	public static Candidate CandidatesCount { get; private set; }

	/// <summary>
	/// Indicates the solution.
	/// </summary>
	/// <remarks><b>
	/// Please note that a puzzle may contain multiple solutions, leading to this field <see cref="Grid.Undefined"/>.
	/// You must check validity of this field before using this field.
	/// </b></remarks>
	/// <seealso cref="Grid.Undefined"/>
	public static ref readonly Grid Solution => ref _cachedSolution;

	/// <summary>
	/// <inheritdoc cref="Grid.EmptyCells"/>
	/// </summary>
	/// <remarks>
	/// This map <b>should</b> be used after <see cref="Initialize(in Grid, in Grid)"/> called,
	/// and you<b>'d better</b> not use this field on instances which are set <see langword="true"/>
	/// for property <see cref="StepSearcherAttribute.IsCachingSafe"/>.
	/// </remarks>
	/// <seealso cref="Initialize(in Grid, in Grid)"/>
	/// <seealso cref="StepSearcherAttribute.IsCachingSafe"/>
	public static ref readonly CellMap EmptyCells => ref _cachedEmptyCells;

	/// <summary>
	/// <inheritdoc cref="Grid.BivalueCells"/>
	/// </summary>
	/// <remarks>
	/// <inheritdoc cref="EmptyCells" path="/remarks"/>
	/// </remarks>
	public static ref readonly CellMap BivalueCells => ref _cachedBivalueCells;

	/// <summary>
	/// <inheritdoc cref="Grid.CandidatesMap"/>
	/// </summary>
	/// <remarks>
	/// <inheritdoc cref="EmptyCells" path="/remarks"/>
	/// </remarks>
	/// <exception cref="NullReferenceException">Throws when not initialized.</exception>
	public static ReadOnlySpan<CellMap> CandidatesMap => _cachedCandidatesMap;

	/// <summary>
	/// <inheritdoc cref="Grid.DigitsMap"/>
	/// </summary>
	/// <remarks>
	/// <inheritdoc cref="EmptyCells" path="/remarks"/>
	/// </remarks>
	/// <exception cref="NullReferenceException">Throws when not initialized.</exception>
	public static ReadOnlySpan<CellMap> DigitsMap => _cachedDigitsMap;

	/// <summary>
	/// <inheritdoc cref="Grid.ValuesMap"/>
	/// </summary>
	/// <remarks>
	/// <inheritdoc cref="EmptyCells" path="/remarks"/>
	/// </remarks>
	/// <exception cref="NullReferenceException">Throws when not initialized.</exception>
	public static ReadOnlySpan<CellMap> ValuesMap => _cachedValuesMap;


	/// <inheritdoc cref="Initialize(Analyzer, in Grid, in Grid)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Initialize(in Grid g, in Grid s) => Initialize(null, g, s);

	/// <summary>
	/// Initialize the maps that used later.
	/// </summary>
	/// <param name="analyzer">The analyzer.</param>
	/// <param name="g">The grid.</param>
	/// <param name="s">The solution of <paramref name="g"/>.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Initialize(Analyzer? analyzer, in Grid g, in Grid s)
	{
		(CandidatesCount, StrongLinkTypesCollected, WeakLinkTypesCollected) = (g.CandidatesCount, LinkType.Unknown, LinkType.Unknown);
		(_cachedEmptyCells, _cachedBivalueCells, _cachedSolution) = (g.EmptyCells, g.BivalueCells, s);
		(_cachedCandidatesMap, _cachedDigitsMap, _cachedValuesMap) = ([.. g.CandidatesMap], [.. g.DigitsMap], [.. g.ValuesMap]);

		if (analyzer is null || analyzer.ResultStepSearchers.Span.Any(match))
		{
			StrongLinkDictionary.Clear();
			WeakLinkDictionary.Clear();
		}


		static bool match(StepSearcher searcher)
			=> searcher is BivalueUniversalGraveForcingChainsStepSearcher
			or BlossomLoopStepSearcher
			or ChainStepSearcher
			or DynamicForcingChainsStepSearcher
			or FinnedChainStepSearcher
			or FinnedRectangleChainStepSearcher
			or GroupedChainStepSearcher
			or GroupedFinnedChainStepSearcher
			or MultipleForcingChainsStepSearcher
			or RectangleForcingChainsStepSearcher
			or RemotePairStepSearcher
			or WhipStepSearcher;
	}

	/// <summary>
	/// Try to collect strong and weak links appeared in a grid.
	/// If all links are "up-to-date" (i.e. meaning there's no extra links to be checked), this method will do nothing,
	/// in order to enhance performance.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="linkTypes">The link types to be checked.</param>
	/// <param name="options">The options used by step searchers.</param>
	/// <param name="rules">The <see cref="ChainingRule"/> instance that collects with strong and weak links if worth.</param>
	[InterceptorMethodCaller]
	[InterceptorPolymorphic(
		typeof(XChainingRule),
		typeof(YChainingRule),
		typeof(LockedCandidatesChainingRule),
		typeof(AlmostLockedSetsChainingRule),
		typeof(UniqueRectangleSameDigitChainingRule),
		typeof(UniqueRectangleDifferentDigitChainingRule),
		typeof(UniqueRectangleSingleSideExternalChainingRule),
		typeof(UniqueRectangleDoubleSideExternalChainingRule),
		typeof(AvoidableRectangleChainingRule),
		typeof(KrakenNormalFishChainingRule),
		typeof(XyzWingChainingRule))]
	public static void InitializeLinks(in Grid grid, LinkType linkTypes, StepGathererOptions options, out ChainingRuleCollection rules)
	{
		rules = from linkType in linkTypes select ChainingRulePool.TryCreate(linkType)!;
		if (!StrongLinkTypesCollected.HasFlag(linkTypes) || !WeakLinkTypesCollected.HasFlag(linkTypes))
		{
			var (strongDic, weakDic) = (new LinkDictionary(), new LinkDictionary());
			foreach (var rule in rules)
			{
				rule.GetLinks(grid, strongDic, weakDic, options);
			}

			if (strongDic.Count != 0)
			{
				StrongLinkTypesCollected |= linkTypes;
				StrongLinkDictionary.Merge(strongDic);
			}
			if (weakDic.Count != 0)
			{
				WeakLinkTypesCollected |= linkTypes;
				WeakLinkDictionary.Merge(weakDic);
			}
		}
	}
}
