namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Loop Type 1</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit1"><inheritdoc cref="UniqueLoopStep.Digit1" path="/summary"/></param>
/// <param name="digit2"><inheritdoc cref="UniqueLoopStep.Digit2" path="/summary"/></param>
/// <param name="loop"><inheritdoc cref="UniqueLoopStep.Loop" path="/summary"/></param>
/// <param name="loopPath"><inheritdoc cref="UniqueLoopStep.LoopPath" path="/summary"/></param>
public sealed class UniqueLoopType1Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit1,
	Digit digit2,
	in CellMap loop,
	Cell[] loopPath
) : UniqueLoopStep(conclusions, views, options, digit1, digit2, loop, loopPath)
{
	/// <inheritdoc/>
	public override int Type => 1;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [Digit1Str, Digit2Str, LoopStr]), new(SR.ChineseLanguage, [Digit1Str, Digit2Str, LoopStr])];
}
