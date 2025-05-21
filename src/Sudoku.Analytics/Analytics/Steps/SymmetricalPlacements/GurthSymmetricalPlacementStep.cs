namespace Sudoku.Analytics.Steps.SymmetricalPlacements;

/// <summary>
/// Provides with a step that is a <b>Gurth's Symmetrical Placement</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="symmetricType">
/// Indicates the symmetric type used. The supported value can only be:
/// <list type="bullet">
/// <item><see cref="SymmetricType.Central"/></item>
/// <item><see cref="SymmetricType.Diagonal"/></item>
/// <item><see cref="SymmetricType.AntiDiagonal"/></item>
/// </list>
/// </param>
/// <param name="mapping">
/// Indicates the mapping relations; in other words, this table shows what digits has symmetrical placement relation to what digits.
/// </param>
public partial class GurthSymmetricalPlacementStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	[Property] SymmetricType symmetricType,
	[Property] Digit?[]? mapping
) : SymmetryStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public override int BaseDifficulty
		=> SymmetricType switch
		{
			SymmetricType.Central => 70,
			SymmetricType.Diagonal or SymmetricType.AntiDiagonal => 71,
			SymmetricType.XAxis or SymmetricType.YAxis => 72 // This pattern will only be used by anti-GSP cases.
		};

	/// <inheritdoc/>
	public override Technique Code => Technique.GurthSymmetricalPlacement;

	/// <inheritdoc/>
	public override Mask DigitsUsed => 0;

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
