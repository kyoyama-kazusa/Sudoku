namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Qiu's Deadly Pattern External Type</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="is2LinesWith2Cells"><inheritdoc cref="QiuDeadlyPatternStep.Is2LinesWith2Cells" path="/summary"/></param>
/// <param name="houses"><inheritdoc cref="QiuDeadlyPatternStep.Houses" path="/summary"/></param>
/// <param name="corner1"><inheritdoc cref="QiuDeadlyPatternStep.Corner1" path="/summary"/></param>
/// <param name="corner2"><inheritdoc cref="QiuDeadlyPatternStep.Corner2" path="/summary"/></param>
public abstract class QiuDeadlyPatternExternalTypeStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	bool is2LinesWith2Cells,
	HouseMask houses,
	Cell? corner1,
	Cell? corner2
) : QiuDeadlyPatternStep(conclusions, views, options, is2LinesWith2Cells, houses, corner1, corner2)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 2;

	/// <inheritdoc/>
	public sealed override Technique Code
		=> Type switch
		{
			>= 1 and <= 4 => Technique.Parse($"QiuDeadlyPatternExternalType{Type}"),
			5 => Technique.LockedQiuDeadlyPattern,
			6 => Technique.QiuDeadlyPatternExternalType1,
			7 => Technique.QiuDeadlyPatternExternalType2
		};
}
