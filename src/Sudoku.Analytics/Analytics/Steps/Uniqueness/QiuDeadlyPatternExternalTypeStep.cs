namespace Sudoku.Analytics.Steps.Uniqueness;

/// <summary>
/// Provides with a step that is a <b>Qiu's Deadly Pattern External Type</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc/></param>
/// <param name="views"><inheritdoc/></param>
/// <param name="options"><inheritdoc/></param>
/// <param name="is2LinesWith2Cells"><inheritdoc/></param>
/// <param name="houses"><inheritdoc/></param>
/// <param name="corner1"><inheritdoc/></param>
/// <param name="corner2"><inheritdoc/></param>
public abstract class QiuDeadlyPatternExternalTypeStep(
	StepConclusions conclusions,
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
