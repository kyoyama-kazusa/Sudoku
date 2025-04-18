namespace SudokuStudio.Collection;

/// <summary>
/// Defines a solving path.
/// </summary>
public sealed class SolvingPathStepCollection : List<SolvingPathStepBindableSource>
{
	/// <summary>
	/// Creates a <see cref="SolvingPathStepCollection"/> instance via the specified <see cref="AnalysisResult"/> instance.
	/// </summary>
	/// <param name="analysisResult">A <see cref="AnalysisResult"/> instance.</param>
	/// <param name="displayItems">Indicates all displaying values.</param>
	/// <returns>An instance of the current type.</returns>
	public static IEnumerable<SolvingPathStepBindableSource> Create(AnalysisResult analysisResult, StepTooltipDisplayItems displayItems)
	{
		ReadOnlySpan<Step> steps = [];
		ReadOnlySpan<Grid> grids = [];
		switch (analysisResult)
		{
			case ({ IsSolved: true } or { IsPartiallySolved: true }) and { StepsSpan: var s, GridsSpan: var g }:
			{
				steps = s;
				grids = g;
				break;
			}
		}
		if (steps.Length == 0 || grids.Length == 0)
		{
			yield break;
		}

		var showHodoku = Application.Current.AsApp().Preference.AnalysisPreferences.DisplayDifficultyRatingForHodoku;
		var showSudokuExplainer = Application.Current.AsApp().Preference.AnalysisPreferences.DisplayDifficultyRatingForSudokuExplainer;
		var path = Step.Combine(grids, steps).ToArray();
		for (var i = 0; i < path.Length; i++)
		{
			var (interimGrid, interimStep) = path[i];
			yield return new()
			{
				Index = i,
				InterimGrid = interimGrid,
				InterimStep = interimStep,
				DisplayItems = displayItems,
				ShowHodokuDifficulty = showHodoku,
				ShowSudokuExplainerDifficulty = showSudokuExplainer
			};
		}
	}
}
