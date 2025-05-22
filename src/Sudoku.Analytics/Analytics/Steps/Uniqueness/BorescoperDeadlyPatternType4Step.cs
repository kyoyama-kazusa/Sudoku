namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Borescoper's Deadly Pattern Type 4</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="BorescoperDeadlyPatternStep.Cells" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="BorescoperDeadlyPatternStep.DigitsMask" path="/summary"/></param>
/// <param name="conjugateHouse"><inheritdoc cref="ConjugateHouse" path="/summary"/></param>
/// <param name="extraDigitsMask"><inheritdoc cref="ExtraDigitsMask" path="/summary"/></param>
public sealed class BorescoperDeadlyPatternType4Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in CellMap cells,
	Mask digitsMask,
	in CellMap conjugateHouse,
	Mask extraDigitsMask
) : BorescoperDeadlyPatternStep(conclusions, views, options, cells, digitsMask)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 2;

	/// <inheritdoc/>
	public override int Type => 4;

	/// <summary>
	/// Indicates the cells used as generalized conjugate.
	/// </summary>
	public CellMap ConjugateHouse { get; } = conjugateHouse;

	/// <summary>
	/// Indicates the mask of extra digits used.
	/// </summary>
	public Mask ExtraDigitsMask { get; } = extraDigitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [DigitsStr, CellsStr, ConjHouseStr, ExtraCombStr]),
			new(SR.ChineseLanguage, [DigitsStr, CellsStr, ExtraCombStr, ConjHouseStr])
		];

	private string ExtraCombStr => Options.Converter.DigitConverter(ExtraDigitsMask);

	private string ConjHouseStr => Options.Converter.CellConverter(ConjugateHouse);
}
