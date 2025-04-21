namespace SudokuStudio.Interaction.Conversions;

/// <summary>
/// Provides with conversion methods used by XAML designer, about technique.
/// </summary>
internal static class TechniqueConversion
{
	/// <summary>
	/// Indicates all technique groups that contains configurable techniques.
	/// </summary>
	public static readonly TechniqueGroup[] ConfigurableTechniqueGroups = [.. TechniqueSet.ConfigurableTechniqueRelationGroups.Keys];


	public static bool IsTechniqueConfigPreviousButtonEnabled(int index) => index > 0;

	public static bool IsTechniqueConfigNextButtonEnabled(int index) => index < ConfigurableTechniqueGroups.Length - 1;

	public static int GetDisplayNameColumnSpan(TechniqueFeatures feature) => feature == TechniqueFeatures.None ? 2 : 1;

	public static string GetName(Technique technique) => technique == Technique.None ? string.Empty : technique.GetName(App.CurrentCulture);

	public static string GetEnglishName(Technique technique) => technique == Technique.None ? string.Empty : technique.EnglishName;

	public static string GetDifficultyLevel(Technique technique)
		=> technique == Technique.None
			? string.Empty
			: DifficultyLevelConversion.GetNameWithDefault(technique.DifficultyLevel, SR.Get("TechniqueSelectionPage_NoDifficultyLevel", App.CurrentCulture));

	public static string GetAliasNames(Technique technique)
		=> technique == Technique.None
			? string.Empty
			: technique.GetAliasedNames(App.CurrentCulture) is { Length: not 0 } a ? string.Join(", ", a) : SR.Get("TechniqueSelectionPage_NoAliases", App.CurrentCulture);

	public static string GetAbbreviation(Technique technique)
		=> technique == Technique.None
			? string.Empty
			: technique.Abbreviation ?? SR.Get("TechniqueSelectionPage_NoAbbreviation", App.CurrentCulture);

	public static string GetProgramRawName(Technique technique) => technique.ToString();

	public static string GetGroup(Technique technique)
		=> technique == Technique.None ? string.Empty : technique.Group.GetName(App.CurrentCulture);

	public static string GetGroupShortenedName(Technique technique)
		=> technique == Technique.None ? string.Empty : technique.Group.GetShortenedName(App.CurrentCulture);

	public static string GetFeature(Technique technique)
		=> technique == Technique.None
			? string.Empty
			: GetStringResourceViaFeature(technique.Features) is var p and not "" ? p : SR.Get("TechniqueSelectionPage_NoExtraFeatures", App.CurrentCulture);

	public static string GetFeatureDescription(Technique technique)
		=> technique == Technique.None
			? string.Empty
			: GetStringTooltipViaFeature(technique.Features) is { Length: not 0 } p ? p : SR.Get("TechniqueSelectionPage_NoExtraFeaturesDescription", App.CurrentCulture);

	public static string GetSudokuExplainerDifficultyRange(Technique technique)
	{
		var advancedText = SR.Get("TechniqueSelectionPage_AdvancedDefined", App.CurrentCulture);
		var nullDefinedText = SR.Get("TechniqueSelectionPage_NullDefined", App.CurrentCulture);
		return technique switch
		{
			Technique.None => string.Empty,
			_ => SudokuExplainerCompatibility.GetDifficultyRatingRange(technique) switch
			{
				({ IsRange: true, Min: var d1, Max: var d2 }, { IsRange: true, Min: var d3, Max: var d4 })
					=> $"{d1:#.0} - {d2:#.0}{Environment.NewLine}{d3:#.0} - {d4:#.0}{advancedText}",
				({ IsRange: true, Min: var d1, Max: var d2 }, { IsRange: false, Min: var d3 })
					=> $"{d1:#.0} - {d2:#.0}{Environment.NewLine}{d3:#.0}{advancedText}",
				({ IsRange: false, Min: var d1 }, { IsRange: true, Min: var d3, Max: var d4 })
					=> $"{d1:#.0}{Environment.NewLine}{d3:#.0} - {d4:#.0}{advancedText}",
				({ Min: var d1 }, { Min: var d3 })
					=> $"{d1:#.0}{Environment.NewLine}{d3:#.0}{advancedText}",
				({ IsRange: false, Min: var d }, null)
					=> $"{d:#.0}",
				({ IsRange: true, Min: var d1, Max: var d2 }, null)
					=> $"{d1:#.0} - {d2:#.0}",
				(_, { IsRange: false, Min: var d })
					=> $"{d:#.0}{advancedText}",
				(_, { IsRange: true, Min: var d1, Max: var d2 })
					=> $"{d1:#.0} - {d2:#.0}{advancedText}",
				_
					=> nullDefinedText,
			}
		};
	}

	public static string GetHodokuDifficultyRating(Technique technique)
		=> technique == Technique.None ? string.Empty : HodokuCompatibility.GetDifficultyScore(technique, out var difficultyLevel) switch
		{
			{ } value => $"{value}{SR.Get("_Token_OpenBrace", App.CurrentCulture)}{SR.Get(difficultyLevel switch
			{
				HodokuDifficultyLevel.Easy => "HodokuDifficultyLevel_Easy",
				HodokuDifficultyLevel.Medium => "HodokuDifficultyLevel_Medium",
				HodokuDifficultyLevel.Hard => "HodokuDifficultyLevel_Hard",
				HodokuDifficultyLevel.Unfair => "HodokuDifficultyLevel_Unfair",
				HodokuDifficultyLevel.Extreme => "HodokuDifficultyLevel_Extreme"
			}, App.CurrentCulture)}{SR.Get("_Token_ClosedBrace", App.CurrentCulture)}",
			_ => SR.Get("TechniqueSelectionPage_NullDefined", App.CurrentCulture)
		};

	public static string GetHodokuPrefix(Technique technique)
		=> technique == Technique.None ? string.Empty : HodokuCompatibility.GetHodokuLibraryPrefix(technique) ?? SR.Get("TechniqueSelectionPage_NoHodokuPrefix", App.CurrentCulture);

	public static string GetStringResourceViaFeature(TechniqueFeatures feature)
		=> feature switch
		{
			TechniqueFeatures.HardToBeGenerated => SR.Get("TechniqueFeature_HardToBeGeneratedShort", App.CurrentCulture),
			TechniqueFeatures.WillBeReplacedByOtherTechnique => SR.Get("TechniqueFeature_WillBeReplacedByOtherTechniqueShort", App.CurrentCulture),
			TechniqueFeatures.OnlyExistInTheory => SR.Get("TechniqueFeature_OnlyExistInTheoryShort", App.CurrentCulture),
			TechniqueFeatures.NotImplemented => SR.Get("TechniqueFeature_NotImplementedShort", App.CurrentCulture),
			TechniqueFeatures.DirectTechniques => SR.Get("TechniqueFeature_DirectTechniquesShort", App.CurrentCulture),
			_ => string.Empty
		};

	public static string? GetStringTooltipViaFeature(TechniqueFeatures feature)
		=> feature switch
		{
			TechniqueFeatures.HardToBeGenerated => SR.Get("TechniqueFeature_HardToBeGenerated", App.CurrentCulture),
			TechniqueFeatures.WillBeReplacedByOtherTechnique => SR.Get("TechniqueFeature_WillBeReplacedByOtherTechnique", App.CurrentCulture),
			TechniqueFeatures.OnlyExistInTheory => SR.Get("TechniqueFeature_OnlyExistInTheory", App.CurrentCulture),
			TechniqueFeatures.NotImplemented => SR.Get("TechniqueFeature_NotImplemented", App.CurrentCulture),
			TechniqueFeatures.DirectTechniques => SR.Get("TechniqueFeature_DirectTechniques", App.CurrentCulture),
			_ => null
		};

	public static Visibility GetExtraDescriptionVisibility(TechniqueFeatures feature)
		=> feature == TechniqueFeatures.None ? Visibility.Collapsed : Visibility.Visible;

	public static Visibility GetFeatureVisibility(Technique technique) => GetExtraDescriptionVisibility(technique.Features);

	public static Visibility GetSelectAllButtonVisibility(bool value) => value ? Visibility.Visible : Visibility.Collapsed;

	public static Brush GetBrush(TechniqueFeatures feature)
		=> new SolidColorBrush(feature == TechniqueFeatures.None ? Colors.Black : Colors.LightGray);

	public static string[] GetIntroductionLinks(Technique technique) => technique.ReferenceLinks.ToArray();
}
