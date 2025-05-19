namespace SudokuStudio.Configuration;

/// <summary>
/// Represents with preference items that is used by <see cref="Analyzer"/> or <see cref="Collector"/>.
/// </summary>
/// <seealso cref="Analyzer"/>
/// <seealso cref="Collector"/>
public sealed partial class AnalysisPreferenceGroup : PreferenceGroup
{
	[Default]
	private static readonly List<Technique> IttoryuSupportedTechniquesDefaultValue = [
		Technique.FullHouse,
		Technique.HiddenSingleBlock,
		Technique.HiddenSingleRow,
		Technique.HiddenSingleColumn,
		Technique.NakedSingle
	];

	[Default]
	private static readonly Dictionary<LinkType, LinkOption> OverriddenLinkOptionsDefaultValue = new()
	{
		{ LinkType.SingleDigit, LinkOption.All },
		{ LinkType.SingleCell, LinkOption.All },
		{ LinkType.KrakenNormalFish, LinkOption.None }
	};


	/// <inheritdoc cref="SingleStepSearcher.EnableFullHouse"/>
	[DependencyProperty(DefaultValue = true)]
	public partial bool EnableFullHouse { get; set; }

	/// <inheritdoc cref="SingleStepSearcher.EnableLastDigit"/>
	[DependencyProperty(DefaultValue = true)]
	public partial bool EnableLastDigit { get; set; }

	/// <inheritdoc cref="SingleStepSearcher.HiddenSinglesInBlockFirst"/>
	[DependencyProperty(DefaultValue = true)]
	public partial bool HiddenSinglesInBlockFirst { get; set; }

	/// <inheritdoc cref="SingleStepSearcher.EnableOrderingStepsByLastingValue"/>
	[DependencyProperty(DefaultValue = true)]
	public partial bool EnableOrderingStepsByLastingValue { get; set; }

	/// <inheritdoc cref="DirectIntersectionStepSearcher.AllowDirectPointing"/>
	[DependencyProperty(DefaultValue = true)]
	public partial bool AllowDirectPointing { get; set; }

	/// <inheritdoc cref="DirectIntersectionStepSearcher.AllowDirectClaiming"/>
	[DependencyProperty(DefaultValue = true)]
	public partial bool AllowDirectClaiming { get; set; }

	/// <inheritdoc cref="DirectSubsetStepSearcher.AllowDirectLockedSubset"/>
	[DependencyProperty(DefaultValue = true)]
	public partial bool AllowDirectLockedSubset { get; set; }

	/// <inheritdoc cref="DirectSubsetStepSearcher.AllowDirectNakedSubset"/>
	[DependencyProperty(DefaultValue = true)]
	public partial bool AllowDirectNakedSubset { get; set; }

	/// <inheritdoc cref="DirectSubsetStepSearcher.AllowDirectLockedHiddenSubset"/>
	[DependencyProperty(DefaultValue = true)]
	public partial bool AllowDirectLockedHiddenSubset { get; set; }

	/// <inheritdoc cref="DirectSubsetStepSearcher.AllowDirectHiddenSubset"/>
	[DependencyProperty(DefaultValue = true)]
	public partial bool AllowDirectHiddenSubset { get; set; }

	/// <inheritdoc cref="DirectSubsetStepSearcher.DirectNakedSubsetMaxSize"/>
	[DependencyProperty(DefaultValue = 2)]
	public partial int DirectNakedSubsetMaxSize { get; set; }

	/// <inheritdoc cref="DirectSubsetStepSearcher.DirectHiddenSubsetMaxSize"/>
	[DependencyProperty(DefaultValue = 2)]
	public partial int DirectHiddenSubsetMaxSize { get; set; }

	/// <inheritdoc cref="ComplexSingleStepSearcher.NakedSubsetMaxSize"/>
	[DependencyProperty(DefaultValue = 4)]
	public partial int NakedSubsetMaxSizeInComplexSingle { get; set; }

	/// <inheritdoc cref="ComplexSingleStepSearcher.HiddenSubsetMaxSize"/>
	[DependencyProperty(DefaultValue = 4)]
	public partial int HiddenSubsetMaxSizeInComplexSingle { get; set; }

	/// <inheritdoc cref="NormalFishStepSearcher.DisableFinnedOrSashimiXWing"/>
	[DependencyProperty(DefaultValue = true)]
	public partial bool DisableFinnedOrSashimiXWing { get; set; }

	/// <inheritdoc cref="GroupedTwoStrongLinksStepSearcher.DisableGroupedTurbotFish"/>
	[DependencyProperty(DefaultValue = true)]
	public partial bool DisableGroupedTurbotFish { get; set; }

	/// <inheritdoc cref="NormalFishStepSearcher.AllowSiamese"/>
	[DependencyProperty]
	public partial bool AllowSiameseNormalFish { get; set; }

	/// <inheritdoc cref="ComplexFishStepSearcher.AllowSiamese"/>
	[DependencyProperty]
	public partial bool AllowSiameseComplexFish { get; set; }

	/// <inheritdoc cref="ComplexFishStepSearcher.MaxSize"/>
	[DependencyProperty(DefaultValue = 5)]
	public partial int MaxSizeOfComplexFish { get; set; }

	/// <inheritdoc cref="UniqueRectangleStepSearcher.AllowIncompleteUniqueRectangles"/>
	[DependencyProperty(DefaultValue = true)]
	public partial bool AllowIncompleteUniqueRectangles { get; set; }

	/// <inheritdoc cref="UniqueRectangleStepSearcher.SearchForExtendedUniqueRectangles"/>
	[DependencyProperty(DefaultValue = true)]
	public partial bool SearchForExtendedUniqueRectangles { get; set; }

	/// <inheritdoc cref="BivalueUniversalGraveStepSearcher.SearchExtendedTypes"/>
	[DependencyProperty(DefaultValue = true)]
	public partial bool SearchExtendedBivalueUniversalGraveTypes { get; set; }

	/// <inheritdoc cref="AlmostLockedCandidatesStepSearcher.CheckValueTypes"/>
	[DependencyProperty]
	public partial bool AlmostLockedCandidatesCheckValueTypes { get; set; }

	/// <inheritdoc cref="AlmostLockedCandidatesStepSearcher.CheckAlmostLockedQuadruple"/>
	[DependencyProperty]
	public partial bool CheckAlmostLockedQuadruple { get; set; }

	/// <inheritdoc cref="XyzRingStepSearcher.AllowSiamese"/>
	[DependencyProperty]
	public partial bool AllowSiameseXyzRing { get; set; }

	/// <inheritdoc cref="RegularWingStepSearcher.MaxSearchingPivotsCount"/>
	[DependencyProperty(DefaultValue = 5)]
	public partial int MaxSizeOfRegularWing { get; set; }

	/// <inheritdoc cref="ChainStepSearcher.MakeConclusionAroundBackdoors"/>
	[DependencyProperty(DefaultValue = false)]
	public partial bool MakeConclusionAroundBackdoorsNormalChain { get; set; }

	/// <inheritdoc cref="GroupedChainStepSearcher.MakeConclusionAroundBackdoors"/>
	[DependencyProperty(DefaultValue = false)]
	public partial bool MakeConclusionAroundBackdoorsGroupedChain { get; set; }

	/// <inheritdoc cref="AlignedExclusionStepSearcher.MaxSearchingSize"/>
	[DependencyProperty(DefaultValue = 3)]
	public partial int AlignedExclusionMaxSearchingSize { get; set; }

	/// <inheritdoc cref="ReverseBivalueUniversalGraveStepSearcher.AllowPartiallyUsedTypes"/>
	[DependencyProperty(DefaultValue = true)]
	public partial bool SearchForReverseBugPartiallyUsedTypes { get; set; }

	/// <inheritdoc cref="ReverseBivalueUniversalGraveStepSearcher.MaxSearchingEmptyCellsCount"/>
	[DependencyProperty(DefaultValue = 2)]
	public partial int ReverseBugMaxSearchingEmptyCellsCount { get; set; }

	/// <inheritdoc cref="DeathBlossomStepSearcher.SearchExtendedTypes"/>
	[DependencyProperty(DefaultValue = true)]
	public partial bool SearchExtendedDeathBlossomTypes { get; set; }

	/// <summary>
	/// Indicates whether the step will be displayed its corresponding rating defined in program Sudoku Explainer.
	/// </summary>
	[DependencyProperty]
	public partial bool DisplayDifficultyRatingForSudokuExplainer { get; set; }


	/// <summary>
	/// Indicates whether the step analyzed will also display its English name of the technique used.
	/// </summary>
	[DependencyProperty]
	public partial bool AlsoDisplayEnglishNameOfStep { get; set; }

	/// <summary>
	/// Indicates whether the step will display its corresponding rating defined in program HoDoKu.
	/// </summary>
	[DependencyProperty]
	public partial bool DisplayDifficultyRatingForHodoku { get; set; }


	/// <inheritdoc cref="Analyzer.IsFullApplying"/>
	[DependencyProperty]
	public partial bool AnalyzerIsFullApplying { get; set; }


	/// <inheritdoc cref="Collector.MaxStepsCollected"/>
	[DependencyProperty(DefaultValue = 1000)]
	public partial int CollectorMaxStepsCollected { get; set; }

	/// <inheritdoc cref="Collector.DifficultyLevelMode"/>
	[DependencyProperty(DefaultValue = 0)]
	public partial int DifficultyLevelMode { get; set; }

	/// <inheritdoc cref="StepGathererOptions.UseIttoryuMode"/>
	[DependencyProperty]
	public partial bool AnalyzerUseIttoryuMode { get; set; }

	/// <inheritdoc cref="StepGathererOptions.BabaGroupInitialLetter"/>
	[DependencyProperty(DefaultValue = BabaGroupInitialLetter.EnglishLetter_X)]
	public partial BabaGroupInitialLetter InitialLetter { get; set; }

	/// <inheritdoc cref="StepGathererOptions.BabaGroupLetterCasing"/>
	[DependencyProperty(DefaultValue = BabaGroupLetterCasing.Lower)]
	public partial BabaGroupLetterCasing LetterCasing { get; set; }

	/// <summary>
	/// Indicates the bottleneck type defined in direct mode.
	/// </summary>
	[DependencyProperty(DefaultValue = BottleneckType.SingleStepOnly)]
	public partial BottleneckType DirectModeBottleneckType { get; set; }

	/// <summary>
	/// Indicates the bottleneck type defined in partial-marking moded.
	/// </summary>
	[DependencyProperty(DefaultValue = BottleneckType.HardestRating)]
	public partial BottleneckType PartialMarkingModeBottleneckType { get; set; }

	/// <summary>
	/// Indicates the bottleneck type defined in full-marking mode.
	/// </summary>
	[DependencyProperty(DefaultValue = BottleneckType.EliminationGroup)]
	public partial BottleneckType FullMarkingModeBottleneckType { get; set; }

	/// <inheritdoc cref="StepGathererOptions.OverriddenLinkOptions"/>
	[DependencyProperty]
	public partial Dictionary<LinkType, LinkOption> OverriddenLinkOptions { get; set; }

	/// <inheritdoc cref="DisorderedIttoryuFinder.SupportedTechniques"/>
	[DependencyProperty]
	public partial List<Technique> IttoryuSupportedTechniques { get; set; }
}
