namespace SudokuStudio.Interaction.Conversions;

/// <summary>
/// Provides with conversion methods used by XAML designer, about analyze tab pages.
/// </summary>
internal static class AnalyzeConversion
{
	public static bool GetIsEnabled(Grid grid) => grid.Uniqueness != Uniqueness.Bad && !grid.IsSolved;

	public static bool GetAnalyzerButtonIsEnabled(bool isGeneratorLaunched) => !isGeneratorLaunched;

	public static bool GetProgressRingIsIntermediate(bool isAnalyzerLaunched, bool isCollectorLaunched, bool isGeneratorLaunched)
		=> (isAnalyzerLaunched, isCollectorLaunched, isGeneratorLaunched) switch { (_, _, true) => true, _ => false };

	public static bool GetProgressRingIsActive(bool isAnalyzerLaunched, bool isCollectorLaunched, bool isGeneratorLaunched)
		=> (isAnalyzerLaunched, isCollectorLaunched, isGeneratorLaunched) switch { (_, _, true) => false, _ => true };

	public static int GetViewPipsPagerPageCount(IDrawable? drawable) => drawable?.Views.Length ?? 0;

	public static int GetCurrentViewIndexForViewPipsPager(int currentIndex) => currentIndex;

	public static double GetWidth_HodokuRatingText(bool showing) => showing ? 40 : 0;

	public static double GetWidth_SudokuExplainerText(bool showing) => showing ? 60 : 0;

	public static string GetEliminationString(Step step) => step.Options.Converter.ConclusionConverter(step.Conclusions.Span);

	public static string GetDifficultyRatingText(Step step)
	{
		var pref = Application.Current.AsApp().Preference.TechniqueInfoPreferences;
		var resultDifficulty = pref.GetRating(step.Code) switch { { } v => v, _ => step.Difficulty } / pref.RatingScale;
		return resultDifficulty.ToString(FactorMarshal.GetScaleFormatString(1 / pref.RatingScale));
	}

	public static string GetDifficultyRatingText_Hodoku(Step step)
		=> HodokuCompatibility.GetDifficultyScore(step.Code, out _) is { } r ? r.ToString() : string.Empty;

	public static string GetDifficultyRatingText_SudokuExplainer(Step step)
		=> SudokuExplainerCompatibility.GetDifficultyRatingRange(step.Code) switch
		{
			({ IsRange: false, Min: var d }, _) => $"{d:0.0}",
			({ IsRange: true, Min: var d1, Max: var d2 }, _) => $"{d1:0.0}-{d2:0.0}",
			(_, { IsRange: false, Min: var d }) => $"{d:0.0}",
			(_, { IsRange: true, Min: var d1, Max: var d2 }) => $"{d1:0.0}-{d2:0.0}",
			_ => string.Empty
		};

	public static string GetIndexText(SolvingPathStepBindableSource step) => (step.Index + 1).ToString();

	public static string GetViewIndexDisplayerString(IDrawable? visualUnit, int currentIndex)
		=> visualUnit?.Views.Length is { } length ? $"{currentIndex + 1}/{length}" : "0/0";

	public static string GetName(Step step) => step.GetName(App.CurrentCulture);

	public static string GetSimpleString(Step step) => step.ToSimpleString(App.CurrentCulture);

	public static Thickness GetMargin_HodokuRating(bool showing) => showing ? new(12, 0, 0, 0) : new();

	public static Thickness GetMargin_SudokuExplainerRating(bool showing) => showing ? new(12, 0, 0, 0) : new();

	public static Visibility GetProgressRingVisibility(bool isAnalyzerLaunched, bool isGathererLaunched, bool isGeneratorLaunched)
		=> isAnalyzerLaunched || isGathererLaunched || isGeneratorLaunched ? Visibility.Visible : Visibility.Collapsed;

	public static Visibility GetAnalyzeTabsVisibility(bool isAnalyzerLaunched, bool isGathererLaunched, bool isGeneratorLaunched)
		=> isAnalyzerLaunched || isGathererLaunched || isGeneratorLaunched ? Visibility.Collapsed : Visibility.Visible;

	public static Visibility GetDifficultyRatingVisibility(bool showDifficultyRating)
		=> showDifficultyRating ? Visibility.Visible : Visibility.Collapsed;

	public static Visibility GetSummaryTableVisibility(IEnumerable itemsSource)
		=> itemsSource is null || !itemsSource.Cast<object>().Any() ? Visibility.Collapsed : Visibility.Visible;

	public static Visibility GetSolvingPathListVisibility(object itemsSource)
		=> itemsSource switch
		{
			ObservableCollection<SolvingPathStepBindableSource> => Visibility.Visible,
			_ => Visibility.Collapsed
		};

	public static Visibility GetViewPipsPagerVisibility(IDrawable? drawable)
		=> drawable switch { { Views.Length: >= 2 } => Visibility.Visible, _ => Visibility.Collapsed };

	public static Visibility GetEnglishNameTextBlockVisibility()
		=> Application.Current.AsApp().Preference.AnalysisPreferences.AlsoDisplayEnglishNameOfStep
		&& App.CurrentCulture.Name != SR.EnglishLanguage
			? Visibility.Visible
			: Visibility.Collapsed;

	public static IEnumerable<Inline> GetInlinesOfTooltip(SolvingPathStepBindableSource s)
	{
		if (s is not
			{
				Index: var index,
				DisplayItems: var displayKind,
				InterimStep:
				{
					Code: var technique,
					BaseDifficulty: var @base,
					Difficulty: var difficulty,
					Factors: var factors
				} step
			})
		{
			throw new ArgumentException($"The argument '{nameof(s)}' is invalid.", nameof(s));
		}

		var pref = Application.Current.AsApp().Preference.TechniqueInfoPreferences;
		var result = new List<Inline>();

		if (displayKind.HasFlag(StepTooltipDisplayItems.TechniqueName))
		{
			result.Add(new Run { Text = SR.Get("AnalyzePage_TechniqueName", App.CurrentCulture) }.SingletonSpan<Bold>());
			result.Add(new LineBreak());
			result.Add(new Run { Text = step.GetName(App.CurrentCulture) });
		}

		if (displayKind.HasFlag(StepTooltipDisplayItems.TechniqueIndex))
		{
			appendEmptyLinesIfNeed();

			result.Add(new Run { Text = SR.Get("AnalyzePage_TechniqueIndex", App.CurrentCulture) }.SingletonSpan<Bold>());
			result.Add(new LineBreak());
			result.Add(new Run { Text = (index + 1).ToString() });
		}

		if (displayKind.HasFlag(StepTooltipDisplayItems.Abbreviation))
		{
			appendEmptyLinesIfNeed();

			result.Add(new Run { Text = SR.Get("AnalyzePage_Abbreviation", App.CurrentCulture) }.SingletonSpan<Bold>());
			result.Add(new LineBreak());
			result.Add(new Run { Text = technique.Abbreviation ?? SR.Get("AnalyzePage_None", App.CurrentCulture) });
		}

		if (displayKind.HasFlag(StepTooltipDisplayItems.Aliases))
		{
			appendEmptyLinesIfNeed();

			result.Add(new Run { Text = SR.Get("AnalyzePage_Aliases", App.CurrentCulture) }.SingletonSpan<Bold>());
			result.Add(new LineBreak());
			result.Add(
				new Run
				{
					Text = technique.GetAliasedNames(App.CurrentCulture) is { } aliases and not []
						? string.Join(SR.Get("_Token_Comma", App.CurrentCulture), aliases)
						: SR.Get("AnalyzePage_None", App.CurrentCulture)
				}
			);
		}

		if (displayKind.HasFlag(StepTooltipDisplayItems.DifficultyRating))
		{
			appendEmptyLinesIfNeed();

			var difficultyValue = pref.GetRating(technique) switch { { } v => v, _ => difficulty } / pref.RatingScale;
			var difficultyValueString = difficultyValue.ToString(FactorMarshal.GetScaleFormatString(1 / pref.RatingScale));
			result.Add(new Run { Text = SR.Get("AnalyzePage_TechniqueDifficultyRating", App.CurrentCulture) }.SingletonSpan<Bold>());
			result.Add(new LineBreak());
			result.Add(new Run { Text = difficultyValueString });
		}

		if (displayKind.HasFlag(StepTooltipDisplayItems.ExtraDifficultyCases))
		{
			appendEmptyLinesIfNeed();

			result.Add(new Run { Text = SR.Get("AnalyzePage_ExtraDifficultyCase", App.CurrentCulture) }.SingletonSpan<Bold>());
			result.Add(new LineBreak());

			switch (factors)
			{
				case { Length: not 0 }:
				{
					var baseDifficulty = pref.GetRating(technique) switch { { } v => v, _ => @base } / pref.RatingScale;
					var baseDifficultyString = baseDifficulty.ToString(FactorMarshal.GetScaleFormatString(1 / pref.RatingScale));
					result.Add(new Run { Text = $"{SR.Get("AnalyzePage_BaseDifficulty", App.CurrentCulture)}{baseDifficultyString}" });
					result.Add(new LineBreak());
					result.AddRange(appendExtraDifficultyFactors(factors));
					break;
				}
				default:
				{
					result.Add(new Run { Text = SR.Get("AnalyzePage_None", App.CurrentCulture) });
					break;
				}
			}
		}

		if (displayKind.HasFlag(StepTooltipDisplayItems.SimpleDescription))
		{
			appendEmptyLinesIfNeed();

			result.Add(new Run { Text = SR.Get("AnalyzePage_SimpleDescription", App.CurrentCulture) }.SingletonSpan<Bold>());
			result.Add(new LineBreak());
			result.Add(new Run { Text = step.ToString(App.CurrentCulture) });
		}

		return result;


		void appendEmptyLinesIfNeed()
		{
			if (result.Count != 0)
			{
				result.Add(new LineBreak());
				result.Add(new LineBreak());
			}
		}

		ReadOnlySpan<Inline> appendExtraDifficultyFactors(FactorArray factors)
		{
			var result = new List<Inline>();
			for (var i = 0; i < factors.Length; i++)
			{
				result.Add(new Run { Text = $"{factors[i].ToString(step, pref.RatingScale, App.CurrentCulture)}" });
				if (i != factors.Length - 1)
				{
					result.Add(new LineBreak());
				}
			}
			return result.AsSpan();
		}
	}
}

/// <include file='../../global-doc-comments.xml' path='g/csharp11/feature[@name="file-local"]/target[@name="class" and @when="extension"]'/>
file static class Extensions
{
	/// <summary>
	/// Calculates the rating from the specified <see cref="Step"/> instance, and return the string representation
	/// of the rating text.
	/// </summary>
	/// <param name="this">The current instance.</param>
	/// <param name="step">The step to be calculated.</param>
	/// <param name="scale">The scale value to be used.</param>
	/// <param name="formatProvider">The culture to be used.</param>
	/// <returns>The string representation of final rating text.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ToString(this Factor @this, Step step, decimal scale, IFormatProvider? formatProvider)
	{
		var culture = formatProvider as CultureInfo;
		var colonCharacter = SR.Get("_Token_Colon", culture);
		return @this.Formula(from propertyInfo in @this.Parameters select propertyInfo.GetValue(step)!) switch
		{
			{ } result when (result / scale).ToString(FactorMarshal.GetScaleFormatString(1 / scale)) is var value
				=> $"{@this.GetName(culture)}{colonCharacter}{value}",
			_ => string.Empty
		};
	}
}
