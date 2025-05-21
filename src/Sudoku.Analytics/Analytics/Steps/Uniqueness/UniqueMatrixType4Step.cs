namespace Sudoku.Analytics.Steps.Uniqueness;

/// <summary>
/// Provides with a step that is a <b>Unique Square Type 4</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cells"><inheritdoc/></param>
/// <param name="digitsMask"><inheritdoc/></param>
/// <param name="digit1">Indicates the first digit used in the conjugate.</param>
/// <param name="digit2">Indicates the second digit used in the conjugate.</param>
/// <param name="conjugateHouse">Indicates the cells that describes the generalized conjugate pair.</param>
public sealed partial class UniqueMatrixType4Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in CellMap cells,
	Mask digitsMask,
	[Property] Digit digit1,
	[Property] Digit digit2,
	[Property] in CellMap conjugateHouse
) : UniqueMatrixStep(conclusions, views, options, cells, digitsMask)
{
	/// <inheritdoc/>
	public override int Type => 4;

	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 2;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [DigitsStr, CellsStr, ConjStr, Digit1Str, Digit2Str]),
			new(SR.ChineseLanguage, [ConjStr, Digit1Str, Digit2Str, DigitsStr, CellsStr])
		];

	private string ConjStr => Options.Converter.CellConverter(ConjugateHouse);

	private string Digit1Str => Options.Converter.DigitConverter((Mask)(1 << Digit1));

	private string Digit2Str => Options.Converter.DigitConverter((Mask)(1 << Digit2));
}
