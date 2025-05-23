namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Square Type 4</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="UniqueMatrixStep.Cells" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="UniqueMatrixStep.DigitsMask" path="/summary"/></param>
/// <param name="digit1"><inheritdoc cref="Digit1" path="/summary"/></param>
/// <param name="digit2"><inheritdoc cref="Digit2" path="/summary"/></param>
/// <param name="conjugateHouse"><inheritdoc cref="ConjugateHouse" path="/summary"/></param>
public sealed class UniqueMatrixType4Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in CellMap cells,
	Mask digitsMask,
	Digit digit1,
	Digit digit2,
	in CellMap conjugateHouse
) : UniqueMatrixStep(conclusions, views, options, cells, digitsMask)
{
	/// <inheritdoc/>
	public override int Type => 4;

	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 2;

	/// <summary>
	/// Indicates the first digit used in the conjugate.
	/// </summary>
	public Digit Digit1 { get; } = digit1;

	/// <summary>
	/// Indicates the second digit used in the conjugate.
	/// </summary>
	public Digit Digit2 { get; } = digit2;

	/// <summary>
	/// Indicates the cells that describes the generalized conjugate pair.
	/// </summary>
	public CellMap ConjugateHouse { get; } = conjugateHouse;

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
