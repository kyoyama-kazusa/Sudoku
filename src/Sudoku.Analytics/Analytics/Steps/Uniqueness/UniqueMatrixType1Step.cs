namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Matrix Type 1</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="UniqueMatrixStep.Cells" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="UniqueMatrixStep.DigitsMask" path="/summary"/></param>
/// <param name="candidate"><inheritdoc cref="Candidate" path="/summary"/></param>
public sealed class UniqueMatrixType1Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in CellMap cells,
	Mask digitsMask,
	Candidate candidate
) : UniqueMatrixStep(conclusions, views, options, cells, digitsMask)
{
	/// <inheritdoc/>
	public override int Type => 1;

	/// <summary>
	/// Indicates the true candidate.
	/// </summary>
	public Candidate Candidate { get; } = candidate;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [DigitsStr, CellsStr, CandidateStr]), new(SR.ChineseLanguage, [CandidateStr, CellsStr, DigitsStr])];

	private string CandidateStr => Options.Converter.CandidateConverter([Candidate]);
}
