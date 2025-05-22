namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Rectangle Conjugate Pair(s) Extra</b>
/// (a.k.a. <b>Unique Rectangle Strong Link Extra</b>) technique. Especially for UR + 3/1SL, UR + 4/1SL and UR + 4/2SL.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="code"><inheritdoc/></param>
/// <param name="digit1"><inheritdoc/></param>
/// <param name="digit2"><inheritdoc/></param>
/// <param name="cells"><inheritdoc/></param>
/// <param name="isAvoidable"><inheritdoc/></param>
/// <param name="conjugatePairs"><inheritdoc/></param>
/// <param name="extraCells">Indicates the extra cells used.</param>
/// <param name="extraDigitsMask">Indicates the extra digits used.</param>
/// <param name="absoluteOffset"><inheritdoc/></param>
public sealed partial class UniqueRectangleConjugatePairExtraStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Technique code,
	Digit digit1,
	Digit digit2,
	in CellMap cells,
	bool isAvoidable,
	Conjugate[] conjugatePairs,
	[Property] in CellMap extraCells,
	[Property] Mask extraDigitsMask,
	int absoluteOffset
) :
	UniqueRectangleConjugatePairStep(
		conclusions,
		views,
		options,
		code,
		digit1,
		digit2,
		cells,
		isAvoidable,
		conjugatePairs,
		absoluteOffset
	),
	IConjugatePairTrait
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 3;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(base.DigitsUsed | ExtraDigitsMask);

	/// <inheritdoc/>
	protected override bool TechniqueResourceKeyInheritsFromBase => true;

	/// <inheritdoc/>
	int IConjugatePairTrait.ConjugatePairsCount => ConjugatePairs.Length;
}
