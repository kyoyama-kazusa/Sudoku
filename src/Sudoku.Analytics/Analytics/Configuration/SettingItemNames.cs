namespace Sudoku.Analytics.Configuration;

/// <summary>
/// Represents a list of <see cref="string"/>s indicating the runtime identifier recognized by UI,
/// used by <see cref="SettingItemNameAttribute"/>.
/// </summary>
/// <seealso cref="SettingItemNameAttribute"/>
public static class SettingItemNames
{
	//
	// Step searcher property names
	//
	/// <inheritdoc cref="SingleStepSearcher.EnableFullHouse"/>
	public const string EnableFullHouse = nameof(EnableFullHouse);

	/// <inheritdoc cref="SingleStepSearcher.EnableLastDigit"/>
	public const string EnableLastDigit = nameof(EnableLastDigit);

	/// <inheritdoc cref="SingleStepSearcher.HiddenSinglesInBlockFirst"/>
	public const string HiddenSinglesInBlockFirst = nameof(HiddenSinglesInBlockFirst);

	/// <inheritdoc cref="SingleStepSearcher.EnableOrderingStepsByLastingValue"/>
	public const string EnableOrderingStepsByLastingValue = nameof(EnableOrderingStepsByLastingValue);

	/// <inheritdoc cref="DirectIntersectionStepSearcher.AllowDirectPointing"/>
	public const string AllowDirectPointing = nameof(AllowDirectPointing);

	/// <inheritdoc cref="DirectIntersectionStepSearcher.AllowDirectClaiming"/>
	public const string AllowDirectClaiming = nameof(AllowDirectClaiming);

	/// <inheritdoc cref="DirectSubsetStepSearcher.AllowDirectHiddenSubset"/>
	public const string AllowDirectHiddenSubset = nameof(AllowDirectHiddenSubset);

	/// <inheritdoc cref="DirectSubsetStepSearcher.AllowDirectLockedHiddenSubset"/>
	public const string AllowDirectLockedHiddenSubset = nameof(AllowDirectLockedHiddenSubset);

	/// <inheritdoc cref="DirectSubsetStepSearcher.AllowDirectNakedSubset"/>
	public const string AllowDirectNakedSubset = nameof(AllowDirectNakedSubset);

	/// <inheritdoc cref="DirectSubsetStepSearcher.AllowDirectLockedSubset"/>
	public const string AllowDirectLockedSubset = nameof(AllowDirectLockedSubset);

	/// <inheritdoc cref="DirectSubsetStepSearcher.DirectNakedSubsetMaxSize"/>
	public const string DirectNakedSubsetMaxSize = nameof(DirectNakedSubsetMaxSize);

	/// <inheritdoc cref="DirectSubsetStepSearcher.DirectHiddenSubsetMaxSize"/>
	public const string DirectHiddenSubsetMaxSize = nameof(DirectHiddenSubsetMaxSize);

	/// <inheritdoc cref="ComplexSingleStepSearcher.NakedSubsetMaxSize"/>
	public const string NakedSubsetMaxSizeInComplexSingle = nameof(NakedSubsetMaxSizeInComplexSingle);

	/// <inheritdoc cref="ComplexSingleStepSearcher.HiddenSubsetMaxSize"/>
	public const string HiddenSubsetMaxSizeInComplexSingle = nameof(HiddenSubsetMaxSizeInComplexSingle);

	/// <inheritdoc cref="NormalFishStepSearcher.DisableFinnedOrSashimiXWing"/>
	public const string DisableFinnedOrSashimiXWing = nameof(DisableFinnedOrSashimiXWing);

	/// <inheritdoc cref="TwoStrongLinksStepSearcher.DisableRemotePair"/>
	public const string DisableRemotePair = nameof(DisableRemotePair);

	/// <inheritdoc cref="GroupedTwoStrongLinksStepSearcher.DisableGroupedTurbotFish"/>
	public const string DisableGroupedTurbotFish = nameof(DisableGroupedTurbotFish);

	/// <inheritdoc cref="NormalFishStepSearcher.AllowSiamese"/>
	public const string AllowSiameseNormalFish = nameof(AllowSiameseNormalFish);

	/// <inheritdoc cref="ComplexFishStepSearcher.AllowSiamese"/>
	public const string AllowSiameseComplexFish = nameof(AllowSiameseComplexFish);

	/// <inheritdoc cref="XyzRingStepSearcher.AllowSiamese"/>
	public const string AllowSiameseXyzRing = nameof(AllowSiameseXyzRing);

	/// <inheritdoc cref="UniqueRectangleStepSearcher.AllowIncompleteUniqueRectangles"/>
	public const string AllowIncompleteUniqueRectangles = nameof(AllowIncompleteUniqueRectangles);

	/// <inheritdoc cref="UniqueRectangleStepSearcher.SearchForExtendedUniqueRectangles"/>
	public const string SearchForExtendedUniqueRectangles = nameof(SearchForExtendedUniqueRectangles);

	/// <inheritdoc cref="UniqueLoopStepSearcher.SearchExtendedTypes"/>
	public const string SearchForExtendedUniqueLoops = nameof(SearchForExtendedUniqueLoops);

	/// <inheritdoc cref="BivalueUniversalGraveStepSearcher.SearchExtendedTypes"/>
	public const string SearchExtendedBivalueUniversalGraveTypes = nameof(SearchExtendedBivalueUniversalGraveTypes);

	/// <inheritdoc cref="ChainStepSearcher.MakeConclusionAroundBackdoors"/>
	public const string MakeConclusionAroundBackdoorsNormalChain = nameof(MakeConclusionAroundBackdoorsNormalChain);

	/// <inheritdoc cref="GroupedChainStepSearcher.MakeConclusionAroundBackdoors"/>
	public const string MakeConclusionAroundBackdoorsGroupedChain = nameof(MakeConclusionAroundBackdoorsGroupedChain);

	/// <inheritdoc cref="ReverseBivalueUniversalGraveStepSearcher.AllowPartiallyUsedTypes"/>
	public const string SearchForReverseBugPartiallyUsedTypes = nameof(SearchForReverseBugPartiallyUsedTypes);

	/// <inheritdoc cref="ReverseBivalueUniversalGraveStepSearcher.MaxSearchingEmptyCellsCount"/>
	public const string ReverseBugMaxSearchingEmptyCellsCount = nameof(ReverseBugMaxSearchingEmptyCellsCount);

	/// <inheritdoc cref="AlignedExclusionStepSearcher.MaxSearchingSize"/>
	public const string AlignedExclusionMaxSearchingSize = nameof(AlignedExclusionMaxSearchingSize);

	/// <inheritdoc cref="RegularWingStepSearcher.MaxSearchingPivotsCount"/>
	public const string MaxSizeOfRegularWing = nameof(MaxSizeOfRegularWing);

	/// <inheritdoc cref="ComplexFishStepSearcher.MaxSize"/>
	public const string MaxSizeOfComplexFish = nameof(MaxSizeOfComplexFish);

	/// <inheritdoc cref="TemplateStepSearcher.TemplateDeleteOnly"/>
	public const string TemplateDeleteOnly = nameof(TemplateDeleteOnly);

	/// <inheritdoc cref="BowmanBingoStepSearcher.MaxLength"/>
	public const string BowmanBingoMaxLength = nameof(BowmanBingoMaxLength);

	/// <inheritdoc cref="AlmostLockedCandidatesStepSearcher.CheckAlmostLockedQuadruple"/>
	public const string CheckAlmostLockedQuadruple = nameof(CheckAlmostLockedQuadruple);

	/// <inheritdoc cref="AlmostLockedCandidatesStepSearcher.CheckValueTypes"/>
	public const string AlmostLockedCandidatesCheckValueTypes = nameof(AlmostLockedCandidatesCheckValueTypes);

	/// <inheritdoc cref="DeathBlossomStepSearcher.SearchExtendedTypes"/>
	public const string SearchExtendedDeathBlossomTypes = nameof(SearchExtendedDeathBlossomTypes);

	//
	// Analyzer & Collector property names
	//
	/// <inheritdoc cref="Analyzer.IsFullApplying"/>
	public const string AnalyzerIsFullApplying = nameof(AnalyzerIsFullApplying);

	/// <inheritdoc cref="Collector.DifficultyLevelMode"/>
	public const string DifficultyLevelMode = nameof(DifficultyLevelMode);

	/// <inheritdoc cref="Collector.MaxStepsCollected"/>
	public const string CollectorMaxStepsCollected = nameof(CollectorMaxStepsCollected);

	/// <summary>
	/// Indicates the supported techniques used in ittoryu mode.
	/// </summary>
	public const string IttoryuSupportedTechniques = nameof(IttoryuSupportedTechniques);
}
