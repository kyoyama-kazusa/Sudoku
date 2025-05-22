namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Uniqueness Clue Cover</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="extraCells"><inheritdoc cref="ExtraCells" path="/summary"/></param>
/// <param name="extraDigits"><inheritdoc cref="ExtraDigits" path="/summary"/></param>
/// <param name="chuteIndex"><inheritdoc cref="ChuteIndex" path="/summary"/></param>
public sealed class UniquenessClueCoverStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in CellMap extraCells,
	Mask extraDigits,
	int chuteIndex
) :
	UnconditionalDeadlyPatternStep(conclusions, views, options),
	IExtraCellListTrait
{
	/// <inheritdoc/>
	public override bool OnlyUseBivalueCells => false;

	/// <inheritdoc/>
	public override int BaseDifficulty => 65;

	/// <summary>
	/// Indicates the chute index.
	/// </summary>
	public int ChuteIndex { get; } = chuteIndex;

	/// <inheritdoc/>
	public override Technique Code => Technique.UniquenessClueCover;

	/// <inheritdoc/>
	public override Mask DigitsUsed => ExtraDigits;

	/// <summary>
	/// Indicates the extra cells.
	/// </summary>
	public CellMap ExtraCells { get; } = extraCells;

	/// <summary>
	/// Indicates the extra digits.
	/// </summary>
	public Mask ExtraDigits { get; } = extraDigits;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [ChuteString, ChuteCellsString]),
			new(SR.ChineseLanguage, [ChuteString, ChuteCellsString]),
		];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_UniquenessClueCoverExtraCellsFactor",
				[nameof(IExtraCellListTrait.ExtraCellSize)],
				GetType(),
				static args => OeisSequences.A004526((int)args![0]!)
			)
		];

	/// <inheritdoc/>
	int IExtraCellListTrait.ExtraCellSize => ExtraCells.Count;

	private string ChuteString => Options.Converter.ChuteConverter([Chutes[ChuteIndex]]);

	private string ChuteCellsString => Options.Converter.CellConverter(in Chutes[ChuteIndex].Cells);
}
