namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Uniqueness Clue Cover</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="extraCells">Indicates the extra cells.</param>
/// <param name="extraDigits">Indicates the extra digits.</param>
/// <param name="chuteIndex">Indicates the chute index.</param>
public sealed partial class UniquenessClueCoverStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	[Property] in CellMap extraCells,
	[Property] Mask extraDigits,
	[Property] int chuteIndex
) : UnconditionalDeadlyPatternStep(conclusions, views, options), IExtraCellListTrait
{
	/// <inheritdoc/>
	public override bool OnlyUseBivalueCells => false;

	/// <inheritdoc/>
	public override int BaseDifficulty => 65;

	/// <inheritdoc/>
	public override Technique Code => Technique.UniquenessClueCover;

	/// <inheritdoc/>
	public override Mask DigitsUsed => ExtraDigits;

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
