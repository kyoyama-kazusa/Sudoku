namespace SudokuStudio.Interaction.Conversions;

/// <summary>
/// Provides with conversion methods used by XAML designer, about <see cref="StepSearcherListView"/> instances.
/// </summary>
/// <seealso cref="StepSearcherListView"/>
internal static class StepSearcherListViewConversion
{
	public static object? GetStepSearcherSupportedDifficultyLevelCollection(StepSearcherInfo? info)
		=> info is null ? null : GetMatchedStepSearcher(info).Metadata.DifficultyLevelRange.ToArray();

	public static object? GetStepSearcherSupportedTechniqueCollection(StepSearcherInfo? info)
		=> info is null ? null : from t in GetMatchedStepSearcher(info).Metadata.SupportedTechniques orderby t.DifficultyLevel select t;

	public static string GetStepSearcherName(StepSearcherInfo? info)
		=> info is null ? string.Empty : GetMatchedStepSearcher(info).Metadata.GetName(App.CurrentCulture);

	public static string GetTechniqueName(Technique technique) => technique.GetName(App.CurrentCulture);

	public static Visibility GetDisplayerVisibility(StepSearcherInfo? info) => info is null ? Visibility.Collapsed : Visibility.Visible;

	public static Brush GetTechniqueForeground(Technique technique)
		=> DifficultyLevelConversion.GetForegroundColor(technique.DifficultyLevel);

	public static Brush GetTechniqueBackground(Technique technique)
		=> DifficultyLevelConversion.GetBackgroundColor(technique.DifficultyLevel);

	private static StepSearcher GetMatchedStepSearcher(StepSearcherInfo info) => StepSearcherFactory.GetStepSearcher(info.TypeName);
}
