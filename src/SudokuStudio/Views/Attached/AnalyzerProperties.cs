namespace SudokuStudio.Views.Attached;

/// <summary>
/// Defines a bind behaviors on <see cref="SudokuPane"/> instances, for <see cref="Analyzer"/> instance's interaction.
/// </summary>
/// <remarks>
/// All names of attached properties in this type can be corresponded to the target property in one <see cref="StepSearcher"/>,
/// via <see cref="SettingItemNameAttribute"/>.
/// </remarks>
/// <seealso cref="SudokuPane"/>
/// <seealso cref="Analyzer"/>
public static partial class AnalyzerProperties
{
	/// <summary>
	/// Indicates the anonymous name for getters.
	/// </summary>
	private const string GetSetterName = "Get";


	[Default]
	private static readonly List<Technique> IttoryuSupportedTechniquesDefaultValue = [
		Technique.FullHouse,
		Technique.HiddenSingleBlock,
		Technique.HiddenSingleRow,
		Technique.HiddenSingleColumn,
		Technique.NakedSingle
	];


	/// <inheritdoc cref="Analyzer.IsFullApplying"/>
	[DependencyProperty]
	public static partial bool AnalyzerIsFullApplying { get; set; }


	/// <inheritdoc cref="DisorderedIttoryuFinder.SupportedTechniques"/>
	[DependencyProperty]
	public static partial List<Technique> IttoryuSupportedTechniques { get; set; }


	/// <inheritdoc cref="SingleStepSearcher.EnableFullHouse"/>
	[DependencyProperty(DefaultValue = true)]
	public static partial bool EnableFullHouse { get; set; }

	/// <inheritdoc cref="SingleStepSearcher.EnableLastDigit"/>
	[DependencyProperty(DefaultValue = true)]
	public static partial bool EnableLastDigit { get; set; }

	/// <inheritdoc cref="SingleStepSearcher.HiddenSinglesInBlockFirst"/>
	[DependencyProperty(DefaultValue = true)]
	public static partial bool HiddenSinglesInBlockFirst { get; set; }

	/// <inheritdoc cref="SingleStepSearcher.EnableOrderingStepsByLastingValue"/>
	[DependencyProperty(DefaultValue = true)]
	public static partial bool EnableOrderingStepsByLastingValue { get; set; }

	/// <inheritdoc cref="DirectIntersectionStepSearcher.AllowDirectPointing"/>
	[DependencyProperty(DefaultValue = true)]
	public static partial bool AllowDirectPointing { get; set; }

	/// <inheritdoc cref="DirectIntersectionStepSearcher.AllowDirectClaiming"/>
	[DependencyProperty(DefaultValue = true)]
	public static partial bool AllowDirectClaiming { get; set; }

	/// <inheritdoc cref="DirectSubsetStepSearcher.AllowDirectLockedSubset"/>
	[DependencyProperty(DefaultValue = true)]
	public static partial bool AllowDirectLockedSubset { get; set; }

	/// <inheritdoc cref="DirectSubsetStepSearcher.AllowDirectNakedSubset"/>
	[DependencyProperty(DefaultValue = true)]
	public static partial bool AllowDirectNakedSubset { get; set; }

	/// <inheritdoc cref="DirectSubsetStepSearcher.AllowDirectLockedHiddenSubset"/>
	[DependencyProperty(DefaultValue = true)]
	public static partial bool AllowDirectLockedHiddenSubset { get; set; }

	/// <inheritdoc cref="DirectSubsetStepSearcher.AllowDirectHiddenSubset"/>
	[DependencyProperty(DefaultValue = true)]
	public static partial bool AllowDirectHiddenSubset { get; set; }

	/// <inheritdoc cref="DirectSubsetStepSearcher.DirectNakedSubsetMaxSize"/>
	[DependencyProperty(DefaultValue = 2)]
	public static partial int DirectNakedSubsetMaxSize { get; set; }

	/// <inheritdoc cref="DirectSubsetStepSearcher.DirectHiddenSubsetMaxSize"/>
	[DependencyProperty(DefaultValue = 2)]
	public static partial int DirectHiddenSubsetMaxSize { get; set; }

	/// <inheritdoc cref="ComplexSingleStepSearcher.NakedSubsetMaxSize"/>
	[DependencyProperty(DefaultValue = 4)]
	public static partial int NakedSubsetMaxSizeInComplexSingle { get; set; }

	/// <inheritdoc cref="ComplexSingleStepSearcher.HiddenSubsetMaxSize"/>
	[DependencyProperty(DefaultValue = 4)]
	public static partial int HiddenSubsetMaxSizeInComplexSingle { get; set; }

	/// <inheritdoc cref="NormalFishStepSearcher.DisableFinnedOrSashimiXWing"/>
	[DependencyProperty(DefaultValue = true)]
	public static partial bool DisableFinnedOrSashimiXWing { get; set; }

	/// <inheritdoc cref="GroupedTwoStrongLinksStepSearcher.DisableGroupedTurbotFish"/>
	[DependencyProperty(DefaultValue = true)]
	public static partial bool DisableGroupedTurbotFish { get; set; }

	/// <inheritdoc cref="NormalFishStepSearcher.AllowSiamese"/>
	[DependencyProperty]
	public static partial bool AllowSiameseNormalFish { get; set; }

	/// <inheritdoc cref="ComplexFishStepSearcher.AllowSiamese"/>
	[DependencyProperty]
	public static partial bool AllowSiameseComplexFish { get; set; }

	/// <inheritdoc cref="ComplexFishStepSearcher.MaxSize"/>
	[DependencyProperty(DefaultValue = 5)]
	public static partial int MaxSizeOfComplexFish { get; set; }

	/// <inheritdoc cref="RegularWingStepSearcher.MaxSearchingPivotsCount"/>
	[DependencyProperty(DefaultValue = 5)]
	public static partial int MaxSizeOfRegularWing { get; set; }

	/// <inheritdoc cref="UniqueRectangleStepSearcher.AllowIncompleteUniqueRectangles"/>
	[DependencyProperty(DefaultValue = true)]
	public static partial bool AllowIncompleteUniqueRectangles { get; set; }

	/// <inheritdoc cref="UniqueRectangleStepSearcher.SearchForExtendedUniqueRectangles"/>
	[DependencyProperty(DefaultValue = true)]
	public static partial bool SearchForExtendedUniqueRectangles { get; set; }

	/// <inheritdoc cref="BivalueUniversalGraveStepSearcher.SearchExtendedTypes"/>
	[DependencyProperty(DefaultValue = true)]
	public static partial bool SearchExtendedBivalueUniversalGraveTypes { get; set; }

	/// <inheritdoc cref="AlmostLockedCandidatesStepSearcher.CheckValueTypes"/>
	[DependencyProperty]
	public static partial bool AlmostLockedCandidatesCheckValueTypes { get; set; }

	/// <inheritdoc cref="AlmostLockedCandidatesStepSearcher.CheckAlmostLockedQuadruple"/>
	[DependencyProperty]
	public static partial bool CheckAlmostLockedQuadruple { get; set; }

	/// <inheritdoc cref="ChainStepSearcher.MakeConclusionAroundBackdoors"/>
	[DependencyProperty(DefaultValue = false)]
	public static partial bool MakeConclusionAroundBackdoorsNormalChain { get; set; }

	/// <inheritdoc cref="GroupedChainStepSearcher.MakeConclusionAroundBackdoors"/>
	[DependencyProperty(DefaultValue = false)]
	public static partial bool MakeConclusionAroundBackdoorsGroupedChain { get; set; }

	/// <inheritdoc cref="DeathBlossomStepSearcher.SearchExtendedTypes"/>
	[DependencyProperty]
	public static partial bool SearchExtendedDeathBlossomTypes { get; set; }

	/// <inheritdoc cref="ReverseBivalueUniversalGraveStepSearcher.AllowPartiallyUsedTypes"/>
	[DependencyProperty(DefaultValue = true)]
	public static partial bool SearchForReverseBugPartiallyUsedTypes { get; set; }

	/// <inheritdoc cref="ReverseBivalueUniversalGraveStepSearcher.MaxSearchingEmptyCellsCount"/>
	[DependencyProperty(DefaultValue = 2)]
	public static partial int ReverseBugMaxSearchingEmptyCellsCount { get; set; }

	/// <inheritdoc cref="XyzRingStepSearcher.AllowSiamese"/>
	[DependencyProperty]
	public static partial bool AllowSiameseXyzRing { get; set; }

	/// <inheritdoc cref="AlignedExclusionStepSearcher.MaxSearchingSize"/>
	[DependencyProperty(DefaultValue = 3)]
	public static partial int AlignedExclusionMaxSearchingSize { get; set; }

	/// <inheritdoc cref="TemplateStepSearcher.TemplateDeleteOnly"/>
	[DependencyProperty]
	public static partial bool TemplateDeleteOnly { get; set; }

	/// <inheritdoc cref="BowmanBingoStepSearcher.MaxLength"/>
	[DependencyProperty(DefaultValue = 64)]
	public static partial int BowmanBingoMaxLength { get; set; }

	/// <inheritdoc cref="StepGathererOptions.UseIttoryuMode"/>
	[DependencyProperty]
	public static partial bool AnalyzerUseIttoryuMode { get; set; }


	/// <summary>
	/// Sets the specified property in a <see cref="StepSearcher"/> with the target value via attached properties
	/// stored in type <see cref="AnalyzerProperties"/>.
	/// </summary>
	/// <param name="this">The analyzer instance.</param>
	/// <param name="attachedPropertyValue">The attached property.</param>
	/// <param name="methodName">The name of the property <paramref name="attachedPropertyValue"/>.</param>
	/// <param name="propertyMatched">
	/// A <see cref="bool"/> value indicating whether the property in <see cref="StepSearcher"/> collection is found,
	/// and the target type of that property is same as argument <paramref name="attachedPropertyValue"/>.
	/// </param>
	/// <returns>The same reference as <paramref name="this"/>.</returns>
	/// <seealso cref="AnalyzerProperties"/>
	public static T WithRuntimeIdentifierSetter<T>(
		this T @this,
		object attachedPropertyValue,
		string methodName,
		out bool propertyMatched
	)
		where T : StepGatherer
	{
		foreach (var searcher in @this.ResultStepSearchers)
		{
			if (searcher.GetType().GetProperties().FirstOrDefault(methodNameMatcher) is { } propertyInfo)
			{
				propertyInfo.SetValue(searcher, attachedPropertyValue);
				propertyMatched = true;
				return @this;
			}


			bool methodNameMatcher(PropertyInfo property)
				=> property.GetCustomAttribute<SettingItemNameAttribute>() is { Identifier: var identifier }
				&& methodName[GetSetterName.Length..] == identifier;
		}

		propertyMatched = false;
		return @this;
	}

	/// <summary>
	/// Calls the method <see cref="WithRuntimeIdentifierSetter"/> for all properties in type <see cref="AnalyzerProperties"/>.
	/// </summary>
	/// <param name="this">The analyzer instance.</param>
	/// <param name="attachedPane">Indicates the <see cref="SudokuPane"/> instance that all properties in this type attached to.</param>
	/// <returns>The same reference with argument <paramref name="this"/>.</returns>
	/// <exception cref="InvalidOperationException">Throws when the matched property is invalid.</exception>
	/// <seealso cref="WithRuntimeIdentifierSetter"/>
	public static T WithRuntimeIdentifierSetters<T>(this T @this, SudokuPane attachedPane)
		where T : StepGatherer
	{
		foreach (var methodInfo in typeof(AnalyzerProperties).GetMethods(BindingFlags.Static | BindingFlags.Public))
		{
			if (methodInfo.Name.StartsWith(GetSetterName))
			{
				@this.WithRuntimeIdentifierSetter(methodInfo.Invoke(null, [attachedPane])!, methodInfo.Name, out _);
			}
		}
		return @this;
	}
}
