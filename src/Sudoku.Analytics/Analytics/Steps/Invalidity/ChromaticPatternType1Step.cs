namespace Sudoku.Analytics.Steps.Invalidity;

/// <summary>
/// Provides with a step that is a <b>Chromatic Pattern Type 1</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="blocks"><inheritdoc cref="ChromaticPatternStep.Blocks" path="/summary"/></param>
/// <param name="pattern"><inheritdoc cref="ChromaticPatternStep.Pattern" path="/summary"/></param>
/// <param name="extraCell"><inheritdoc cref="ExtraCell" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="ChromaticPatternStep.DigitsMask" path="/summary"/></param>
public sealed class ChromaticPatternType1Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	House[] blocks,
	in CellMap pattern,
	Cell extraCell,
	Mask digitsMask
) : ChromaticPatternStep(conclusions, views, options, blocks, pattern, digitsMask)
{
	/// <inheritdoc/>
	public override Technique Code => Technique.ChromaticPatternType1;

	/// <inheritdoc/>
	public override Mask DigitsUsed => DigitsMask;

	/// <summary>
	/// Indicates the extra cell used.
	/// </summary>
	public Cell ExtraCell { get; } = extraCell;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [CellsStr, BlocksStr, DigitsStr]), new(SR.ChineseLanguage, [BlocksStr, CellsStr, DigitsStr])];
}
