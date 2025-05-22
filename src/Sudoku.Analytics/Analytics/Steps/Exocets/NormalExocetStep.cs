namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is an <b>Exocet (Base)</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="ExocetStep.DigitsMask" path="/summary"/></param>
/// <param name="baseCells"><inheritdoc cref="ExocetStep.BaseCells" path="/summary"/></param>
/// <param name="targetCells"><inheritdoc cref="ExocetStep.TargetCells" path="/summary"/></param>
/// <param name="endoTargetCells"><inheritdoc cref="ExocetStep.EndoTargetCells" path="/summary"/></param>
/// <param name="crosslineCells"><inheritdoc cref="ExocetStep.CrosslineCells" path="/summary"/></param>
/// <param name="conjugatePairs"><inheritdoc cref="ConjugatePairs" path="/summary"/></param>
public sealed class NormalExocetStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Mask digitsMask,
	in CellMap baseCells,
	in CellMap targetCells,
	in CellMap endoTargetCells,
	in CellMap crosslineCells,
	Conjugate[] conjugatePairs
) :
	ExocetStep(conclusions, views, options, digitsMask, baseCells, targetCells, endoTargetCells, crosslineCells),
	IConjugatePairTrait
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + (Code == Technique.SeniorExocet ? 2 : 0);

	/// <inheritdoc/>
	public override Technique Code
		=> (Delta, ConjugatePairs) switch
		{
			( < 0, _) => Technique.SeniorExocet,
			(_, []) => Technique.JuniorExocet,
			_ => Technique.JuniorExocetConjugatePair
		};

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(base.DigitsUsed | Mask.Create(from c in ConjugatePairs select c.Digit));

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_ExocetConjugatePairsCountFactor",
				[nameof(IConjugatePairTrait.ConjugatePairsCount)],
				GetType(),
				static args => OeisSequences.A004526((int)args![0]!)
			)
		];

	/// <summary>
	/// Indicates the conjugate pairs used in target.
	/// </summary>
	public Conjugate[] ConjugatePairs { get; } = conjugatePairs;

	/// <inheritdoc/>
	int IConjugatePairTrait.ConjugatePairsCount => ConjugatePairs.Length;
}
