namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Brute Force</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
public sealed class BruteForceStep(ReadOnlyMemory<Conclusion> conclusions, View[]? views, StepGathererOptions options) :
	LastResortStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => AnalysisResult.MaximumRatingValueTheory;

	/// <inheritdoc/>
	public override Technique Code => Technique.BruteForce;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(1 << Conclusions.Span[0].Digit);

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [AssignmentStr]), new(SR.ChineseLanguage, [AssignmentStr])];

	private string AssignmentStr => Options.Converter.ConclusionConverter(Conclusions.Span);
}
