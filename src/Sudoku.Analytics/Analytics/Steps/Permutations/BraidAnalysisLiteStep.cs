namespace Sudoku.Analytics.Steps;

/// <summary>
/// Represents a data structure that describes for a technique of <b>Braid Analysis Lite</b>.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digitsUsed"><inheritdoc cref="DigitsUsed" path="/summary"/></param>
/// <param name="cells1"><inheritdoc cref="Cells" path="/summary"/></param>
/// <param name="cells2"><inheritdoc cref="Cells" path="/summary"/></param>
/// <param name="cells3"><inheritdoc cref="Cells" path="/summary"/></param>
/// <param name="invalidDigitsMask"><inheritdoc cref="InvalidDigitsMask" path="/summary"/></param>
public sealed class BraidAnalysisLiteStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Mask digitsUsed,
	in CellMap cells1,
	in CellMap cells2,
	in CellMap cells3,
	Mask invalidDigitsMask
) : PermutationStep(conclusions, views, options)
{
	/// <summary>
	/// <inheritdoc cref="Cells" path="/summary"/>
	/// </summary>
	private readonly CellMap _cells1 = cells1, _cells2 = cells2, _cells3 = cells3;


	/// <inheritdoc/>
	public override int BaseDifficulty => 60;

	/// <inheritdoc/>
	public override Technique Code => Technique.BraidAnalysisLite;

	/// <inheritdoc/>
	public override Mask DigitsUsed { get; } = digitsUsed;

	/// <summary>
	/// Indicates invalid digits mask.
	/// </summary>
	public Mask InvalidDigitsMask { get; } = invalidDigitsMask;

	/// <summary>
	/// Indicates all cells used.
	/// </summary>
	public ReadOnlySpan<CellMap> Cells => (CellMap[])[_cells1, _cells2, _cells3];

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [DigitsStr, CellsStr, InvalidDigitsStr]),
			new(SR.ChineseLanguage, [DigitsStr, CellsStr, InvalidDigitsStr])
		];

	private string DigitsStr => Options.Converter.DigitConverter(DigitsUsed);

	private string InvalidDigitsStr => Options.Converter.DigitConverter(InvalidDigitsMask);

	private string CellsStr => Options.Converter.CellConverter(_cells1 | _cells2 | _cells3);
}
