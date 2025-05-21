namespace Sudoku.Analytics.Steps.Uniqueness;

/// <summary>
/// Provides with a step that is a <b>Unique Rectangle Conjugate Pair(s)</b> (a.k.a. <b>Unique Rectangle Strong Link</b>) technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="code"><inheritdoc/></param>
/// <param name="digit1"><inheritdoc/></param>
/// <param name="digit2"><inheritdoc/></param>
/// <param name="cells"><inheritdoc/></param>
/// <param name="isAvoidable"><inheritdoc/></param>
/// <param name="conjugatePairs">Indicates the conjugate pairs used.</param>
/// <param name="absoluteOffset"><inheritdoc/></param>
public partial class UniqueRectangleConjugatePairStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Technique code,
	Digit digit1,
	Digit digit2,
	in CellMap cells,
	bool isAvoidable,
	[Property] Conjugate[] conjugatePairs,
	int absoluteOffset
) :
	UniqueRectangleStep(conclusions, views, options, code, digit1, digit2, cells, isAvoidable, absoluteOffset),
	IConjugatePairTrait
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty - 1;

	/// <inheritdoc/>
	public sealed override int Type
		=> Code switch { Technique.UniqueRectangleType4 => 4, Technique.UniqueRectangleType6 => 6, _ => base.Type };

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [D1Str, D2Str, CellsStr, Prefix, Suffix, ConjPairsStr]),
			new(SR.ChineseLanguage, [D1Str, D2Str, CellsStr, ConjPairsStr])
		];

	/// <inheritdoc/>
	public sealed override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_RectangleConjugatePairsCountFactor",
				[nameof(IConjugatePairTrait.ConjugatePairsCount)],
				GetType(),
				static args => OeisSequences.A004526((int)args![0]! + 2)
			),
			Factor.Create(
				"Factor_RectangleIsAvoidableFactor",
				[nameof(IsAvoidable)],
				GetType(),
				static args => (bool)args![0]! ? 1 : 0
			)
		];

	/// <inheritdoc/>
	int IConjugatePairTrait.ConjugatePairsCount => ConjugatePairs.Length;

	private string ConjPairsStr => Options.Converter.ConjugateConverter(ConjugatePairs);

	private string Prefix => ConjugatePairs.Length == 1 ? "a " : string.Empty;

	private string Suffix => ConjugatePairs.Length == 1 ? string.Empty : "s";
}
