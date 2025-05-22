namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Qiu's Deadly Pattern</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="is2LinesWith2Cells"><inheritdoc cref="Is2LinesWith2Cells" path="/summary"/></param>
/// <param name="houses"><inheritdoc cref="Houses" path="/summary"/></param>
/// <param name="corner1"><inheritdoc cref="Corner1" path="/summary"/></param>
/// <param name="corner2"><inheritdoc cref="Corner2" path="/summary"/></param>
public abstract class QiuDeadlyPatternStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	bool is2LinesWith2Cells,
	HouseMask houses,
	Cell? corner1,
	Cell? corner2
) :
	UnconditionalDeadlyPatternStep(conclusions, views, options),
	IDeadlyPatternTypeTrait
{
	/// <inheritdoc/>
	public override bool OnlyUseBivalueCells => false;

	/// <summary>
	/// Indicates whether the pattern contains 2 lines and 2 cells. If not, the pattern should be 2 rows and 2 columns intersected.
	/// </summary>
	[MemberNotNullWhen(true, nameof(Corner1), nameof(Corner2))]
	public bool Is2LinesWith2Cells { get; } = is2LinesWith2Cells;

	/// <inheritdoc/>
	public override int BaseDifficulty => 58;

	/// <inheritdoc/>
	public abstract int Type { get; }

	/// <summary>
	/// Indicates all houses used in the pattern.
	/// </summary>
	public House Houses { get; } = houses;

	/// <summary>
	/// Indicates the corner cell 1.
	/// The value can be <see langword="null"/> if <see cref="Is2LinesWith2Cells"/> is <see langword="false"/>.
	/// </summary>
	public Cell? Corner1 { get; } = corner1;

	/// <summary>
	/// Indicates the corner cell 2.
	/// The value can be <see langword="null"/> if <see cref="Is2LinesWith2Cells"/> is <see langword="false"/>.
	/// </summary>
	public Cell? Corner2 { get; } = corner2;

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
