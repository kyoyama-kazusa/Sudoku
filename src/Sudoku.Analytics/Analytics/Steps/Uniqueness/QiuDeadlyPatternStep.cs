namespace Sudoku.Analytics.Steps.Uniqueness;

/// <summary>
/// Provides with a step that is a <b>Qiu's Deadly Pattern</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc/></param>
/// <param name="views"><inheritdoc/></param>
/// <param name="options"><inheritdoc/></param>
/// <param name="is2LinesWith2Cells">
/// Indicates whether the pattern contains 2 lines and 2 cells. If not, the pattern should be 2 rows and 2 columns intersected.
/// </param>
/// <param name="houses">Indicates all houses used in the pattern.</param>
/// <param name="corner1">
/// Indicates the corner cell 1. The value can be <see langword="null"/> if <paramref name="is2LinesWith2Cells"/> is <see langword="false"/>.
/// </param>
/// <param name="corner2">
/// Indicates the corner cell 2. The value can be <see langword="null"/> if <paramref name="is2LinesWith2Cells"/> is <see langword="false"/>.
/// </param>
public abstract partial class QiuDeadlyPatternStep(
	StepConclusions conclusions,
	View[]? views,
	StepGathererOptions options,
	[Property] bool is2LinesWith2Cells,
	[Property] HouseMask houses,
	[Property] Cell? corner1,
	[Property] Cell? corner2
) : UnconditionalDeadlyPatternStep(conclusions, views, options), IDeadlyPatternTypeTrait
{
	/// <inheritdoc/>
	public override bool OnlyUseBivalueCells => false;

	/// <inheritdoc/>
	public override int BaseDifficulty => 58;

	/// <inheritdoc/>
	public abstract int Type { get; }

	/// <inheritdoc/>
	public override Technique Code => Technique.Parse($"QiuDeadlyPatternType{Type}");

	private protected string PatternStr => Options.Converter.CellConverter(Pattern);

	/// <summary>
	/// Indicates the internal pattern.
	/// </summary>
	private CellMap Pattern
	{
		get
		{
			var result = CellMap.Empty;
			foreach (var house in Houses)
			{
				result |= HousesMap[house];
			}
			return result;
		}
	}
}
