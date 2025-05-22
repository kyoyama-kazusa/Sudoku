namespace Sudoku.Analytics.Steps.SymmetricalPlacements;

/// <summary>
/// Provides with a step that is an <b>Anti- Gurth's Symmetrical Placement</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="symmetricType"><inheritdoc cref="GurthSymmetricalPlacementStep.SymmetricType" path="/summary"/></param>
/// <param name="mapping"><inheritdoc cref="GurthSymmetricalPlacementStep.Mapping" path="/summary"/></param>
public sealed class AntiGurthSymmetricalPlacementStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	SymmetricType symmetricType,
	Digit?[]? mapping
) : GurthSymmetricalPlacementStep(conclusions, views, options, symmetricType, mapping)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 3;

	/// <inheritdoc/>
	public override Technique Code => Technique.AntiGurthSymmetricalPlacement;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [SymmetryTypeStr(SR.EnglishLanguage), MappingStr(SR.EnglishLanguage)]),
			new(SR.ChineseLanguage, [SymmetryTypeStr(SR.ChineseLanguage), MappingStr(SR.ChineseLanguage)])
		];


	private string MappingStr(string cultureName)
	{
		var culture = new CultureInfo(cultureName);
		var comma = SR.Get("Comma", culture);
		if (Mapping is not null)
		{
			var sb = new StringBuilder(10);
			for (var i = 0; i < 9; i++)
			{
				var currentMappingRelationDigit = Mapping[i];
				sb.Append(i + 1);
				sb.Append(currentMappingRelationDigit is { } c && c != i ? $" -> {c + 1}" : string.Empty);
				sb.Append(comma);
			}
			return sb.RemoveFrom(^comma.Length).ToString();
		}
		return SR.Get("NoMappingRelation", culture);
	}

	private string SymmetryTypeStr(string cultureName) => SR.Get($"{SymmetricType}Symmetry", new(cultureName));
}
